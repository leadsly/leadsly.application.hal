﻿using System.Collections.Generic;

namespace Domain.Models.Requests
{
    public class RecentlyAddedProspectsRequest
    {
        public string NamespaceName { get; set; }
        public string ServiceDiscoveryName { get; set; }
        public string RequestUrl { get; set; }
        public string ApplicationUserId { get; set; }
        public IList<RecentlyAddedProspect> Items { get; set; }
    }
}
