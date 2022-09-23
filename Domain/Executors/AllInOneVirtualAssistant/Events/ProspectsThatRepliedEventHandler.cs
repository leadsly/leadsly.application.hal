using System.Threading.Tasks;

namespace Domain.Executors.AllInOneVirtualAssistant.Events
{
    public delegate Task ProspectsThatRepliedEventHandler(object sender, ProspectsThatRepliedEventArgs e);
}
