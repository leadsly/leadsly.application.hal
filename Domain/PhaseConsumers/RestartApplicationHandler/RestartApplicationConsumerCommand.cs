namespace Domain.PhaseConsumers.RestartApplicationHandler
{
    public class RestartApplicationConsumerCommand : IConsumeCommand
    {
        public RestartApplicationConsumerCommand(string halId)
        {
            HalId = halId;
        }
        public string HalId { get; set; }
    }
}
