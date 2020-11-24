using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;

namespace DataCore
{
    public class DatabaseContext : IdentityDbContext<ApplicationUser>
    {
    }
}
