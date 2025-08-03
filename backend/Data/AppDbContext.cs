namespace Cogtive.DevAssignment.Api.Data;

using Microsoft.EntityFrameworkCore;
using Cogtive.DevAssignment.Api.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Machine> Machines { get; set; } = null!;
    public DbSet<ProductionData> ProductionData { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ProductionData>()
            .HasIndex(p => p.MachineId);

        modelBuilder.Entity<ProductionData>()
            .HasOne<Machine>()
            .WithMany()
            .HasForeignKey(p => p.MachineId);
    
        // PostgreSQL specific configurations
        if (Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL")
        {
            // Use proper PostgreSQL types
            modelBuilder.Entity<ProductionData>()
                .Property(p => p.Efficiency)
                .HasColumnType("decimal(5,2)");
            
            // PostgreSQL case-sensitive collation if needed
            modelBuilder.Entity<Machine>()
                .Property(m => m.Name)
                .UseCollation("und-x-icu");
        }
    }
}