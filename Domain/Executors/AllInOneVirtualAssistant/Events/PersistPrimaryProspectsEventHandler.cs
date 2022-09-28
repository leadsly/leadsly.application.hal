using System.Threading.Tasks;

namespace Domain.Executors.AllInOneVirtualAssistant.Events
{
    public delegate Task PersistPrimaryProspectsEventHandler(object sender, PersistPrimaryProspectsEventArgs e);
}
