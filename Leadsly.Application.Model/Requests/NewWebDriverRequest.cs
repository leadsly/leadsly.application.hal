using Leadsly.Application.Model.WebDriver;
using System;
using System.Runtime.Serialization;

namespace Leadsly.Application.Model.Requests
{
    [DataContract]
    public class NewWebDriverRequest : AccountBaseRequest, IInfoForHal
    {
        public BrowserPurpose BrowserPurpose { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public long AttemptNumber { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string WindowHandleId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
