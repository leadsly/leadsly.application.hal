namespace Domain.Interactions
{
    public interface IInteractionHandler
    {
        bool HandleInteraction(InteractionBase interaction);
    }
}
