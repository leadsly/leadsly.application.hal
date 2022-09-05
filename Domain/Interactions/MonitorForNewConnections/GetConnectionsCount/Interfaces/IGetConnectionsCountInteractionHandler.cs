namespace Domain.Interactions.MonitorForNewConnections.GetConnectionsCount.Interfaces
{
    public interface IGetConnectionsCountInteractionHandler : IInteractionHandler
    {
        int GetConnectionsCount();
    }
}
