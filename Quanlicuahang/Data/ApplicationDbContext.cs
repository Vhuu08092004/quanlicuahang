using Microsoft.EntityFrameworkCore;
using Quanlicuahang.Models;

namespace Quanlicuahang.Data
{
    public class ApplicationDbContext : DbContext
    {
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
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<WarehouseArea> WarehouseAreas { get; set; }
        public DbSet<AreaInventory> AreaInventories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình relationships
            ConfigureRelationships(modelBuilder);

            // Cấu hình indexes
            ConfigureIndexes(modelBuilder);

            // Seed data
            SeedData(modelBuilder);
        }

        private void ConfigureRelationships(ModelBuilder modelBuilder)
        {
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

            modelBuilder.Entity<StockEntryItem>()
                .HasOne(sei => sei.WarehouseArea)
                .WithMany()
                .HasForeignKey(sei => sei.WarehouseAreaId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

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

            modelBuilder.Entity<WarehouseArea>()
                .HasOne(wa => wa.Warehouse)
                .WithMany(w => w.Areas)
                .HasForeignKey(wa => wa.WarehouseId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            modelBuilder.Entity<AreaInventory>()
                .HasOne(ai => ai.WarehouseArea)
                .WithMany(wa => wa.AreaInventories)
                .HasForeignKey(ai => ai.WarehouseAreaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AreaInventory>()
                .HasOne(ai => ai.Product)
                .WithMany()
                .HasForeignKey(ai => ai.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ActionLog>()
                .HasOne(al => al.User)
                .WithMany(u => u.ActionLogs)
                .HasForeignKey(al => al.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
        }

        private void ConfigureIndexes(ModelBuilder modelBuilder)
        {
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
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            var now = DateTime.UtcNow;

            var adminRoleId = GenerateCode("ROLE");
            var managerRoleId = GenerateCode("ROLE");
            var cashierRoleId = GenerateCode("ROLE");
            var stockRoleId = GenerateCode("ROLE");

            var adminUserId = GenerateCode("USER");
            var managerUserId = GenerateCode("USER");
            var cashier1UserId = GenerateCode("USER");
            var stockUserId = GenerateCode("USER");

            var emp1Id = GenerateCode("EMP");
            var emp2Id = GenerateCode("EMP");
            var emp3Id = GenerateCode("EMP");

            var cat1Id = GenerateCode("CAT");
            var cat2Id = GenerateCode("CAT");
            var cat3Id = GenerateCode("CAT");
            var cat4Id = GenerateCode("CAT");
            var cat5Id = GenerateCode("CAT");
            var cat6Id = GenerateCode("CAT");

            var sup1Id = GenerateCode("SUP");
            var sup2Id = GenerateCode("SUP");
            var sup3Id = GenerateCode("SUP");

            var cust1Id = GenerateCode("CUST");
            var cust2Id = GenerateCode("CUST");
            var cust3Id = GenerateCode("CUST");

            var prod1Id = GenerateCode("PROD");
            var prod2Id = GenerateCode("PROD");
            var prod3Id = GenerateCode("PROD");
            var prod4Id = GenerateCode("PROD");
            var prod5Id = GenerateCode("PROD");
            var prod6Id = GenerateCode("PROD");
            var prod7Id = GenerateCode("PROD");
            var prod8Id = GenerateCode("PROD");
            var prod9Id = GenerateCode("PROD");
            var prod10Id = GenerateCode("PROD");
            var prod11Id = GenerateCode("PROD");
            var prod12Id = GenerateCode("PROD");

            var promo1Id = GenerateCode("PROMO");
            var promo2Id = GenerateCode("PROMO");

            var attr1Id = GenerateCode("ATTR");
            var attr2Id = GenerateCode("ATTR");
            var attr3Id = GenerateCode("ATTR");
            var attr4Id = GenerateCode("ATTR");


            // Seed Roles
            modelBuilder.Entity<Role>().HasData(
                new Role
                {
                    Id = adminRoleId,
                    Code = "ADMIN",
                    Name = "Quản trị viên",
                    Description = "Toàn quyền quản lý hệ thống",
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new Role
                {
                    Id = managerRoleId,
                    Code = "MANAGER",
                    Name = "Quản lý cửa hàng",
                    Description = "Quản lý toàn bộ hoạt động cửa hàng",
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new Role
                {
                    Id = cashierRoleId,
                    Code = "CASHIER",
                    Name = "Thu ngân",
                    Description = "Nhân viên thu ngân, bán hàng",
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new Role
                {
                    Id = stockRoleId,
                    Code = "STOCK",
                    Name = "Thủ kho",
                    Description = "Quản lý kho hàng, nhập xuất",
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                }
            );

            // Seed Employees
            modelBuilder.Entity<Employee>().HasData(
                new Employee
                {
                    Id = emp1Id,
                    Code = "NV001",
                    Name = "Nguyễn Văn Quản",
                    Phone = "0901234567",
                    Email = "quanly@minimart.vn",
                    Address = "123 Nguyễn Văn Linh, Q.7, TP.HCM",
                    BirthDate = new DateTime(1985, 3, 15),
                    Position = "Quản lý",
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new Employee
                {
                    Id = emp2Id,
                    Code = "NV002",
                    Name = "Trần Thị Thu",
                    Phone = "0902345678",
                    Email = "thungan1@minimart.vn",
                    Address = "456 Lê Văn Việt, Q.9, TP.HCM",
                    BirthDate = new DateTime(1995, 7, 20),
                    Position = "Thu ngân",
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new Employee
                {
                    Id = emp3Id,
                    Code = "NV003",
                    Name = "Lê Văn Kho",
                    Phone = "0903456789",
                    Email = "thukho@minimart.vn",
                    Address = "789 Võ Văn Ngân, Q.Thủ Đức, TP.HCM",
                    BirthDate = new DateTime(1992, 11, 5),
                    Position = "Thủ kho",
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                }
            );

            // Seed Users
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = adminUserId,
                    Username = "admin",
                    Password = HashPassword("admin123@"),
                    FullName = "Administrator",
                    EmployeeId = null,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new User
                {
                    Id = managerUserId,
                    Username = "quanly",
                    Password = HashPassword("quanly123"),
                    FullName = "Nguyễn Văn Quản",
                    EmployeeId = emp1Id,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new User
                {
                    Id = cashier1UserId,
                    Username = "thungan1",
                    Password = HashPassword("thungan123"),
                    FullName = "Trần Thị Thu",
                    EmployeeId = emp2Id,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new User
                {
                    Id = stockUserId,
                    Username = "thukho",
                    Password = HashPassword("thukho123"),
                    FullName = "Lê Văn Kho",
                    EmployeeId = emp3Id,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                }
            );

            // Seed UserRoles
            modelBuilder.Entity<UserRole>().HasData(
                new UserRole
                {
                    Id = GenerateCode("UR"),
                    UserId = adminUserId,
                    RoleId = adminRoleId,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new UserRole
                {
                    Id = GenerateCode("UR"),
                    UserId = managerUserId,
                    RoleId = managerRoleId,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new UserRole
                {
                    Id = GenerateCode("UR"),
                    UserId = cashier1UserId,
                    RoleId = cashierRoleId,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new UserRole
                {
                    Id = GenerateCode("UR"),
                    UserId = stockUserId,
                    RoleId = stockRoleId,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                }
            );

            // Seed Categories (Danh mục cho siêu thị mini)
            modelBuilder.Entity<Category>().HasData(
                new Category
                {
                    Id = cat1Id,
                    Code = "DM001",
                    Name = "Đồ uống",
                    Description = "Nước ngọt, nước suối, bia, rượu",
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new Category
                {
                    Id = cat2Id,
                    Code = "DM002",
                    Name = "Snack & Bánh kẹo",
                    Description = "Bánh quy, kẹo, snack các loại",
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new Category
                {
                    Id = cat3Id,
                    Code = "DM003",
                    Name = "Thực phẩm tươi sống",
                    Description = "Rau củ, thịt cá, trái cây",
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new Category
                {
                    Id = cat4Id,
                    Code = "DM004",
                    Name = "Sữa & Đồ uống có đường",
                    Description = "Sữa tươi, sữa chua, trà sữa",
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new Category
                {
                    Id = cat5Id,
                    Code = "DM005",
                    Name = "Đồ dùng cá nhân",
                    Description = "Kem đánh răng, dầu gội, xà phòng",
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new Category
                {
                    Id = cat6Id,
                    Code = "DM006",
                    Name = "Gia vị & Mì gói",
                    Description = "Nước mắm, dầu ăn, mì tôm",
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                }
            );

            // Seed Suppliers
            modelBuilder.Entity<Supplier>().HasData(
                new Supplier
                {
                    Id = sup1Id,
                    Code = "NCC001",
                    Name = "Công ty Coca Cola Việt Nam",
                    Phone = "0281234567",
                    Email = "sales@coca-cola.vn",
                    Address = "Khu CN Tân Tạo, Q.Bình Tân, TP.HCM",
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new Supplier
                {
                    Id = sup2Id,
                    Code = "NCC002",
                    Name = "Công ty TNHH Orion Việt Nam",
                    Phone = "0282345678",
                    Email = "contact@orionvn.com",
                    Address = "KCN Vĩnh Lộc, Huyện Bình Chánh, TP.HCM",
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new Supplier
                {
                    Id = sup3Id,
                    Code = "NCC003",
                    Name = "Công ty Vinamilk",
                    Phone = "0283456789",
                    Email = "sales@vinamilk.vn",
                    Address = "120 Tân Cảng, Q.Bình Thạnh, TP.HCM",
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                }
            );

            // Seed Customers
            modelBuilder.Entity<Customer>().HasData(
                new Customer
                {
                    Id = cust1Id,
                    Code = "KH001",
                    Name = "Khách vãng lai",
                    Phone = "",
                    Email = "",
                    Address = "",
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new Customer
                {
                    Id = cust2Id,
                    Code = "KH002",
                    Name = "Phạm Thị Lan",
                    Phone = "0904567890",
                    Email = "lanpham@gmail.com",
                    Address = "234 Lý Thường Kiệt, Q.10, TP.HCM",
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new Customer
                {
                    Id = cust3Id,
                    Code = "KH003",
                    Name = "Hoàng Văn Nam",
                    Phone = "0905678901",
                    Email = "namhoang@gmail.com",
                    Address = "567 Hoàng Văn Thụ, Q.Phú Nhuận, TP.HCM",
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                }
            );

            // Seed Products (Sản phẩm siêu thị mini)
            modelBuilder.Entity<Product>().HasData(
                // Đồ uống
                new Product
                {
                    Id = prod1Id,
                    Code = "SP001",
                    Name = "Coca Cola 330ml",
                    Barcode = "8934588012341",
                    Price = 10000m,
                    Unit = "lon",
                    Quantity = 500,
                    CategoryId = cat1Id,
                    SupplierId = sup1Id,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new Product
                {
                    Id = prod2Id,
                    Code = "SP002",
                    Name = "Pepsi 330ml",
                    Barcode = "8934588012358",
                    Price = 10000m,
                    Unit = "lon",
                    Quantity = 450,
                    CategoryId = cat1Id,
                    SupplierId = sup1Id,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new Product
                {
                    Id = prod3Id,
                    Code = "SP003",
                    Name = "Nước suối Lavie 500ml",
                    Barcode = "8934588012365",
                    Price = 5000m,
                    Unit = "chai",
                    Quantity = 1000,
                    CategoryId = cat1Id,
                    SupplierId = sup1Id,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                // Snack & Bánh kẹo
                new Product
                {
                    Id = prod4Id,
                    Code = "SP004",
                    Name = "Snack khoai tây Poca 54g",
                    Barcode = "8934588012372",
                    Price = 12000m,
                    Unit = "gói",
                    Quantity = 300,
                    CategoryId = cat2Id,
                    SupplierId = sup2Id,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new Product
                {
                    Id = prod5Id,
                    Code = "SP005",
                    Name = "Bánh Chocopie 396g",
                    Barcode = "8934588012389",
                    Price = 45000m,
                    Unit = "hộp",
                    Quantity = 150,
                    CategoryId = cat2Id,
                    SupplierId = sup2Id,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new Product
                {
                    Id = prod6Id,
                    Code = "SP006",
                    Name = "Kẹo Alpenliebe 24 viên",
                    Barcode = "8934588012396",
                    Price = 15000m,
                    Unit = "gói",
                    Quantity = 200,
                    CategoryId = cat2Id,
                    SupplierId = sup2Id,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                // Sữa
                new Product
                {
                    Id = prod7Id,
                    Code = "SP007",
                    Name = "Sữa tươi Vinamilk 1L",
                    Barcode = "8934588012402",
                    Price = 35000m,
                    Unit = "hộp",
                    Quantity = 200,
                    CategoryId = cat4Id,
                    SupplierId = sup3Id,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new Product
                {
                    Id = prod8Id,
                    Code = "SP008",
                    Name = "Sữa chua Vinamilk 4 hộp",
                    Barcode = "8934588012419",
                    Price = 20000m,
                    Unit = "lốc",
                    Quantity = 150,
                    CategoryId = cat4Id,
                    SupplierId = sup3Id,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                // Đồ dùng cá nhân
                new Product
                {
                    Id = prod9Id,
                    Code = "SP009",
                    Name = "Kem đánh răng PS 230g",
                    Barcode = "8934588012426",
                    Price = 28000m,
                    Unit = "tuýp",
                    Quantity = 100,
                    CategoryId = cat5Id,
                    SupplierId = sup2Id,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new Product
                {
                    Id = prod10Id,
                    Code = "SP010",
                    Name = "Dầu gội Clear 650ml",
                    Barcode = "8934588012433",
                    Price = 120000m,
                    Unit = "chai",
                    Quantity = 80,
                    CategoryId = cat5Id,
                    SupplierId = sup2Id,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                // Gia vị & Mì gói
                new Product
                {
                    Id = prod11Id,
                    Code = "SP011",
                    Name = "Mì Hảo Hảo tôm chua cay (lốc 5)",
                    Barcode = "8934588012440",
                    Price = 20000m,
                    Unit = "lốc",
                    Quantity = 400,
                    CategoryId = cat6Id,
                    SupplierId = sup2Id,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new Product
                {
                    Id = prod12Id,
                    Code = "SP012",
                    Name = "Dầu ăn Simply 1L",
                    Barcode = "8934588012457",
                    Price = 45000m,
                    Unit = "chai",
                    Quantity = 120,
                    CategoryId = cat6Id,
                    SupplierId = sup2Id,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                }
            );


            modelBuilder.Entity<ProductAttribute>().HasData(
                new ProductAttribute
                {
                    Id = attr1Id,
                    Code = "COLOR",
                    Name = "Màu sắc",
                    Description = "Màu sắc sản phẩm",
                    DataType = "string",
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new ProductAttribute
                {
                    Id = attr2Id,
                    Code = "WEIGHT",
                    Name = "Trọng lượng/Khối lượng",
                    Description = "Trọng lượng hoặc khối lượng sản phẩm",
                    DataType = "decimal",
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new ProductAttribute
                {
                    Id = attr3Id,
                    Code = "FLAVOR",
                    Name = "Hương vị",
                    Description = "Hương vị sản phẩm",
                    DataType = "string",
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new ProductAttribute
                {
                    Id = attr4Id,
                    Code = "EXPIRY_DATE",
                    Name = "Hạn sử dụng",
                    Description = "Ngày hết hạn sản phẩm",
                    DataType = "date",
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                }
            );

            // Seed ProductAttributeValues
            modelBuilder.Entity<ProductAttributeValue>().HasData(
                // Coca Cola 330ml
                new ProductAttributeValue
                {
                    Id = GenerateCode("PAV"),
                    ProductId = prod1Id,
                    AttributeId = attr1Id,
                    ValueString = "Đỏ",
                    DisplayOrder = 1,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new ProductAttributeValue
                {
                    Id = GenerateCode("PAV"),
                    ProductId = prod1Id,
                    AttributeId = attr2Id,
                    ValueDecimal = 0.33m,
                    DisplayOrder = 2,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },

                // Pepsi 330ml
                new ProductAttributeValue
                {
                    Id = GenerateCode("PAV"),
                    ProductId = prod2Id,
                    AttributeId = attr1Id,
                    ValueString = "Xanh",
                    DisplayOrder = 1,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new ProductAttributeValue
                {
                    Id = GenerateCode("PAV"),
                    ProductId = prod2Id,
                    AttributeId = attr2Id,
                    ValueDecimal = 0.33m,
                    DisplayOrder = 2,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },

                // Nước suối Lavie 500ml
                new ProductAttributeValue
                {
                    Id = GenerateCode("PAV"),
                    ProductId = prod3Id,
                    AttributeId = attr2Id,
                    ValueDecimal = 0.5m,
                    DisplayOrder = 1,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },

                // Snack khoai tây Poca 54g
                new ProductAttributeValue
                {
                    Id = GenerateCode("PAV"),
                    ProductId = prod4Id,
                    AttributeId = attr2Id,
                    ValueDecimal = 0.054m,
                    DisplayOrder = 1,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new ProductAttributeValue
                {
                    Id = GenerateCode("PAV"),
                    ProductId = prod4Id,
                    AttributeId = attr3Id,
                    ValueString = "Khoai tây",
                    DisplayOrder = 2,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },

                // Bánh Chocopie 396g
                new ProductAttributeValue
                {
                    Id = GenerateCode("PAV"),
                    ProductId = prod5Id,
                    AttributeId = attr2Id,
                    ValueDecimal = 0.396m,
                    DisplayOrder = 1,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },

                // Kẹo Alpenliebe 24 viên
                new ProductAttributeValue
                {
                    Id = GenerateCode("PAV"),
                    ProductId = prod6Id,
                    AttributeId = attr3Id,
                    ValueString = "Caramel",
                    DisplayOrder = 1,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },

                // Sữa tươi Vinamilk 1L
                new ProductAttributeValue
                {
                    Id = GenerateCode("PAV"),
                    ProductId = prod7Id,
                    AttributeId = attr2Id,
                    ValueDecimal = 1m,
                    DisplayOrder = 1,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },

                // Sữa chua Vinamilk 4 hộp
                new ProductAttributeValue
                {
                    Id = GenerateCode("PAV"),
                    ProductId = prod8Id,
                    AttributeId = attr2Id,
                    ValueDecimal = 0.5m,
                    DisplayOrder = 1,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },

                // Kem đánh răng PS 230g
                new ProductAttributeValue
                {
                    Id = GenerateCode("PAV"),
                    ProductId = prod9Id,
                    AttributeId = attr2Id,
                    ValueDecimal = 0.23m,
                    DisplayOrder = 1,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },

                // Dầu gội Clear 650ml
                new ProductAttributeValue
                {
                    Id = GenerateCode("PAV"),
                    ProductId = prod10Id,
                    AttributeId = attr2Id,
                    ValueDecimal = 0.65m,
                    DisplayOrder = 1,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },

                // Mì Hảo Hảo tôm chua cay (lốc 5)
                new ProductAttributeValue
                {
                    Id = GenerateCode("PAV"),
                    ProductId = prod11Id,
                    AttributeId = attr3Id,
                    ValueString = "Tôm chua cay",
                    DisplayOrder = 1,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },

                // Dầu ăn Simply 1L
                new ProductAttributeValue
                {
                    Id = GenerateCode("PAV"),
                    ProductId = prod12Id,
                    AttributeId = attr2Id,
                    ValueDecimal = 1m,
                    DisplayOrder = 1,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                }
            );

            // Seed Inventories
            modelBuilder.Entity<Inventory>().HasData(
                new Inventory
                {
                    Id = GenerateCode("INV"),
                    Code = "INV001",
                    ProductId = prod1Id,
                    Quantity = 500,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new Inventory
                {
                    Id = GenerateCode("INV"),
                    Code = "INV002",
                    ProductId = prod2Id,
                    Quantity = 450,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new Inventory
                {
                    Id = GenerateCode("INV"),
                    Code = "INV003",
                    ProductId = prod3Id,
                    Quantity = 1000,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new Inventory
                {
                    Id = GenerateCode("INV"),
                    Code = "INV004",
                    ProductId = prod4Id,
                    Quantity = 300,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new Inventory
                {
                    Id = GenerateCode("INV"),
                    Code = "INV005",
                    ProductId = prod5Id,
                    Quantity = 150,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new Inventory
                {
                    Id = GenerateCode("INV"),
                    Code = "INV006",
                    ProductId = prod6Id,
                    Quantity = 200,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new Inventory
                {
                    Id = GenerateCode("INV"),
                    Code = "INV007",
                    ProductId = prod7Id,
                    Quantity = 200,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new Inventory
                {
                    Id = GenerateCode("INV"),
                    Code = "INV008",
                    ProductId = prod8Id,
                    Quantity = 150,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new Inventory
                {
                    Id = GenerateCode("INV"),
                    Code = "INV009",
                    ProductId = prod9Id,
                    Quantity = 100,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new Inventory
                {
                    Id = GenerateCode("INV"),
                    Code = "INV010",
                    ProductId = prod10Id,
                    Quantity = 80,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new Inventory
                {
                    Id = GenerateCode("INV"),
                    Code = "INV011",
                    ProductId = prod11Id,
                    Quantity = 400,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new Inventory
                {
                    Id = GenerateCode("INV"),
                    Code = "INV012",
                    ProductId = prod12Id,
                    Quantity = 120,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                }
            );

            // Seed Promotions
            modelBuilder.Entity<Promotion>().HasData(
                new Promotion
                {
                    Id = promo1Id,
                    Code = "KM001",
                    Description = "Giảm 10% cho đơn hàng từ 200,000đ",
                    DiscountType = "percent",
                    DiscountValue = 10m,
                    StartDate = now.AddDays(-10),
                    EndDate = now.AddDays(20),
                    MinOrderAmount = 200000m,
                    UsageLimit = 100,
                    UsedCount = 0,
                    Status = "active",
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new Promotion
                {
                    Id = promo2Id,
                    Code = "KM002",
                    Description = "Giảm 50,000đ cho đơn hàng từ 500,000đ",
                    DiscountType = "fixed",
                    DiscountValue = 50000m,
                    StartDate = now.AddDays(-5),
                    EndDate = now.AddDays(25),
                    MinOrderAmount = 500000m,
                    UsageLimit = 50,
                    UsedCount = 0,
                    Status = "active",
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