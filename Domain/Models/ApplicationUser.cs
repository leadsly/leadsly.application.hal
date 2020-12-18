using Microsoft.AspNetCore.Identity;

namespace Domain.Models
{
    public interface IApplicationUser
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }

    public class ApplicationUser : IdentityUser, IApplicationUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool Deleted { get; set; } = false;
        public string ExternalProviderUserId { get; set; }
        public string PhotoUrl { get; set; }
        public string ExternalProvider { get; set; }
    }
}
