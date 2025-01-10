using Microsoft.EntityFrameworkCore;

namespace tmapi.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Task> Tasks { get; set; }  // Adding Task DbSet to the context
    }
}
