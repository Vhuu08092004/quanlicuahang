using Microsoft.EntityFrameworkCore;
using Quanlicuahang.Models;

namespace Quanlicuahang.Data
{
    public class ApplicationDbContext : DbContext
    {
        private const string V = "[\"*\"]";

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<UserToken> UserTokens { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductAttribute> ProductAttributes { get; set; }
        public DbSet<ProductAttributeValue> ProductAttributeValues { get; set; }
        public DbSet<ProductVariantAttributeValue> ProductVariantAttributeValues { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<ActionLog> ActionLogs { get; set; }
        public DbSet<StockEntry> StockEntries { get; set; }
        public DbSet<StockEntryItem> StockEntryItems { get; set; }
        public DbSet<StockExit> StockExits { get; set; }
        public DbSet<StockExitItem> StockExitItems { get; set; }
        public DbSet<Return> Returns { get; set; }
        public DbSet<ReturnItem> ReturnItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Employee)
                .WithMany(e => e.Users)
                .HasForeignKey(u => u.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserToken>()
                .HasOne(ut => ut.User)
                .WithMany()
                .HasForeignKey(ut => ut.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Supplier)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ProductVariant>()
                .HasOne(pv => pv.Product)
                .WithMany(p => p.Variants)
                .HasForeignKey(pv => pv.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductAttributeValue>()
                .HasOne(pav => pav.Attribute)
                .WithMany(pa => pa.AttributeValues)
                .HasForeignKey(pav => pav.AttributeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductVariantAttributeValue>()
                .HasOne(pvav => pvav.ProductVariant)
                .WithMany(pv => pv.VariantValues)
                .HasForeignKey(pvav => pvav.ProductVariantId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductVariantAttributeValue>()
                .HasOne(pvav => pvav.AttributeValue)
                .WithMany(pav => pav.VariantValues)
                .HasForeignKey(pvav => pvav.AttributeValueId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.Product)
                .WithMany(p => p.Inventories)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Promotion)
                .WithMany(p => p.Orders)
                .HasForeignKey(o => o.PromoId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.ProductVariant)
                .WithMany()
                .HasForeignKey(oi => oi.ProductVariantId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithMany(o => o.Payments)
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StockEntry>()
                .HasOne(se => se.Supplier)
                .WithMany(s => s.StockEntries)
                .HasForeignKey(se => se.SupplierId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<StockEntry>()
                .HasOne(se => se.User)
                .WithMany(u => u.StockEntries)
                .HasForeignKey(se => se.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<StockEntryItem>()
                .HasOne(sei => sei.StockEntry)
                .WithMany(se => se.StockEntryItems)
                .HasForeignKey(sei => sei.StockEntryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StockEntryItem>()
                .HasOne(sei => sei.Product)
                .WithMany(p => p.StockEntryItems)
                .HasForeignKey(sei => sei.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StockExit>()
                .HasOne(se => se.User)
                .WithMany(u => u.StockExits)
                .HasForeignKey(se => se.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<StockExitItem>()
                .HasOne(sei => sei.StockExit)
                .WithMany(se => se.StockExitItems)
                .HasForeignKey(sei => sei.StockExitId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StockExitItem>()
                .HasOne(sei => sei.Product)
                .WithMany(p => p.StockExitItems)
                .HasForeignKey(sei => sei.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Return>()
                .HasOne(r => r.Order)
                .WithMany()
                .HasForeignKey(r => r.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Return>()
                .HasOne(r => r.User)
                .WithMany(u => u.Returns)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ReturnItem>()
                .HasOne(ri => ri.Return)
                .WithMany(r => r.ReturnItems)
                .HasForeignKey(ri => ri.ReturnId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ReturnItem>()
                .HasOne(ri => ri.Product)
                .WithMany(p => p.ReturnItems)
                .HasForeignKey(ri => ri.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ActionLog>()
                .HasOne(al => al.User)
                .WithMany(u => u.ActionLogs)
                .HasForeignKey(al => al.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false); 


            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Code)
                .IsUnique();

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Barcode);

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Code)
                .IsUnique();

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.Code)
                .IsUnique();

            var adminRoleId = GenerateCode("ROLE");      
            var adminUserId = GenerateCode("USER");     
            var userRoleId = GenerateCode("USERROLE");
            var now = DateTime.UtcNow;
            var adminPassword = HashPassword("admin123@");

            modelBuilder.Entity<Role>().HasData(
                new Role
                {
                    Id = adminRoleId,
                    Code = "ADMIN",
                    Name = "Admin",
                    Description = "Administrator role with full access",
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                }
            );

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = adminUserId,
                    Username = "admin",
                    Password = adminPassword,
                    FullName = "Administrator",
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false,
                    EmployeeId = null
                }
            );

            modelBuilder.Entity<UserRole>().HasData(
                new UserRole
                {
                    Id = userRoleId,
                    UserId = adminUserId,
                    RoleId = adminRoleId,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                }
            );
        }


        private static string GenerateCode(string prefix)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var randomPart = Guid.NewGuid().ToString("N").Substring(0, 8);
            return $"{prefix.ToLower()}-{timestamp}-{randomPart}";
        }

        private static string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }


}
