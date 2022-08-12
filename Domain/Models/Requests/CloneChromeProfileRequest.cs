namespace Domain.Models.Requests
{
    public class CloneChromeProfileRequest
    {
        public string GridNamespaceName { get; set; }
        public string GridServiceDiscoveryName { get; set; }
        public string RequestUrl { get; set; }
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
