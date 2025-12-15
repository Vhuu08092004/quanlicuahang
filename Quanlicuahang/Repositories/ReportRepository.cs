using Microsoft.EntityFrameworkCore;
using Quanlicuahang.Data;
using Quanlicuahang.DTOs.Report;
using Quanlicuahang.Enum;

namespace Quanlicuahang.Repositories
{
    // Interface cho ReportRepository
    public interface IReportRepository
    {
        Task<object> GetRevenueByDayAsync(DateTime date, int skip, int take);
        Task<object> GetRevenueByMonthAsync(int year, int month, int skip, int take);
        Task<object> GetRevenueByYearAsync(int year, int skip, int take);
        Task<object> GetRevenueByEmployeeAsync(DateTime? fromDate, DateTime? toDate, int skip, int take);
        Task<object> GetRevenueByCustomerAsync(DateTime? fromDate, DateTime? toDate, string? region, int skip, int take);
        Task<object> GetTopSellingProductsAsync(int topN, DateTime? fromDate, DateTime? toDate, int skip, int take);
        Task<object> GetInventoryReportAsync(int skip, int take);
    }

    // Implementation của ReportRepository, lấy thẳng từ DbContext
    public class ReportRepository : IReportRepository
    {
        private readonly ApplicationDbContext _context;

        public ReportRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Báo cáo doanh thu theo ngày (net revenue: total - discount - refund)
        public async Task<object> GetRevenueByDayAsync(DateTime date, int skip, int take)
        {
            skip = skip < 0 ? 0 : skip;
            take = take <= 0 ? 10 : take;

            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

            // Get orders revenue grouped by date
            var ordersQuery = from o in _context.Orders
                              where o.OrderDate >= startOfDay && o.OrderDate <= endOfDay
                                    && o.Status == OrderStatus.Paid
                                    && o.IsDeleted == false
                              group o by o.OrderDate.Date into g
                              select new
                              {
                                  Date = g.Key,
                                  TotalRevenue = g.Sum(x => x.TotalAmount - x.DiscountAmount),
                                  OrderCount = g.Count()
                              };

            // Get returns grouped by date
            var returnsQuery = from r in _context.Returns
                               join o in _context.Orders on r.OrderId equals o.Id
                               where o.OrderDate >= startOfDay && o.OrderDate <= endOfDay
                                     && o.IsDeleted == false
                                     && r.IsDeleted == false
                               group r by o.OrderDate.Date into g
                               select new
                               {
                                   Date = g.Key,
                                   TotalRefund = g.Sum(x => x.RefundAmount)
                               };

            var orders = await ordersQuery.AsNoTracking().ToListAsync();
            var returns = await returnsQuery.AsNoTracking().ToListAsync();

            // Combine the results
            var allResults = orders.Select(o => new RevenueReportDto
            {
                Date = o.Date,
                TotalRevenue = o.TotalRevenue - (returns.FirstOrDefault(r => r.Date == o.Date)?.TotalRefund ?? 0),
                OrderCount = o.OrderCount,
                AvgOrderValue = (float)((o.OrderCount == 0) ? 0 :
                    (o.TotalRevenue - (returns.FirstOrDefault(r => r.Date == o.Date)?.TotalRefund ?? 0)) / (decimal)o.OrderCount)
            }).ToList();

            var total = allResults.Count;
            var data = allResults.Skip(skip).Take(take).ToList();

            return new { data, total };
        }

        // Báo cáo doanh thu theo tháng
        public async Task<object> GetRevenueByMonthAsync(int year, int month, int skip, int take)
        {
            skip = skip < 0 ? 0 : skip;
            take = take <= 0 ? 10 : take;

            var startOfMonth = new DateTime(year, month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1).Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            // Get orders revenue grouped by day
            var ordersQuery = from o in _context.Orders
                              where o.OrderDate >= startOfMonth && o.OrderDate <= endOfMonth
                                    && o.Status == OrderStatus.Paid
                                    && o.IsDeleted == false
                              group o by new { o.OrderDate.Year, o.OrderDate.Month, o.OrderDate.Day } into g
                              select new
                              {
                                  Year = g.Key.Year,
                                  Month = g.Key.Month,
                                  Day = g.Key.Day,
                                  TotalRevenue = g.Sum(x => x.TotalAmount - x.DiscountAmount),
                                  OrderCount = g.Count()
                              };

            // Get returns grouped by day
            var returnsQuery = from r in _context.Returns
                               join o in _context.Orders on r.OrderId equals o.Id
                               where o.OrderDate >= startOfMonth && o.OrderDate <= endOfMonth
                                     && o.IsDeleted == false
                                     && r.IsDeleted == false
                               group r by new { o.OrderDate.Year, o.OrderDate.Month, o.OrderDate.Day } into g
                               select new
                               {
                                   Year = g.Key.Year,
                                   Month = g.Key.Month,
                                   Day = g.Key.Day,
                                   TotalRefund = g.Sum(x => x.RefundAmount)
                               };

            var orders = await ordersQuery.AsNoTracking().ToListAsync();
            var returns = await returnsQuery.AsNoTracking().ToListAsync();

            // Combine the results
            var allResults = orders.Select(o => new RevenueReportDto
            {
                Date = new DateTime(o.Year, o.Month, o.Day),
                TotalRevenue = o.TotalRevenue - (returns.FirstOrDefault(r => r.Year == o.Year && r.Month == o.Month && r.Day == o.Day)?.TotalRefund ?? 0),
                OrderCount = o.OrderCount,
                AvgOrderValue = (float)((o.OrderCount == 0) ? 0 :
                        (o.TotalRevenue - (returns.FirstOrDefault(r => r.Year == o.Year && r.Month == o.Month && r.Day == o.Day)?.TotalRefund ?? 0)) / (decimal)o.OrderCount)
            }).OrderBy(x => x.Date).ToList();

            var total = allResults.Count;
            var data = allResults.Skip(skip).Take(take).ToList();

            return new { data, total };
        }

        // Báo cáo doanh thu theo năm (theo tháng trong năm)
        public async Task<object> GetRevenueByYearAsync(int year, int skip, int take)
        {
            skip = skip < 0 ? 0 : skip;
            take = take <= 0 ? 10 : take;

            var startOfYear = new DateTime(year, 1, 1);
            var endOfYear = new DateTime(year, 12, 31, 23, 59, 59);

            // Get orders revenue grouped by month
            var ordersQuery = from o in _context.Orders
                              where o.OrderDate >= startOfYear && o.OrderDate <= endOfYear
                                    && o.Status == OrderStatus.Paid
                                    && o.IsDeleted == false
                              group o by new { o.OrderDate.Year, o.OrderDate.Month } into g
                              select new
                              {
                                  Year = g.Key.Year,
                                  Month = g.Key.Month,
                                  TotalRevenue = g.Sum(x => x.TotalAmount - x.DiscountAmount),
                                  OrderCount = g.Count()
                              };

            // Get returns grouped by month
            var returnsQuery = from r in _context.Returns
                               join o in _context.Orders on r.OrderId equals o.Id
                               where o.OrderDate >= startOfYear && o.OrderDate <= endOfYear
                                     && o.IsDeleted == false
                                     && r.IsDeleted == false
                               group r by new { o.OrderDate.Year, o.OrderDate.Month } into g
                               select new
                               {
                                   Year = g.Key.Year,
                                   Month = g.Key.Month,
                                   TotalRefund = g.Sum(x => x.RefundAmount)
                               };

            var orders = await ordersQuery.AsNoTracking().ToListAsync();
            var returns = await returnsQuery.AsNoTracking().ToListAsync();

            // Combine the results
            var allResults = orders.Select(o => new RevenueReportDto
            {
                Date = new DateTime(o.Year, o.Month, 1), // Ngày đầu tháng
                TotalRevenue = o.TotalRevenue - (returns.FirstOrDefault(r => r.Year == o.Year && r.Month == o.Month)?.TotalRefund ?? 0),
                OrderCount = o.OrderCount,
                AvgOrderValue = (float)((o.OrderCount == 0) ? 0 : (o.TotalRevenue - (returns.FirstOrDefault(r => r.Year == o.Year && r.Month == o.Month)?.TotalRefund ?? 0)) / (decimal)o.OrderCount)


            }).OrderBy(x => x.Date).ToList();

            var total = allResults.Count;
            var data = allResults.Skip(skip).Take(take).ToList();

            return new { data, total };
        }

        public async Task<object> GetRevenueByEmployeeAsync(DateTime? fromDate, DateTime? toDate, int skip, int take)
        {
            skip = skip < 0 ? 0 : skip;
            take = take <= 0 ? 10 : take;

            var realToDate = toDate.HasValue ? toDate.Value.AddDays(1) : (DateTime?)null;
            
            // Lấy tất cả nhân viên và tính tổng doanh thu (kể cả không có đơn)
            var ordersQuery =
                from e in _context.Employees
                join u in _context.Users on e.Id equals u.EmployeeId into userGroup
                from u in userGroup.DefaultIfEmpty()
                join o in _context.Orders on u.Id equals o.UserId into orderGroup
                from o in orderGroup.DefaultIfEmpty()
                group new { o } by new { e.Id, e.Name } into g
                select new
                {
                    EmployeeId = g.Key.Id,
                    EmployeeName = g.Key.Name,
                    TotalRevenue = g
                        .Where(x =>
                            x.o != null &&
                            (fromDate == null || x.o.OrderDate >= fromDate) &&
                            (toDate == null || x.o.OrderDate < realToDate) &&
                            x.o.Status == OrderStatus.Paid &&
                            !x.o.IsDeleted
                        )
                        .Sum(x => (decimal?)(x.o.TotalAmount - x.o.DiscountAmount)) ?? 0,
                    OrderCount = g
                        .Where(x =>
                            x.o != null &&
                            (fromDate == null || x.o.OrderDate >= fromDate) &&
                            (toDate == null || x.o.OrderDate < realToDate) &&
                            x.o.Status == OrderStatus.Paid &&
                            !x.o.IsDeleted
                        )
                        .Select(x => x.o.Id)
                        .Distinct()
                        .Count()
                };

            // Lấy tổng tiền hoàn (returns)
            var returnsQuery =
                from e in _context.Employees
                join u in _context.Users on e.Id equals u.EmployeeId into userGroup
                from u in userGroup.DefaultIfEmpty()
                join o in _context.Orders on u.Id equals o.UserId into orderGroup
                from o in orderGroup.DefaultIfEmpty()
                join r in _context.Returns on o.Id equals r.OrderId into returnGroup
                from r in returnGroup.DefaultIfEmpty()
                group new { o, r } by new { e.Id, e.Name } into g
                select new
                {
                    EmployeeId = g.Key.Id,
                    EmployeeName = g.Key.Name,
                    TotalRefund = g
                        .Where(x =>
                            x.o != null &&
                            (fromDate == null || x.o.OrderDate >= fromDate) &&
                            (toDate == null || x.o.OrderDate < realToDate) &&
                            x.o.IsDeleted == false &&
                            x.r != null &&
                            x.r.IsDeleted == false
                        )
                        .Sum(x => (decimal?)x.r.RefundAmount) ?? 0
                };

            var orders = await ordersQuery.AsNoTracking().ToListAsync();
            var returns = await returnsQuery.AsNoTracking().ToListAsync();

            // Ghép kết quả, đảm bảo mọi nhân viên đều có mặt
            var allResults = (
                from o in orders
                join r in returns on o.EmployeeId equals r.EmployeeId into returnGroup
                from r in returnGroup.DefaultIfEmpty()
                select new RevenueByEmployeeDto
                {
                    EmployeeName = o.EmployeeName,
                    TotalRevenue = o.TotalRevenue - (r?.TotalRefund ?? 0),
                    TotalPayment = o.TotalRevenue - (r?.TotalRefund ?? 0), // TotalPayment = TotalRevenue sau khi trừ refund
                    OrderCount = o.OrderCount
                }
            )
            .OrderByDescending(x => x.TotalRevenue)
            .ToList();

            var total = allResults.Count;
            var data = allResults.Skip(skip).Take(take).ToList();

            return new { data, total };
        }



        // Báo cáo doanh thu theo khách hàng
        public async Task<object> GetRevenueByCustomerAsync(DateTime? fromDate, DateTime? toDate, string? region, int skip, int take)
        {
            skip = skip < 0 ? 0 : skip;
            take = take <= 0 ? 10 : take;
            var realToDate = toDate.HasValue ? toDate.Value.AddDays(1) : (DateTime?)null;

            // Chuẩn hóa region string để so sánh không phân biệt hoa thường
            var normalizedRegion = region?.Trim().ToLower();

            // Get orders revenue grouped by customer
            var ordersQuery = from o in _context.Orders
                              join c in _context.Customers on o.CustomerId equals c.Id
                              where (fromDate == null || o.OrderDate >= fromDate)
                                    && (toDate == null || o.OrderDate < realToDate)
                                    && o.Status == OrderStatus.Paid
                                    && o.IsDeleted == false
                                    // Lọc theo khu vực nếu được chỉ định
                                    && (string.IsNullOrEmpty(normalizedRegion)
                                        || (normalizedRegion == "hà nội" && c.Address.ToLower().Contains("hà nội"))
                                        || (normalizedRegion == "hồ chí minh" && (c.Address.ToLower().Contains("hồ chí minh") || c.Address.ToLower().Contains("tp.hcm") || c.Address.ToLower().Contains("sài gòn")))
                                        || (normalizedRegion == "đà nẵng" && c.Address.ToLower().Contains("đà nẵng")))
                              group new { o, c } by new { c.Id, c.Name, c.Address } into g
                              select new
                              {
                                  CustomerId = g.Key.Id,
                                  CustomerName = g.Key.Name,
                                  CustomerAddress = g.Key.Address,
                                  TotalRevenue = g.Sum(x => x.o.TotalAmount - x.o.DiscountAmount),
                                  OrderCount = g.Count()
                              };

            // Get returns grouped by customer
            var returnsQuery = from r in _context.Returns
                               join o in _context.Orders on r.OrderId equals o.Id
                               join c in _context.Customers on o.CustomerId equals c.Id
                               where (fromDate == null || o.OrderDate >= fromDate)
                                     && (toDate == null || o.OrderDate < realToDate)
                                     && o.IsDeleted == false
                                     && r.IsDeleted == false
                                     // Lọc theo khu vực nếu được chỉ định
                                     && (string.IsNullOrEmpty(normalizedRegion)
                                         || (normalizedRegion == "hà nội" && c.Address.ToLower().Contains("hà nội"))
                                         || (normalizedRegion == "hồ chí minh" && (c.Address.ToLower().Contains("hồ chí minh") || c.Address.ToLower().Contains("tp.hcm") || c.Address.ToLower().Contains("sài gòn")))
                                         || (normalizedRegion == "đà nẵng" && c.Address.ToLower().Contains("đà nẵng")))
                               group new { r, c } by new { c.Id, c.Name } into g
                               select new
                               {
                                   CustomerId = g.Key.Id,
                                   CustomerName = g.Key.Name,
                                   TotalRefund = g.Sum(x => x.r.RefundAmount)
                               };

            var orders = await ordersQuery.AsNoTracking().ToListAsync();
            var returns = await returnsQuery.AsNoTracking().ToListAsync();

            // Combine the results
            var allResults = orders.Select(o =>
            {
                var refund = returns.FirstOrDefault(r => r.CustomerId == o.CustomerId)?.TotalRefund ?? 0;
                var netRevenue = o.TotalRevenue - refund;

                return new RevenueByCustomerDto
                {
                    CustomerName = o.CustomerName,
                    TotalRevenue = netRevenue,
                    OrderCount = o.OrderCount,
                    AverageOrder = o.OrderCount > 0 ? (float)(netRevenue / o.OrderCount) : 0
                };
            }).OrderByDescending(x => x.TotalRevenue).ToList();

            var total = allResults.Count;
            var data = allResults.Skip(skip).Take(take).ToList();

            return new { data, total };
        }

        // Báo cáo sản phẩm bán chạy (top theo quantity hoặc revenue)
        public async Task<object> GetTopSellingProductsAsync(int topN, DateTime? fromDate, DateTime? toDate, int skip, int take)
        {
            skip = skip < 0 ? 0 : skip;
            take = take <= 0 ? 10 : take;

            // Query để lấy số lượng đã bán
            var soldQuery = from oi in _context.OrderItems
                            join o in _context.Orders on oi.OrderId equals o.Id
                            join prod in _context.Products on oi.ProductId equals prod.Id
                            join cat in _context.Categories on prod.CategoryId equals cat.Id into categories
                            from category in categories.DefaultIfEmpty()
                            where (fromDate == null || o.OrderDate >= fromDate)
                                  && (toDate == null || o.OrderDate <= toDate)
                                  && o.Status == OrderStatus.Paid
                                  && o.IsDeleted == false
                            group new { oi, category } by new { prod.Id, prod.Name, CategoryName = category != null ? category.Name : "Kh�ng ph�n lo?i" } into g
                            select new
                            {
                                ProductId = g.Key.Id,
                                ProductName = g.Key.Name,
                                CategoryName = g.Key.CategoryName,
                                TotalQuantity = g.Sum(x => x.oi.Quantity),
                                TotalRevenue = g.Sum(x => x.oi.Subtotal)
                            };

            // Query để lấy số lượng tồn kho từ AreaInventory
            var inventoryQuery = from ai in _context.AreaInventories
                                 where !ai.IsDeleted
                                 group ai by ai.ProductId into g
                                 select new
                                 {
                                     ProductId = g.Key,
                                     StockQuantity = g.Sum(x => x.Quantity)
                                 };

            var soldProducts = await soldQuery.AsNoTracking().ToListAsync();
            var inventories = await inventoryQuery.AsNoTracking().ToListAsync();

            // Kết hợp dữ liệu
            var allResults = soldProducts.Select(sp => new TopSellingProductDto
            {
                ProductName = sp.ProductName,
                CategoryName = sp.CategoryName,
                TotalQuantity = sp.TotalQuantity,
                TotalRevenue = sp.TotalRevenue,
                StockQuantity = inventories.FirstOrDefault(i => i.ProductId == sp.ProductId)?.StockQuantity ?? 0
            })
            .OrderByDescending(x => x.TotalQuantity)
            .Take(topN)
            .ToList();

            var total = allResults.Count;
            var data = allResults.Skip(skip).Take(take).ToList();

            return new { data, total };
        }

        // Báo cáo sản phẩm tồn kho
        public async Task<object> GetInventoryReportAsync(int skip, int take)
        {
            skip = skip < 0 ? 0 : skip;
            take = take <= 0 ? 10 : take;

            // Query để lấy tồn kho với thông tin sản phẩm từ AreaInventory
            var inventoryQuery = from ai in _context.AreaInventories
                                 join prod in _context.Products on ai.ProductId equals prod.Id
                                 join cat in _context.Categories on prod.CategoryId equals cat.Id into categories
                                 from category in categories.DefaultIfEmpty()
                                 where ai.IsDeleted == false && prod.IsDeleted == false
                                 group new { ai, prod, category } by new
                                 {
                                     prod.Id,
                                     prod.Name,
                                     prod.Price,
                                     CategoryName = category != null ? category.Name : "Không phân loại"
                                 } into g
                                 select new
                                 {
                                     ProductId = g.Key.Id,
                                     ProductName = g.Key.Name,
                                     CategoryName = g.Key.CategoryName,
                                     UnitPrice = g.Key.Price,
                                     Quantity = g.Sum(x => x.ai.Quantity)
                                 };

            // Query để lấy số lượng đã bán (tất cả thời gian)
            var soldQuery = from oi in _context.OrderItems
                            join o in _context.Orders on oi.OrderId equals o.Id
                            where o.Status == OrderStatus.Paid && !o.IsDeleted
                            group oi by oi.ProductId into g
                            select new
                            {
                                ProductId = g.Key,
                                SoldQuantity = g.Sum(x => x.Quantity)
                            };

            var inventories = await inventoryQuery.AsNoTracking().ToListAsync();
            var soldProducts = await soldQuery.AsNoTracking().ToListAsync();

            // Kết hợp dữ liệu và tính toán trạng thái
            var allResults = inventories.Select(inv =>
            {
                var soldQty = soldProducts.FirstOrDefault(sp => sp.ProductId == inv.ProductId)?.SoldQuantity ?? 0;
                var quantity = inv.Quantity;
                var inventoryValue = quantity * inv.UnitPrice;

                // Xác định trạng thái dựa trên số lượng tồn kho
                string status;
                if (quantity == 0)
                {
                    status = "OutOfStock"; // Hết hàng
                }
                else if (quantity <= 10) // Có thể điều chỉnh ngưỡng này
                {
                    status = "LowStock"; // Sắp hết
                }
                else
                {
                    status = "InStock"; // Còn hàng
                }

                return new InventoryReportDto
                {
                    ProductName = inv.ProductName,
                    CategoryName = inv.CategoryName,
                    Quantity = quantity,
                    SoldQuantity = soldQty,
                    UnitPrice = inv.UnitPrice,
                    InventoryValue = inventoryValue,
                    Status = status
                };
            })
            .OrderBy(x => x.CategoryName)
            .ThenBy(x => x.ProductName)
            .ToList();

            var total = allResults.Count;
            var data = allResults.Skip(skip).Take(take).ToList();

            return new { data, total };
        }
    }
}