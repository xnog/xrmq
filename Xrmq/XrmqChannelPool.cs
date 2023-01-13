using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Client;

namespace X;

public class ChannelPooledObjectPolicy : IPooledObjectPolicy<IModel>
{
    private readonly XrmqProperties properties;
    private readonly ConnectionFactory connectionFactory;
    private IConnection connection;

    public ChannelPooledObjectPolicy(XrmqProperties properties)
    {
        this.properties = properties;
        this.connectionFactory = new ConnectionFactory() {
            UserName = this.properties.UserName,
            Password = this.properties.Password,
            VirtualHost = this.properties.VHost,
            HostName = this.properties.HostName,
            Port = this.properties.Port,
        };
        if(this.properties.Ssl) {
            this.connectionFactory.Ssl = new SslOption() {
                ServerName = this.properties.HostName,
                Enabled = this.properties.Ssl,
            };
        }
        this.connection = GetConnection();
    }

    public IModel Create()
    {
        var channel = GetConnection().CreateModel();
        channel.BasicQos(0, this.properties.PrefetchCount, false);
        if (this.properties.WaitForConfirm)
        {
            channel.ConfirmSelect();
        }
        return channel;
    }

    public bool Return(IModel obj)
    {
        if (obj.IsOpen)
        {
            return true;
        }
        else
        {
            obj?.Dispose();
            return false;
        }
    }

    private IConnection GetConnection()
    {
        if (this.connection == null || !this.connection.IsOpen)
        {
            this.connection = this.connectionFactory.CreateConnection();
        }

        return this.connection;
    }
}

public class PoolObject<T> : IDisposable where T : class
{
    private DefaultObjectPool<T> pool;
    public T Item { get; set; }

    public PoolObject(DefaultObjectPool<T> pool)
    {
        this.pool = pool;
        this.Item = this.pool.Get();
    }

    public void Dispose()
    {
        this.pool.Return(this.Item);
    }
}
