namespace Leadsly.Application.Model.Requests
{
    public class NewMessageRequest
    {
        public string? ResponseMessage { get; set; }

        /// <summary>
        /// This isnt the actual resopnse message timestamp, rather the timestamp of when hal created this request object
        /// </summary>
        public long ResponseMessageTimestamp { get; set; }

        public string? ProspectName { get; set; }
    }
}
