using ECommBackend.Models;
using Microsoft.EntityFrameworkCore;
namespace ECommBackend.DatabaseConns
{
    public class SQLiteConn:DbContext
    {
        public DbSet<UserModel> Users { get; set; }
        public DbSet<AdminModel> Admins { get; set; }
        public DbSet<OrderModel> Orders { get; set; }
        public DbSet<ProductModel> Products { get; set; }

        public SQLiteConn(DbContextOptions<SQLiteConn> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserModel>().HasKey(user => user.UserId);
            modelBuilder.Entity<AdminModel>().HasKey(admin => admin.UserId);
            modelBuilder.Entity<OrderModel>().HasKey(order => order.OrderId);
            modelBuilder.Entity<ProductModel>().HasKey(product => product.ProductId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
