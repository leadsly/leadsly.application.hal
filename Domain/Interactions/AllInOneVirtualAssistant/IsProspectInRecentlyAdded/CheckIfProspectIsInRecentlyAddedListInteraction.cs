namespace Domain.Interactions.AllInOneVirtualAssistant.IsProspectInRecentlyAdded
{
    public class CheckIfProspectIsInRecentlyAddedListInteraction : InteractionBase
    {
        public string ProspectName { get; set; }
        public string ProfileUrl { get; set; }
        public bool IsFilteredByProspectName { get; set; }
    }
}
