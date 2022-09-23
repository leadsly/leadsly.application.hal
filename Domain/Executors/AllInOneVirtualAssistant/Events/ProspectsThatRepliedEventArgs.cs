using Domain.Models.DeepScanProspectsForReplies;
using Domain.MQ.Messages;
using System;
using System.Collections.Generic;

namespace Domain.Executors.AllInOneVirtualAssistant.Events
{
    public class ProspectsThatRepliedEventArgs : EventArgs
    {
        public ProspectsThatRepliedEventArgs(PublishMessageBody message, IList<ProspectRepliedModel> prospects)
        {
            Message = message;
            Prospects = prospects;
        }

        public PublishMessageBody Message { get; set; }
        public IList<ProspectRepliedModel> Prospects { get; set; }
    }
}
