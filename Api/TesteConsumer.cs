using X;

namespace Api
{
    public class TesteConsumer : XrmqConsumerBackgroundService<Message>
    {
        private readonly ILogger<TesteConsumer> logger;

        public TesteConsumer(ILogger<TesteConsumer> logger, IXrmq xrmq) : base(xrmq, "teste")
        {
            this.logger = logger;
        }

        protected override void Handle(Message message)
        {
            logger.LogInformation("message teste: {c} - {n}", message.Code, message.Name);
            throw new Exception("sagaz");
        }
    }
}