using BlazorDemoApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorDemoApp.Data;

public class PostgresDbContext : DbContext
{
    public PostgresDbContext(DbContextOptions<PostgresDbContext> options) : base(options) { }

    public DbSet<KnowledgeItem> KnowledgeItems => Set<KnowledgeItem>();
    public DbSet<PostgresCategory> Categories => Set<PostgresCategory>();
    public DbSet<PostgresProduct> Products => Set<PostgresProduct>();
    public DbSet<PostgresAuditLog> AuditLogs => Set<PostgresAuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<KnowledgeItem>()
            .OwnsOne(k => k.Metadata, b => b.ToJson("metadata"));

        modelBuilder.Entity<PostgresProduct>()
            .OwnsOne(p => p.Specifications, b => b.ToJson("specifications"));
    }
}