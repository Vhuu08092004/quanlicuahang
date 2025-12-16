using Microsoft.EntityFrameworkCore;
using Quanlicuahang.Models;
using Quanlicuahang.Enum;

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
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<ActionLog> ActionLogs { get; set; }
        public DbSet<StockEntry> StockEntries { get; set; }
        public DbSet<StockEntryItem> StockEntryItems { get; set; }
        public DbSet<Return> Returns { get; set; }
        public DbSet<ReturnItem> ReturnItems { get; set; }
        public DbSet<WarehouseArea> WarehouseAreas { get; set; }
        public DbSet<AreaInventory> AreaInventories { get; set; }
        public DbSet<InvoiceSetting> InvoiceSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureRelationships(modelBuilder);
            ConfigureIndexes(modelBuilder);
            SeedData(modelBuilder);

            modelBuilder
              .Entity<Payment>()
              .Property(p => p.PaymentMethod)
              .HasConversion<string>();

            modelBuilder
                .Entity<Payment>()
                .Property(p => p.PaymentStatus)
                .HasConversion<string>();

            base.OnModelCreating(modelBuilder);
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
                    IsDeleted = false,
                    ImageUrl = "https://res.cloudinary.com/dygibgnym/image/upload/v1765816490/unnamed_znytbl.jpg"
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
                    IsDeleted = false,
                    ImageUrl = "https://res.cloudinary.com/dygibgnym/image/upload/v1765816752/unnamed_cbpw5s.jpg"
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
                    IsDeleted = false,
                    ImageUrl = "https://res.cloudinary.com/dygibgnym/image/upload/v1765816627/unnamed_fj01em.jpg"
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
                    IsDeleted = false,
                    ImageUrl = "https://res.cloudinary.com/dygibgnym/image/upload/v1765816866/unnamed_wxbn4v.jpg"
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
                    IsDeleted = false,
                    ImageUrl = "https://res.cloudinary.com/dygibgnym/image/upload/v1765816922/unnamed_eftmjd.jpg"
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
                    IsDeleted = false,
                    ImageUrl = "https://res.cloudinary.com/dygibgnym/image/upload/v1765817020/unnamed_q8pwc1.jpg"
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
                    CategoryId = cat1Id,
                    SupplierId = sup1Id,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false,
                    ImageUrl = "https://res.cloudinary.com/dygibgnym/image/upload/v1765817527/products/szdbbfgryub3gg3xtaig.jpg"
                },
                new Product
                {
                    Id = prod2Id,
                    Code = "SP002",
                    Name = "Pepsi 330ml",
                    Barcode = "8934588012358",
                    Price = 10000m,
                    Unit = "lon",
                    CategoryId = cat1Id,
                    SupplierId = sup1Id,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false,
                    ImageUrl = "https://res.cloudinary.com/dygibgnym/image/upload/v1765817658/products/wdbiglkrxvyjdybiq7nb.jpg"
                },
                new Product
                {
                    Id = prod3Id,
                    Code = "SP003",
                    Name = "Nước suối Lavie 500ml",
                    Barcode = "8934588012365",
                    Price = 5000m,
                    Unit = "chai",
                    CategoryId = cat1Id,
                    SupplierId = sup1Id,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false,
                    ImageUrl = "https://res.cloudinary.com/dygibgnym/image/upload/v1765817921/shopping_gluvav.webp"
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
                    CategoryId = cat2Id,
                    SupplierId = sup2Id,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false,
                    ImageUrl = "https://res.cloudinary.com/dygibgnym/image/upload/v1765817708/products/dh4re0ynz4y8i1a4jsxr.jpg"
                },
                new Product
                {
                    Id = prod5Id,
                    Code = "SP005",
                    Name = "Bánh Chocopie 396g",
                    Barcode = "8934588012389",
                    Price = 45000m,
                    Unit = "hộp",
                    CategoryId = cat2Id,
                    SupplierId = sup2Id,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false,
                    ImageUrl = "https://res.cloudinary.com/dygibgnym/image/upload/v1765818143/download_pngmcc.jpg"
                },
                new Product
                {
                    Id = prod6Id,
                    Code = "SP006",
                    Name = "Kẹo Alpenliebe 24 viên",
                    Barcode = "8934588012396",
                    Price = 15000m,
                    Unit = "gói",
                    CategoryId = cat2Id,
                    SupplierId = sup2Id,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false,
                    ImageUrl = "https://res.cloudinary.com/dygibgnym/image/upload/v1765817746/products/tswtbvoigplzmghemzkf.jpg"
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
                    CategoryId = cat4Id,
                    SupplierId = sup3Id,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false,
                    ImageUrl = "https://res.cloudinary.com/dygibgnym/image/upload/v1765817121/shopping_izs0wf.webp"
                },
                new Product
                {
                    Id = prod8Id,
                    Code = "SP008",
                    Name = "Sữa chua Vinamilk 4 hộp",
                    Barcode = "8934588012419",
                    Price = 20000m,
                    Unit = "lốc",
                    CategoryId = cat4Id,
                    SupplierId = sup3Id,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false,
                    ImageUrl = "https://res.cloudinary.com/dygibgnym/image/upload/v1765818105/shopping_pxhzfl.webp"
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
                    CategoryId = cat5Id,
                    SupplierId = sup2Id,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false,
                    ImageUrl = "https://res.cloudinary.com/dygibgnym/image/upload/v1765817614/products/sxythlkgi1hwyftyr7or.avif"
                },
                new Product
                {
                    Id = prod10Id,
                    Code = "SP010",
                    Name = "Dầu gội Clear 650ml",
                    Barcode = "8934588012433",
                    Price = 120000m,
                    Unit = "chai",
                    CategoryId = cat5Id,
                    SupplierId = sup2Id,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false,
                    ImageUrl = "https://res.cloudinary.com/dygibgnym/image/upload/v1765818057/shopping_dhr9ar.webp"
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
                    CategoryId = cat6Id,
                    SupplierId = sup2Id,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false,
                    ImageUrl = "https://res.cloudinary.com/dygibgnym/image/upload/v1765817818/products/hft2nscbpgamixspoq2f.webp"
                },
                new Product
                {
                    Id = prod12Id,
                    Code = "SP012",
                    Name = "Dầu ăn Simply 1L",
                    Barcode = "8934588012457",
                    Price = 45000m,
                    Unit = "chai",
                    CategoryId = cat6Id,
                    SupplierId = sup2Id,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false,
                    ImageUrl = "https://res.cloudinary.com/dygibgnym/image/upload/v1765817328/products/w5zxv57s6zhzepqos4le.jpg"
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

            // Seed Promotions
            modelBuilder.Entity<Promotion>().HasData(
                // Khuyến mãi đang diễn ra - Percent với giảm tối đa
                new Promotion
                {
                    Id = promo1Id,
                    Code = "KM001",
                    Description = "Giảm 10% cho đơn hàng từ 200,000đ (tối đa 50,000đ)",
                    DiscountType = "percent",
                    DiscountValue = 10m,
                    MaxDiscount = 50000m,
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
                // Khuyến mãi đang diễn ra - Fixed
                new Promotion
                {
                    Id = promo2Id,
                    Code = "KM002",
                    Description = "Giảm 50,000đ cho đơn hàng từ 500,000đ",
                    DiscountType = "fixed",
                    DiscountValue = 50000m,
                    MaxDiscount = 0m,
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
                },
                // Khuyến mãi chờ diễn ra (upcoming)
                new Promotion
                {
                    Id = GenerateCode("promo"),
                    Code = "KM003",
                    Description = "Giảm 15% cho đơn hàng từ 300,000đ (tối đa 100,000đ)",
                    DiscountType = "percent",
                    DiscountValue = 15m,
                    MaxDiscount = 100000m,
                    StartDate = now.AddDays(5),
                    EndDate = now.AddDays(35),
                    MinOrderAmount = 300000m,
                    UsageLimit = 200,
                    UsedCount = 0,
                    Status = "active",
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                // Khuyến mãi tạm dừng (paused)
                new Promotion
                {
                    Id = GenerateCode("promo"),
                    Code = "KM004",
                    Description = "Giảm 20% cho đơn hàng từ 400,000đ (tối đa 150,000đ)",
                    DiscountType = "percent",
                    DiscountValue = 20m,
                    MaxDiscount = 150000m,
                    StartDate = now.AddDays(-3),
                    EndDate = now.AddDays(15),
                    MinOrderAmount = 400000m,
                    UsageLimit = 80,
                    UsedCount = 0,
                    Status = "inactive",
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                // Khuyến mãi đã kết thúc (expired)
                new Promotion
                {
                    Id = GenerateCode("promo"),
                    Code = "KM005",
                    Description = "Giảm 100,000đ cho đơn hàng từ 1,000,000đ",
                    DiscountType = "fixed",
                    DiscountValue = 100000m,
                    MaxDiscount = 0m,
                    StartDate = now.AddDays(-30),
                    EndDate = now.AddDays(-5),
                    MinOrderAmount = 1000000m,
                    UsageLimit = 30,
                    UsedCount = 25,
                    Status = "active",
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                // Khuyến mãi percent không giới hạn giảm tối đa
                new Promotion
                {
                    Id = GenerateCode("promo"),
                    Code = "KM006",
                    Description = "Giảm 5% không giới hạn cho mọi đơn hàng",
                    DiscountType = "percent",
                    DiscountValue = 5m,
                    MaxDiscount = 0m,
                    StartDate = now.AddDays(-15),
                    EndDate = now.AddDays(45),
                    MinOrderAmount = 0m,
                    UsageLimit = 0,
                    UsedCount = 0,
                    Status = "active",
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                }
            );

            // Seed WarehouseAreas
            var wa1Id = GenerateSeedId("WA", 1);
            var wa2Id = GenerateSeedId("WA", 2);
            var wa3Id = GenerateSeedId("WA", 3);

            modelBuilder.Entity<WarehouseArea>().HasData(
                new WarehouseArea
                {
                    Id = wa1Id,
                    Code = "KVA001",
                    Name = "Khu vực A",
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new WarehouseArea
                {
                    Id = wa2Id,
                    Code = "KVB001",
                    Name = "Khu vực B",
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new WarehouseArea
                {
                    Id = wa3Id,
                    Code = "KVC001",
                    Name = "Khu vực C",
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                }
            );

            // Seed AreaInventories
            // Số lượng = StockEntryItems (nhập) - OrderItems (xuất)
            // AI1: prod1Id, wa1Id: SEI1 nhập 500 - OI1 xuất 10 = 490
            // AI2: prod2Id, wa1Id: SEI2 nhập 450 - OI6 xuất 3 = 447
            // AI3: prod3Id, wa1Id: SEI3 nhập 1000 - OI2 xuất 10 - OI7 xuất 3 = 987
            // AI4: prod4Id, wa2Id: SEI4 nhập 300 - OI3 xuất 5 = 295
            // AI5: prod5Id, wa2Id: SEI5 nhập 150 - không xuất = 150
            // AI6: prod6Id, wa2Id: SEI6 nhập 200 - OI4 xuất 1 = 199
            // AI7: prod7Id, wa3Id: SEI7 nhập 200 - không xuất = 200
            // AI8: prod8Id, wa3Id: SEI8 nhập 150 - OI5 xuất 1 = 149
            modelBuilder.Entity<AreaInventory>().HasData(
                new AreaInventory
                {
                    Id = GenerateSeedId("AI", 1),
                    WarehouseAreaId = wa1Id,
                    ProductId = prod1Id,
                    Quantity = 490, // SEI1: 500 - OI1: 10
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new AreaInventory
                {
                    Id = GenerateSeedId("AI", 2),
                    WarehouseAreaId = wa1Id,
                    ProductId = prod2Id,
                    Quantity = 447, // SEI2: 450 - OI6: 3
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new AreaInventory
                {
                    Id = GenerateSeedId("AI", 3),
                    WarehouseAreaId = wa1Id,
                    ProductId = prod3Id,
                    Quantity = 987, // SEI3: 1000 - OI2: 10 - OI7: 3
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new AreaInventory
                {
                    Id = GenerateSeedId("AI", 4),
                    WarehouseAreaId = wa2Id,
                    ProductId = prod4Id,
                    Quantity = 295, // SEI4: 300 - OI3: 5
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new AreaInventory
                {
                    Id = GenerateSeedId("AI", 5),
                    WarehouseAreaId = wa2Id,
                    ProductId = prod5Id,
                    Quantity = 150, // SEI5: 150 - không xuất
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new AreaInventory
                {
                    Id = GenerateSeedId("AI", 6),
                    WarehouseAreaId = wa2Id,
                    ProductId = prod6Id,
                    Quantity = 199, // SEI6: 200 - OI4: 1
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new AreaInventory
                {
                    Id = GenerateSeedId("AI", 7),
                    WarehouseAreaId = wa3Id,
                    ProductId = prod7Id,
                    Quantity = 200, // SEI7: 200 - không xuất
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new AreaInventory
                {
                    Id = GenerateSeedId("AI", 8),
                    WarehouseAreaId = wa3Id,
                    ProductId = prod8Id,
                    Quantity = 149, // SEI8: 150 - OI5: 1
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                    IsDeleted = false
                }
            );

            // Seed StockEntries
            var se1Id = GenerateSeedId("SE", 1);
            var se2Id = GenerateSeedId("SE", 2);

            modelBuilder.Entity<StockEntry>().HasData(
                new StockEntry
                {
                    Id = se1Id,
                    Code = "PN001",
                    SupplierId = sup1Id,
                    EntryDate = now.AddDays(-5),
                    Status = "completed",
                    TotalCost = 12600000m, // 4000000 + 3600000 + 5000000
                    Note = "Nhập hàng đầu tháng",
                    UserId = stockUserId,
                    CreatedAt = now.AddDays(-5),
                    CreatedBy = "System",
                    UpdatedAt = now.AddDays(-5),
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new StockEntry
                {
                    Id = se2Id,
                    Code = "PN002",
                    SupplierId = sup2Id,
                    EntryDate = now.AddDays(-2),
                    Status = "completed",
                    TotalCost = 10450000m, // 3000000 + 1800000 + 2400000 + 3000000 + 2250000
                    Note = "Nhập hàng snack và bánh kẹo",
                    UserId = stockUserId,
                    CreatedAt = now.AddDays(-2),
                    CreatedBy = "System",
                    UpdatedAt = now.AddDays(-2),
                    UpdatedBy = "System",
                    IsDeleted = false
                }
            );

            // Seed StockEntryItems
            // SEI1: prod1Id, wa1Id, Quantity = 500 → AreaInventory AI1 ban đầu = 500, sau khi xuất 10 còn 490
            // SEI2: prod2Id, wa1Id, Quantity = 450 → AreaInventory AI2 ban đầu = 450, sau khi xuất 3 còn 447
            // SEI3: prod4Id, wa2Id, Quantity = 300 → AreaInventory AI4 ban đầu = 300, sau khi xuất 5 còn 295
            // SEI4: prod3Id, wa1Id, Quantity = 1000 → AreaInventory AI3 ban đầu = 1000, sau khi xuất 13 còn 987
            // SEI5: prod5Id, wa2Id, Quantity = 150 → AreaInventory AI5 = 150
            // SEI6: prod6Id, wa2Id, Quantity = 200 → AreaInventory AI6 ban đầu = 200, sau khi xuất 1 còn 199
            // SEI7: prod7Id, wa3Id, Quantity = 200 → AreaInventory AI7 = 200
            // SEI8: prod8Id, wa3Id, Quantity = 150 → AreaInventory AI8 ban đầu = 150, sau khi xuất 1 còn 149
            modelBuilder.Entity<StockEntryItem>().HasData(
                new StockEntryItem
                {
                    Id = GenerateSeedId("SEI", 1),
                    StockEntryId = se1Id,
                    ProductId = prod1Id,
                    Quantity = 500,
                    UnitCost = 8000m,
                    Subtotal = 4000000m,
                    WarehouseAreaId = wa1Id,
                    CreatedAt = now.AddDays(-5),
                    CreatedBy = "System",
                    UpdatedAt = now.AddDays(-5),
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new StockEntryItem
                {
                    Id = GenerateSeedId("SEI", 2),
                    StockEntryId = se1Id,
                    ProductId = prod2Id,
                    Quantity = 450,
                    UnitCost = 8000m,
                    Subtotal = 3600000m,
                    WarehouseAreaId = wa1Id,
                    CreatedAt = now.AddDays(-5),
                    CreatedBy = "System",
                    UpdatedAt = now.AddDays(-5),
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new StockEntryItem
                {
                    Id = GenerateSeedId("SEI", 3),
                    StockEntryId = se1Id,
                    ProductId = prod3Id,
                    Quantity = 1000,
                    UnitCost = 5000m,
                    Subtotal = 5000000m,
                    WarehouseAreaId = wa1Id,
                    CreatedAt = now.AddDays(-5),
                    CreatedBy = "System",
                    UpdatedAt = now.AddDays(-5),
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new StockEntryItem
                {
                    Id = GenerateSeedId("SEI", 4),
                    StockEntryId = se2Id,
                    ProductId = prod4Id,
                    Quantity = 300,
                    UnitCost = 10000m,
                    Subtotal = 3000000m,
                    WarehouseAreaId = wa2Id,
                    CreatedAt = now.AddDays(-2),
                    CreatedBy = "System",
                    UpdatedAt = now.AddDays(-2),
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new StockEntryItem
                {
                    Id = GenerateSeedId("SEI", 5),
                    StockEntryId = se2Id,
                    ProductId = prod5Id,
                    Quantity = 150,
                    UnitCost = 12000m,
                    Subtotal = 1800000m,
                    WarehouseAreaId = wa2Id,
                    CreatedAt = now.AddDays(-2),
                    CreatedBy = "System",
                    UpdatedAt = now.AddDays(-2),
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new StockEntryItem
                {
                    Id = GenerateSeedId("SEI", 6),
                    StockEntryId = se2Id,
                    ProductId = prod6Id,
                    Quantity = 200,
                    UnitCost = 12000m,
                    Subtotal = 2400000m,
                    WarehouseAreaId = wa2Id,
                    CreatedAt = now.AddDays(-2),
                    CreatedBy = "System",
                    UpdatedAt = now.AddDays(-2),
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new StockEntryItem
                {
                    Id = GenerateSeedId("SEI", 7),
                    StockEntryId = se2Id,
                    ProductId = prod7Id,
                    Quantity = 200,
                    UnitCost = 15000m,
                    Subtotal = 3000000m,
                    WarehouseAreaId = wa3Id,
                    CreatedAt = now.AddDays(-2),
                    CreatedBy = "System",
                    UpdatedAt = now.AddDays(-2),
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new StockEntryItem
                {
                    Id = GenerateSeedId("SEI", 8),
                    StockEntryId = se2Id,
                    ProductId = prod8Id,
                    Quantity = 150,
                    UnitCost = 15000m,
                    Subtotal = 2250000m,
                    WarehouseAreaId = wa3Id,
                    CreatedAt = now.AddDays(-2),
                    CreatedBy = "System",
                    UpdatedAt = now.AddDays(-2),
                    UpdatedBy = "System",
                    IsDeleted = false
                }
            );

            // Seed Orders
            var order1Id = GenerateSeedId("ORD", 1);
            var order2Id = GenerateSeedId("ORD", 2);
            var order3Id = GenerateSeedId("ORD", 3);

            modelBuilder.Entity<Order>().HasData(
                new Order
                {
                    Id = order1Id,
                    Code = "DH001",
                    CustomerId = cust2Id,
                    UserId = cashier1UserId,
                    PromoId = promo1Id,
                    OrderDate = now.AddDays(-3),
                    Status = OrderStatus.Paid,
                    TotalAmount = 150000m,
                    DiscountAmount = 15000m,
                    PaidAmount = 135000m,
                    Info = null,
                    CreatedAt = now.AddDays(-3),
                    CreatedBy = "System",
                    UpdatedAt = now.AddDays(-3),
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new Order
                {
                    Id = order2Id,
                    Code = "DH002",
                    CustomerId = cust3Id,
                    UserId = cashier1UserId,
                    PromoId = null,
                    OrderDate = now.AddDays(-1),
                    Status = OrderStatus.Confirmed,
                    TotalAmount = 85000m,
                    DiscountAmount = 0m,
                    PaidAmount = 0m,
                    Info = null,
                    CreatedAt = now.AddDays(-1),
                    CreatedBy = "System",
                    UpdatedAt = now.AddDays(-1),
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new Order
                {
                    Id = order3Id,
                    Code = "DH003",
                    CustomerId = cust1Id,
                    UserId = cashier1UserId,
                    PromoId = null,
                    OrderDate = now.AddHours(-2),
                    Status = OrderStatus.Pending,
                    TotalAmount = 45000m,
                    DiscountAmount = 0m,
                    PaidAmount = 0m,
                    Info = null,
                    CreatedAt = now.AddHours(-2),
                    CreatedBy = "System",
                    UpdatedAt = now.AddHours(-2),
                    UpdatedBy = "System",
                    IsDeleted = false
                }
            );

            // Seed OrderItems
            modelBuilder.Entity<OrderItem>().HasData(
                new OrderItem
                {
                    Id = GenerateSeedId("OI", 1),
                    OrderId = order1Id,
                    ProductId = prod1Id,
                    Quantity = 10,
                    Price = 10000m,
                    Subtotal = 100000m,
                    CreatedAt = now.AddDays(-3),
                    CreatedBy = "System",
                    UpdatedAt = now.AddDays(-3),
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new OrderItem
                {
                    Id = GenerateSeedId("OI", 2),
                    OrderId = order1Id,
                    ProductId = prod3Id,
                    Quantity = 10,
                    Price = 5000m,
                    Subtotal = 50000m,
                    CreatedAt = now.AddDays(-3),
                    CreatedBy = "System",
                    UpdatedAt = now.AddDays(-3),
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new OrderItem
                {
                    Id = GenerateSeedId("OI", 3),
                    OrderId = order2Id,
                    ProductId = prod4Id,
                    Quantity = 5,
                    Price = 12000m,
                    Subtotal = 60000m,
                    CreatedAt = now.AddDays(-1),
                    CreatedBy = "System",
                    UpdatedAt = now.AddDays(-1),
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new OrderItem
                {
                    Id = GenerateSeedId("OI", 4),
                    OrderId = order2Id,
                    ProductId = prod6Id,
                    Quantity = 1,
                    Price = 15000m,
                    Subtotal = 15000m,
                    CreatedAt = now.AddDays(-1),
                    CreatedBy = "System",
                    UpdatedAt = now.AddDays(-1),
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new OrderItem
                {
                    Id = GenerateSeedId("OI", 5),
                    OrderId = order2Id,
                    ProductId = prod8Id,
                    Quantity = 1,
                    Price = 20000m,
                    Subtotal = 20000m,
                    CreatedAt = now.AddDays(-1),
                    CreatedBy = "System",
                    UpdatedAt = now.AddDays(-1),
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new OrderItem
                {
                    Id = GenerateSeedId("OI", 6),
                    OrderId = order3Id,
                    ProductId = prod2Id,
                    Quantity = 3,
                    Price = 10000m,
                    Subtotal = 30000m,
                    CreatedAt = now.AddHours(-2),
                    CreatedBy = "System",
                    UpdatedAt = now.AddHours(-2),
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new OrderItem
                {
                    Id = GenerateSeedId("OI", 7),
                    OrderId = order3Id,
                    ProductId = prod3Id,
                    Quantity = 3,
                    Price = 5000m,
                    Subtotal = 15000m,
                    CreatedAt = now.AddHours(-2),
                    CreatedBy = "System",
                    UpdatedAt = now.AddHours(-2),
                    UpdatedBy = "System",
                    IsDeleted = false
                }
            );

            // Seed Payments
            modelBuilder.Entity<Payment>().HasData(
                new Payment
                {
                    Id = GenerateSeedId("PAY", 1),
                    Code = "TT001",
                    OrderId = order1Id,
                    Amount = 135000m,
                    PaymentMethod = PaymentMethod.Cash,
                    PaymentStatus = PaymentStatus.Completed,
                    PaymentDate = now.AddDays(-3),
                    TransactionRef = null,
                    Note = "Thanh toán tiền mặt",
                    IsAutoGenerated = false,
                    CreatedAt = now.AddDays(-3),
                    CreatedBy = "System",
                    UpdatedAt = now.AddDays(-3),
                    UpdatedBy = "System",
                    IsDeleted = false
                },
                new Payment
                {
                    Id = GenerateSeedId("PAY", 2),
                    Code = "TT002",
                    OrderId = order2Id,
                    Amount = 50000m,
                    PaymentMethod = PaymentMethod.BankTransfer,
                    PaymentStatus = PaymentStatus.Pending,
                    PaymentDate = now.AddDays(-1),
                    TransactionRef = "TRF20251213001",
                    Note = "Chuyển khoản ngân hàng",
                    IsAutoGenerated = false,
                    CreatedAt = now.AddDays(-1),
                    CreatedBy = "System",
                    UpdatedAt = now.AddDays(-1),
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

        private static string GenerateSeedId(string prefix, int seedNumber)
        {
            // Sử dụng timestamp cố định cho seed data và randomPart dựa trên seedNumber
            var baseTimestamp = 1734172800000; // Timestamp cố định cho seed data
            var randomPart = seedNumber.ToString("X8").ToLower().PadLeft(8, '0');
            return $"{prefix.ToLower()}-{baseTimestamp}-{randomPart}";
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