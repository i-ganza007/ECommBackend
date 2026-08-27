using ECommBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommBackend.DatabaseConns
{
    public class SQLConn : DbContext
    {
        public DbSet<UserModel> Users { get; set; }
        public DbSet<AdminModel> Admins { get; set; }
        public DbSet<OrderModel> Orders { get; set; }
        public DbSet<ProductModel> Products { get; set; }
        public DbSet<VariantModel> Variants { get; set; }
        public DbSet<ImageModel> Images { get; set; }

        public SQLConn(DbContextOptions<SQLConn> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Email uniqueness constraints
            modelBuilder.Entity<UserModel>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<AdminModel>()
                .HasIndex(a => a.Email)
                .IsUnique();

            // SKU uniqueness constraint
            modelBuilder.Entity<ProductModel>()
                .HasIndex(p => p.Base_SKU)
                .IsUnique();

            // User-Order relationship
            modelBuilder.Entity<UserModel>()
                .HasMany(u => u.Orders)
                .WithOne(o => o.OrderCreator)
                .OnDelete(DeleteBehavior.Cascade);

            // Admin-Product relationship
            modelBuilder.Entity<AdminModel>()
                .HasMany(a => a.ProductsOwned)
                .WithOne(p => p.Owner)
                .OnDelete(DeleteBehavior.Restrict);

            // Product-Variant relationship
            modelBuilder.Entity<ProductModel>()
                .HasMany(p => p.Variants)
                .WithOne(o => o.Product)
                .OnDelete(DeleteBehavior.Cascade);

            // Product Id Key
            modelBuilder.Entity<ProductModel>()
            .HasIndex(v => v.ProductId).IsUnique();

            // Product Name Key
            modelBuilder.Entity<ProductModel>()
            .HasIndex(v => v.Name).IsUnique();

            // Variant Index Key
            modelBuilder.Entity<VariantModel>()
            .HasIndex(v => v.VariantId).IsUnique();

            // Variant Price Key
            modelBuilder.Entity<VariantModel>()
            .HasIndex(v => v.Price).IsUnique();


            // Variant Size Key
            modelBuilder.Entity<VariantModel>()
            .HasIndex(v => v.Size).IsUnique();

            // Variant-Image relationship
            modelBuilder.Entity<VariantModel>()
            .HasOne(v => v.VariantImage)
            .WithOne()
            .HasForeignKey<VariantModel>(v => v.VariantImageId)
            .OnDelete(DeleteBehavior.Cascade);

            // Order-Product many-to-many relationship
            modelBuilder.Entity<OrderModel>()
                .HasMany(o => o.Products)
                .WithMany()
                .UsingEntity("OrderProducts");

            base.OnModelCreating(modelBuilder);
        }
    }
}