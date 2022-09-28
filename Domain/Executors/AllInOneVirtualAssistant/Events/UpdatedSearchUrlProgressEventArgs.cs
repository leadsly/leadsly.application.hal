using Domain.Models.Networking;
using Domain.MQ.Messages;
using System;
using System.Collections.Generic;

namespace Domain.Executors.AllInOneVirtualAssistant.Events
{
    public class UpdatedSearchUrlProgressEventArgs : EventArgs
    {
        public UpdatedSearchUrlProgressEventArgs(PublishMessageBody message, IList<SearchUrlProgressModel> updatedSearchUrlsProgress)
        {
            Message = message;
            UpdatedSearchUrlsProgress = updatedSearchUrlsProgress;
        }

        public PublishMessageBody Message { get; set; }
        public IList<SearchUrlProgressModel> UpdatedSearchUrlsProgress { get; set; }

    }
}
