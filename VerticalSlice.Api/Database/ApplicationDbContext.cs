using Microsoft.EntityFrameworkCore;
using VerticalSlice.Api.Entities;

namespace VerticalSlice.Api.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
    }
}
