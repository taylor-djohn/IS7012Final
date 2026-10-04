using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using IS7012Final.Model;

namespace IS7012Final.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<IS7012Final.Model.Genre> Genre { get; set; } = default!;
        public DbSet<IS7012Final.Model.Movie> Movie { get; set; } = default!;
        public DbSet<IS7012Final.Model.Review> Review { get; set; } = default!;
        public DbSet<IS7012Final.Model.Watchlist> Watchlist { get; set; } = default!;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
         
        }
    }
}
