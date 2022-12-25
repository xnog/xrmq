using X;

namespace Api
{
    public class CreateProgramConsumer : XrmqConsumerBackgroundService<Message>
    {
        private readonly ILogger<CreateProgramConsumer> logger;

        public CreateProgramConsumer(ILogger<CreateProgramConsumer> logger, IXrmq xrmq) : base(xrmq)
        {
            this.logger = logger;
        }

        protected override string GetQueue() => "teste";

        protected override void Handle(Message message)
        {
            logger.LogInformation("message: {c} - {n}", message.Code, message.Name);
            throw new Exception("sagaz");
        }
    }
}