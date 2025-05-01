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
            .HasPrecision(18, 2); // Ensure precision for Rating

        modelBuilder.Entity<Product>()
            .Property(p => p.Name)
            .HasColumnType("varchar(255)"); // Use varchar for Name

        modelBuilder.Entity<Product>()
            .Property(p => p.Description)
            .HasColumnType("text"); // Use text for Description

        modelBuilder.Entity<Product>()
            .Property(p => p.Producer)
            .HasColumnType("varchar(255)"); // Use varchar for Producer

        modelBuilder.Entity<Product>()
            .Property(p => p.Publisher)
            .HasColumnType("varchar(255)"); // Use varchar for Publisher

        modelBuilder.Entity<Product>()
            .Property(p => p.Platform)
            .HasColumnType("varchar(255)"); // Use varchar for Platform

        modelBuilder.Entity<Product>()
            .Property(p => p.Year)
            .HasColumnType("int"); // Use int for Year

        modelBuilder.Entity<Product>()
            .Property(p => p.Genres)
            .HasColumnType("json"); // Use json for Genres (array of strings)

        modelBuilder.Entity<Product>()
            .OwnsOne(p => p.SystemRequirements, sa =>
            {
                sa.Property(s => s.OS).HasColumnType("varchar(255)");
                sa.Property(s => s.Processor).HasColumnType("varchar(255)");
                sa.Property(s => s.Memory).HasColumnType("varchar(255)");
                sa.Property(s => s.Graphics).HasColumnType("varchar(255)");
                sa.Property(s => s.Storage).HasColumnType("varchar(255)");
            });
    }





}
