using EShop.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EShop.Domain.Repositories;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>()
            .OwnsOne(p => p.SystemRequirements);

        modelBuilder.Entity<Product>()
            .Property(p => p.Rating)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Product>()
            .Property(p => p.Name)
            .HasColumnType("nvarchar(255)");

        modelBuilder.Entity<Product>()
            .Property(p => p.Description)
            .HasColumnType("nvarchar(max)");

        modelBuilder.Entity<Product>()
            .Property(p => p.Producer)
            .HasColumnType("nvarchar(255)");

        modelBuilder.Entity<Product>()
            .Property(p => p.Publisher)
            .HasColumnType("nvarchar(255)");

        modelBuilder.Entity<Product>()
            .Property(p => p.Platform)
            .HasColumnType("nvarchar(255)");

        modelBuilder.Entity<Product>()
            .Property(p => p.Year)
            .HasColumnType("int");

        modelBuilder.Entity<Product>()
            .Property(p => p.Genres)
            .HasColumnType("nvarchar(max)");

        modelBuilder.Entity<Product>()
            .OwnsOne(p => p.SystemRequirements, sa =>
            {
                sa.Property(s => s.OS).HasColumnType("nvarchar(255)");
                sa.Property(s => s.Processor).HasColumnType("nvarchar(255)");
                sa.Property(s => s.Memory).HasColumnType("nvarchar(255)");
                sa.Property(s => s.Graphics).HasColumnType("nvarchar(255)");
                sa.Property(s => s.Storage).HasColumnType("nvarchar(255)");
            });
    }
}
