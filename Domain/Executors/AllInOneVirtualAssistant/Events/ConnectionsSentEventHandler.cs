using System.Threading.Tasks;

namespace Domain.Executors.AllInOneVirtualAssistant.Events
{
    public delegate Task ConnectionsSentEventHandler(object sender, ConnectionsSentEventArgs e);
}
