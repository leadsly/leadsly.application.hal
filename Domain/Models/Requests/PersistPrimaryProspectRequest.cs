﻿namespace Domain.Models.Requests
{
    public class PersistPrimaryProspectRequest
    {
        public string Name { get; set; }
        public string ProfileUrl { get; set; }
        public long AddedTimestamp { get; set; }
        public string PrimaryProspectListId { get; set; }
        public string Area { get; set; }
        public string EmploymentInfo { get; set; }
        public string SearchResultAvatarUrl { get; set; }
    }
}
