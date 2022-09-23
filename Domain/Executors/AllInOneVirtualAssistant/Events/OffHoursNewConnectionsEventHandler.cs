using System.Threading.Tasks;

namespace Domain.Executors.AllInOneVirtualAssistant.Events
{
    public delegate Task OffHoursNewConnectionsEventHandler(object sender, OffHoursNewConnectionsEventArgs e);
}
