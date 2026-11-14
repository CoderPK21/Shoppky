using Microsoft.EntityFrameworkCore;
using Shoppky.Models;

namespace Shoppky.Data
{
    public class ShoppkyDbContext : DbContext
    {
        public ShoppkyDbContext(DbContextOptions<ShoppkyDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<AdminUser> AdminUsers => Set<AdminUser>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Product entity
            modelBuilder.Entity<Product>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18, 2)");

            // Create unique index on ASIN
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Asin)
                .IsUnique();

            // Configure AdminUser entity
            modelBuilder.Entity<AdminUser>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<AdminUser>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<AdminUser>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}
