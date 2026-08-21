using BlazorDemoApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorDemoApp.Data;

public class PostgresDbContext : DbContext
{
    public PostgresDbContext(DbContextOptions<PostgresDbContext> options) : base(options) { }

    public DbSet<KnowledgeItem> KnowledgeItems => Set<KnowledgeItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Mapowanie obiektu Metadata jako kolumny JSONB w PostgreSQL
        modelBuilder.Entity<KnowledgeItem>()
            .OwnsOne(k => k.Metadata, ownedNavigationBuilder =>
            {
                ownedNavigationBuilder.ToJson();
            });
    }
}