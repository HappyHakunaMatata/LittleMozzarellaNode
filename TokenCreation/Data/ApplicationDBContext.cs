using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace TokenCreation.Data
{
	public partial class ApplicationDBContext : IdentityDbContext
    {
		public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
            var created = Database.EnsureCreated();
           
            if (Database.GetPendingMigrations().Any())
            {
                Database.Migrate();
            }
        }
    }
}

