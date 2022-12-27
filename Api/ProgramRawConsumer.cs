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

        protected override void Handle(byte[] message)
        {
            logger.LogInformation("message program: {m}", Encoding.UTF8.GetString(message));
            throw new Exception("sagaz");
        }
    }
}