using RabbitMQ.Client;
using Microsoft.Extensions.Logging;

namespace X
{
    public class Producer
    {
        private readonly ILogger<Producer> logger;
        private readonly IConnectionFactory connectionFactory;
        private IConnection? connection;
        private Dictionary<Int32, IModel> channelPool;

        public Producer (ILogger<Producer> logger, IConnectionFactory connectionFactory)
        {
            this.logger = logger;
            this.connectionFactory = connectionFactory;
            this.connection = null;
            this.channelPool = new Dictionary<Int32, IModel>();
        }

        public void Produce (string queue, byte[] message)
        {
            var channel = GetChannel();
            channel.BasicPublish(string.Empty, queue, null, message);
        }

        private IConnection GetConnection()
        {
            if (this.connection == null || !this.connection.IsOpen)
            {
                this.connection = connectionFactory.CreateConnection();
            }

            return this.connection;
        }

        private IModel GetChannel()
        {
            var threadId = Thread.GetCurrentProcessorId();

            var channel = this.channelPool[threadId];

            if (channel == null || !channel.IsOpen)
            {
                channel = GetConnection().CreateModel();
                this.channelPool[threadId] = channel;
            }

            return channel;
        }
    }
}
