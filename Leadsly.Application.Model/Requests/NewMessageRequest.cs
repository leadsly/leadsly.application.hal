namespace Leadsly.Application.Model.Requests
{
    public class NewMessageRequest
    {
        public string? ResponseMessage { get; set; }

        public long ResponseMessageTimestamp { get; set; }

        public string? ProspectName { get; set; }
    }
}
