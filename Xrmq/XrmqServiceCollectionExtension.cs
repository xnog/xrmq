using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Client;

namespace X;

public static class XrmqServiceCollectionExtension
{
    public static IServiceCollection AddRabbitMQ(this IServiceCollection services, XrmqProperties properties)
    {
        services.AddSingleton<IXrmq, Xrmq>();
        services.AddSingleton<XrmqProperties>(p => properties);
        services.AddSingleton<IPooledObjectPolicy<IModel>, ChannelPooledObjectPolicy>();
        return services;
    }
}
