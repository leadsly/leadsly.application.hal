using System.Threading.Tasks;

namespace Domain.Executors.AllInOneVirtualAssistant.Events
{
    public delegate Task MonthlySearchLimitReachedEventHandler(object sender, MonthlySearchLimitReachedEventArgs
        e);
}
