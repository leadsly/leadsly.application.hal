namespace Domain.Models.Requests
{
    public class CloneChromeProfileRequest
    {
        public string BaseUrl { get; set; }
        public string Endpoint { get; set; }
        public string NewChromeProfile { get; set; } = string.Empty;
        public string DefaultChromeUserProfilesDir { get; set; } = string.Empty;
        public string DefaultChromeProfileName { get; set; } = string.Empty;
        /// <summary>
        /// Used for local development only
        /// </summary>
        public string ProfilesVolume { get; set; } = string.Empty;
        public bool? UseGrid { get; set; } = false;
    }
}
