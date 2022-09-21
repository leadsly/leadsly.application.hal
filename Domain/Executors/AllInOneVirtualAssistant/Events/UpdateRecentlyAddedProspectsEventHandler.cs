using System.Threading.Tasks;

namespace Domain.Executors.AllInOneVirtualAssistant.Events
{
    public delegate Task UpdateRecentlyAddedProspectsEventHandler(object sender, UpdateRecentlyAddedProspectsEventArgs e);
}
