using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.WebDriver
{
    public enum BrowserPurpose
    {
        None,
        Auth,
        ProspectList,
        FollowUpMessages,
        MonitorForNewAcceptedConnections,
        Connect,
        ScanForReplies,
        Networking
    }
}
