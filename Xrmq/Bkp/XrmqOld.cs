using RabbitMQ.Client;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;

namespace XOld
{
    public class XrmqOld
    {
        private readonly ILogger<XrmqOld> logger;
        private readonly IConnectionFactory connectionFactory;
        private IConnection? connection;
        private Dictionary<Int32, IModel> channelPool;
        private Dictionary<string, RetryProperties> retryPropertiesMap;

        public XrmqOld(ILogger<XrmqOld> logger, IConnectionFactory connectionFactory)
        {
            this.logger = logger;
            this.connectionFactory = connectionFactory;
            this.connection = null;
            this.channelPool = new Dictionary<Int32, IModel>();
            this.retryPropertiesMap = new Dictionary<string, RetryProperties>();
        }

        public void QueueDeclareWithRetry(string queue, RetryProperties? retryProperties)
        {
            var channel = GetChannel();

            channel.ExchangeDeclare("dlx", "direct", true, false);
            channel.ExchangeDeclare("rx", "direct", true, false);

            channel.QueueDeclare($"{queue}.dlq", true, false, false);
            channel.QueueDeclare($"{queue}.rq", true, false, false, new Dictionary<string, object> {
                { "x-dead-letter-exchange", "dlx" },
                { "x-dead-letter-routing-key", $"{queue}" }
            });
            channel.QueueDeclare($"{queue}", true, false, false, new Dictionary<string, object> {
                { "x-dead-letter-exchange", "dlx" },
                { "x-dead-letter-routing-key", $"{queue}.dlq" }
            });

            channel.QueueBind($"{queue}", "dlx", $"{queue}");
            channel.QueueBind($"{queue}.dlq", "dlx", $"{queue}.dlq");
            channel.QueueBind($"{queue}.rq", "rx", $"{queue}");

            this.retryPropertiesMap[queue] = retryProperties ?? new RetryProperties();
        }

        public void Publish(string exchange, string routingKey, IBasicProperties? basicProperties, ReadOnlyMemory<byte> body)
        {
            var channel = GetChannel();
            channel.BasicPublish(exchange, routingKey, basicProperties, body);
        }

        public void ConsumeWithRetry(string queue, EventHandler<BasicDeliverEventArgs> onReceive, ConsumeProperties? consumeProperties)
        {
            var channel = GetChannel();
            consumeProperties = consumeProperties ?? new ConsumeProperties();
            channel.BasicQos(0, consumeProperties.PrefetchCount, false);
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, evt) =>
            {
                try
                {
                    onReceive(model, evt);
                    channel.BasicAck(evt.DeliveryTag, false);
                }
                catch (Exception e)
                {
                    evt.BasicProperties.Headers = evt.BasicProperties.Headers ?? new Dictionary<string, object>();
                    evt.BasicProperties.Headers.TryGetValue("x-attempt", out var attempt);
                    int.TryParse(attempt?.ToString() ?? "0", out var currentAttempt);
                    var nextAttempt = currentAttempt + 1;
                    evt.BasicProperties.Headers["x-attempt"] = nextAttempt;
                    evt.BasicProperties.Headers["x-exception-message"] = e.Message;
                    evt.BasicProperties.Headers["x-exception-stacktrace"] = e.ToString();
                    evt.BasicProperties.Headers["x-date"] = DateTime.UtcNow.ToString("s");

                    var retryProperties = this.retryPropertiesMap[queue];
                    if (retryProperties.NumberOfRetries < nextAttempt)
                    {
                        channel.BasicReject(evt.DeliveryTag, false);
                    }
                    else
                    {
                        evt.BasicProperties.Expiration = retryProperties.RetryDelayMs.TotalMilliseconds.ToString();
                        Publish("rx", queue, evt.BasicProperties, evt.Body);
                        channel.BasicAck(evt.DeliveryTag, false);
                    }
                }
            };
            channel.BasicConsume(queue, false, consumer);
        }

        public IConnection GetConnection()
        {
            var threadId = Thread.GetCurrentProcessorId();

            if (this.connection == null || !this.connection.IsOpen)
            {
                this.connection = connectionFactory.CreateConnection();
            }

            return this.connection;
        }

        public IModel GetChannel()
        {
            var threadId = Thread.GetCurrentProcessorId();

            var channel = default(IModel);

            if (this.channelPool.ContainsKey(threadId))
            {
                channel = this.channelPool[threadId];
            }

            if (channel == null || !channel.IsOpen)
            {
                channel = GetConnection().CreateModel();
                this.channelPool[threadId] = channel;
            }

            return channel;
        }

        public bool IsConnected()
        {
            return this.connection != null && this.connection.IsOpen;
        }
    }

    public class RetryProperties
    {
        public int NumberOfRetries { get; set; } = 5;
        public TimeSpan RetryDelayMs { get; set; } = TimeSpan.FromSeconds(10);
    }

    public class ConsumeProperties
    {
        public ushort PrefetchCount { get; set; } = 20;
    }
}
