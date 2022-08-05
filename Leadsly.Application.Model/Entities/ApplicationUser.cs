using Leadsly.Application.Model.Entities.Campaigns;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Leadsly.Application.Model.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? StripeCustomerId { get; set; }
        public bool Deleted { get; set; } = false;
        public string? ExternalProviderUserId { get; set; }
        public string? PhotoUrl { get; set; }
        public string? ExternalProvider { get; set; }
        public ICollection<PrimaryProspectList> ProspectLists { get; set; }
        public Customer_Stripe? Customer_Stripe { get; set; }
        public ICollection<SocialAccount> SocialAccounts { get; set; }
        public ICollection<Organization> Organizations { get; set; }
        public ICollection<Campaign> Campaigns { get; set; }
        public ICollection<HalUnit> HalDetails { get; set; }
        public ICollection<VirtualAssistant> VirtualAssistants { get; set; }
    }
}
