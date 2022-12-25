using Microsoft.Extensions.Hosting;

namespace X
{
    public abstract class XrmqConsumerBackgroundService<T> : BackgroundService
    {
        private readonly IXrmq xrmq;

        public XrmqConsumerBackgroundService(IXrmq xrmq)
        {
            this.xrmq = xrmq;
        }

        protected abstract string GetQueue();
        
        protected abstract void Handle(T message);

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            xrmq.Consume<T>(GetQueue(), Handle);
            
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
            }
        }
    }
}