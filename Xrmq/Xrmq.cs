using RabbitMQ.Client;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.ObjectPool;
using System.Text;
using System.Text.Json;

namespace X;

public interface IXrmq
{
    public Task QueueBind(string queue, string exchange, string routingKey, IDictionary<string, object> arguments);
    public Task ExchangeBind(string destination, string source, string routingKey, IDictionary<string, object> arguments);
    public Task QueueDeclare(string queue);
    public Task ExchangeDeclare(string exchange, string type, bool durable, bool autoDelete, IDictionary<string, object> arguments);
    public Task Publish(string exchange, string routingKey, IBasicProperties basicProperties, byte[] body);
    public Task Publish<T>(string exchange, string routingKey, IBasicProperties basicProperties, T message);
    public Task Consume(string queue, Action<byte[]> onReceive);
    public Task Consume<T>(string queue, Action<T> onReceive);
}

public class Xrmq : IXrmq
{
    private readonly ILogger<Xrmq> logger;
    private readonly XrmqProperties properties;
    private DefaultObjectPool<IModel> channelPool;

    public Xrmq(ILogger<Xrmq> logger, IPooledObjectPolicy<IModel> objectPolicy, XrmqProperties properties)
    {
        this.logger = logger;
        this.properties = properties;
        this.channelPool = new DefaultObjectPool<IModel>(objectPolicy, properties.MaxPoolSize);
    }

    public Task QueueBind(string queue, string exchange, string routingKey, IDictionary<string, object> arguments)
    {
        return Task.Run(() => {
            using var channel = new PoolObject<IModel>(this.channelPool);
            channel.Item.QueueBind(queue, exchange, routingKey, arguments);
        });
    }

    public Task ExchangeBind(string destination, string source, string routingKey, IDictionary<string, object> arguments)
    {
        return Task.Run(() => {
            using var channel = new PoolObject<IModel>(this.channelPool);
            channel.Item.ExchangeBind(destination, source, routingKey, arguments);
        });
    }

    public Task QueueDeclare(string queue)
    {
        return Task.Run(() => {
            using var channel = new PoolObject<IModel>(this.channelPool);
            channel.Item.ExchangeDeclare("dlx", "direct", true, false);
            channel.Item.ExchangeDeclare("rx", "direct", true, false);
            channel.Item.QueueDeclare($"{queue}.dlq", true, false, false);
            channel.Item.QueueDeclare($"{queue}.rq", true, false, false, new Dictionary<string, object> {
                { "x-dead-letter-exchange", "dlx" },
                { "x-dead-letter-routing-key", $"{queue}" }
            });
            channel.Item.QueueDeclare($"{queue}", true, false, false, new Dictionary<string, object> {
                { "x-dead-letter-exchange", "dlx" },
                { "x-dead-letter-routing-key", $"{queue}.dlq" }
            });
            channel.Item.QueueBind($"{queue}", "dlx", $"{queue}");
            channel.Item.QueueBind($"{queue}.dlq", "dlx", $"{queue}.dlq");
            channel.Item.QueueBind($"{queue}.rq", "rx", $"{queue}");
        });
    }

    public Task ExchangeDeclare(string exchange, string type, bool durable, bool autoDelete, IDictionary<string, object> arguments)
    {
        return Task.Run(() => {
            using var channel = new PoolObject<IModel>(this.channelPool);
            channel.Item.ExchangeDeclare(exchange, type, durable, autoDelete, arguments);
        });
    }

    public Task Publish(string exchange, string routingKey, IBasicProperties basicProperties, byte[] body)
    {
        return Task.Run(() => {
            using var channel = new PoolObject<IModel>(this.channelPool);
            channel.Item.BasicPublish(exchange, routingKey, basicProperties, body);
            if (this.properties.WaitForConfirm)
            {
                channel.Item.WaitForConfirmsOrDie(this.properties.WaitForConfirmTimeout);
            }
        });
    }

    public Task Publish<T>(string exchange, string routingKey, IBasicProperties basicProperties, T message)
    {
        return Task.Run(() => {
            if (message == null)
            {
                return;
            }

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            Publish(exchange, routingKey, basicProperties, body);
        });
    }

    public Task Consume(string queue, Action<byte[]> onReceive)
    {
        return Task.Run(() => {
            var channel = new PoolObject<IModel>(this.channelPool);
            var consumer = new EventingBasicConsumer(channel.Item);
            consumer.Received += async (model, evt) =>
            {
                try
                {
                    onReceive(evt.Body.ToArray());
                    channel.Item.BasicAck(evt.DeliveryTag, false);
                }
                catch (Exception e)
                {
                    try
                    {
                        evt.BasicProperties.Headers = evt.BasicProperties.Headers ?? new Dictionary<string, object>();
                        evt.BasicProperties.Headers.TryGetValue("x-attempt", out var attempt);
                        int.TryParse(attempt?.ToString() ?? "0", out var currentAttempt);
                        var nextAttempt = currentAttempt + 1;
                        evt.BasicProperties.Headers["x-attempt"] = nextAttempt;
                        evt.BasicProperties.Headers["x-exception-message"] = e.Message;
                        evt.BasicProperties.Headers["x-exception-stacktrace"] = e.ToString();
                        evt.BasicProperties.Headers["x-date"] = DateTime.UtcNow.ToString("s");

                        if (this.properties.NumberOfRetries < nextAttempt)
                        {
                            channel.Item.BasicReject(evt.DeliveryTag, false);
                        }
                        else
                        {
                            evt.BasicProperties.Expiration = this.properties.RetryDelayMs.TotalMilliseconds.ToString();
                            await Publish("rx", queue, evt.BasicProperties, evt.Body.ToArray());
                            channel.Item.BasicAck(evt.DeliveryTag, false);
                        }
                    }
                    catch
                    {
                        channel.Item.BasicReject(evt.DeliveryTag, false);
                    }
                }
            };
            channel.Item.BasicConsume(queue, false, consumer);
        });
    }

    public Task Consume<T>(string queue, Action<T> onReceive)
    {
        return Consume(queue, (byte[] message) => {
            var obj = JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(message));
            onReceive(obj!);
        });
    }
}
