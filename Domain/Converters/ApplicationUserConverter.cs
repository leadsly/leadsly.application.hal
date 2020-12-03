using Domain.Models;
using Domain.ViewModels;
using System.Linq;

namespace Domain.Converters
{
    static class ApplicationUserConverter
    {
        public static ApplicationUserViewModel Convert(ApplicationUser user)
        {
            return new ApplicationUserViewModel
            {
                Id = user.Id,
                Email = user.Email
            };
        }
    }
}
