using Leadsly.Application.Model.Responses.Hal.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Responses.Hal.POMs
{
    public class NotificationsNavBarControl : INotificationNavBarControl
    {
        public OperationInformation OperationInformation { get; set; }
        public bool? NewNotification { get; set; }
        public int NotificationCount { get; set; }
    }
}
