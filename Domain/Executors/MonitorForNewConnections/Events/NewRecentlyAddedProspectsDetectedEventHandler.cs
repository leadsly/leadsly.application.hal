using System.Threading.Tasks;

namespace Domain.Executors.MonitorForNewConnections.Events
{
    public delegate Task NewRecentlyAddedProspectsDetectedEventHandler(object sender, NewRecentlyAddedProspectsDetectedEventArgs e);
}
