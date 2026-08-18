using BlazorDemoApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorDemoApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<DemoTask> DemoTasks => Set<DemoTask>();
    }
}
