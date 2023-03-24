using System.Text;
using X;

namespace Api
{
    public class ProgramRawConsumer : XrmqConsumerBackgroundService<byte[]>
    {
        private readonly ILogger<ProgramRawConsumer> logger;

        public ProgramRawConsumer(ILogger<ProgramRawConsumer> logger, IXrmq xrmq) : base(xrmq, "program")
        {
            this.logger = logger;
        }

        protected override async Task Handle(byte[] message)
        {
            logger.LogInformation("message program: {m}", Encoding.UTF8.GetString(message));
            await Task.FromException(new Exception("sagaz"));

        }
    }
}