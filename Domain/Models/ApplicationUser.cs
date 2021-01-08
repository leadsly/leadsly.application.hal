using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [Required]
        public string ApplicationId { get; set; }
        public bool Deleted { get; set; } = false;
        public string ExternalProviderUserId { get; set; }
        public string PhotoUrl { get; set; }
        public string ExternalProvider { get; set; }
    }
}
