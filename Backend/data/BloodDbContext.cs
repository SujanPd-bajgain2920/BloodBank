using bloodconnect.Model;
using Microsoft.EntityFrameworkCore;

namespace bloodconnect.data
{
    public class BloodDbContext : DbContext
    {
        public BloodDbContext(DbContextOptions<BloodDbContext> options) : base(options)
        {

        }

        public DbSet<Registration> registration { get; set; }
        public DbSet<Profile> Profile { get; set; }


    }
}
