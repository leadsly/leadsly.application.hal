namespace Domain.Interactions.DeepScanProspectsForReplies.GetProspectsMessageItem
{
    public class GetProspectsMessageItemInteraction : InteractionBase
    {
        public string ProspectName { get; set; }
        public int MessagesCountBefore { get; set; }
    }
}
