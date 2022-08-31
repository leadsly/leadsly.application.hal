using System.Threading.Tasks;

namespace Domain.Executors.ScanProspectsForReplies.Events
{
    public delegate Task NewMessagesReceivedEventHandler(object sender, NewMessagesReceivedEventArgs e);
}
