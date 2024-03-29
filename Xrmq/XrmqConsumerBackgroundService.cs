using Microsoft.Extensions.Hosting;

namespace X;

public abstract class XrmqConsumerBackgroundService<T> : BackgroundService
{
    private readonly IXrmq xrmq;
    private readonly string queue;

    public XrmqConsumerBackgroundService(IXrmq xrmq, string queue)
    {
        this.xrmq = xrmq;
        this.queue = queue;
    }

    protected abstract Task Handle(T message);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await xrmq.Consume<T>(queue, Handle);
        
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
        }
    }
}
