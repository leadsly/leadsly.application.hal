using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Responses.Hal.Interfaces
{
    public interface INotificationNavBarControl : IOperationResponse
    {
        public bool? NewNotification { get; }
        public int NotificationCount { get; }
    }
}
