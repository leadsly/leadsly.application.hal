using System.Threading.Tasks;

namespace Domain.Executors.AllInOneVirtualAssistant.Events
{
    public delegate Task UpdatedSearchUrlProgressEventHandler(object sender, UpdatedSearchUrlProgressEventArgs e);
}
