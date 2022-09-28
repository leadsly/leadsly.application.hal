using Domain.Models.ProspectList;
using Domain.MQ.Messages;
using System;
using System.Collections.Generic;

namespace Domain.Executors.AllInOneVirtualAssistant.Events
{
    public class PersistPrimaryProspectsEventArgs : EventArgs
    {
        public PersistPrimaryProspectsEventArgs(PublishMessageBody message, List<PersistPrimaryProspectModel> persistPrimaryProspects)
        {
            Message = message;
            PersistPrimaryProspects = persistPrimaryProspects;
        }

        public PublishMessageBody Message { get; set; }
        public List<PersistPrimaryProspectModel> PersistPrimaryProspects { get; set; }

    }
}
