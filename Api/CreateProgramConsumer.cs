using X;

namespace Api
{
    public class CreateProgramConsumer : XrmqConsumerBackgroundService<Message>
    {
        private readonly ILogger<CreateProgramConsumer> logger;

        public CreateProgramConsumer(ILogger<CreateProgramConsumer> logger, IXrmq xrmq) : base(xrmq, "teste")
        {
            this.logger = logger;
        }

        protected override void Handle(Message message)
        {
            logger.LogInformation("message: {c} - {n}", message.Code, message.Name);
            throw new Exception("sagaz");
        }
    }
}