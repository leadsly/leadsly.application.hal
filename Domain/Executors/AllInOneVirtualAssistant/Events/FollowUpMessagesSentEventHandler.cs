using System.Threading.Tasks;

namespace Domain.Executors.AllInOneVirtualAssistant.Events
{
    public delegate Task FollowUpMessagesSentEventHandler(object sender, FollowUpMessagesSentEventArgs e);
}
