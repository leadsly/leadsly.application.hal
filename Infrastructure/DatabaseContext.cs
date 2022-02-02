using DataCore.Configurations;
using Leadsly.Models.Database;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataCore
{
    public class DatabaseContext : IdentityDbContext<ApplicationUser>
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options, ILogger<DatabaseContext> logger) : base(options) 
        {
            _logger = logger;
        }

        private ILogger<DatabaseContext> _logger;

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            this._logger.LogInformation("Configuring custom database settings.");

            IdentityUsersConfiguration.ConfigureIdentityUsersTableNames(builder, _logger);
        }
    }
}
