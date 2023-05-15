using Microsoft.Extensions.Hosting;

namespace X;

public abstract class XrmqRawConsumerBackgroundService : BackgroundService
{
    private readonly IXrmq xrmq;
    private readonly string queue;

    public XrmqRawConsumerBackgroundService(IXrmq xrmq, string queue)
    {
        this.xrmq = xrmq;
        this.queue = queue;
    }

    protected abstract Task Handle(byte[] message);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await xrmq.Consume(queue, Handle);
        
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
        }
    }
}
