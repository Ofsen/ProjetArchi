using ArchiLibrary.Data;
using ArchiLibrary.Models;
using ArchiLog.Models;
using Microsoft.EntityFrameworkCore;

namespace ArchiLog.Data
{
    public class ArchiLogDbContext : BaseDbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer(@"");
        }

        public DbSet<Brand> Brands { get; set; }

        public DbSet<Car> Cars { get; set; }
    }
}
