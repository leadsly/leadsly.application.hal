namespace Domain.PhaseConsumers.AllInOneVirtualAssistantHandler
{
    public class AllInOneVirtualAssistantConsumerCommand : IConsumeCommand
    {
        public AllInOneVirtualAssistantConsumerCommand(string halId)
        {
            HalId = halId;
        }

        public string HalId { get; set; }
    }
}
