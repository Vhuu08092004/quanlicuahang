using Microsoft.EntityFrameworkCore;
using Quanlicuahang.Data;
using Quanlicuahang.DTOs.Report;

namespace Quanlicuahang.Repositories
{
    // Interface cho ReportRepository
    public interface IReportRepository
    {
        Task<List<RevenueReportDto>> GetRevenueByDayAsync(DateTime date);
        Task<List<RevenueReportDto>> GetRevenueByMonthAsync(int year, int month);
        Task<List<RevenueReportDto>> GetRevenueByYearAsync(int year);
        Task<List<RevenueByEmployeeDto>> GetRevenueByEmployeeAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<RevenueByCustomerDto>> GetRevenueByCustomerAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<RevenueByCustomerGroupDto>> GetRevenueByCustomerGroupAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<TopSellingProductDto>> GetTopSellingProductsAsync(int topN = 10, DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<InventoryReportDto>> GetInventoryReportAsync();
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
        public async Task<List<RevenueReportDto>> GetRevenueByDayAsync(DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

            // Get payments grouped by date
            var paymentsQuery = from o in _context.Orders
                                join p in _context.Payments on o.Id equals p.OrderId
                                where o.OrderDate >= startOfDay && o.OrderDate <= endOfDay
                                      && o.Status == "Completed"
                                      && o.IsDeleted == false
                                group new { o, p } by o.OrderDate.Date into g
                                select new
                                {
                                    Date = g.Key,
                                    TotalPayment = g.Sum(x => x.p.Amount),
                                    OrderCount = g.Select(x => x.o.Id).Distinct().Count()
                                };

            // Get returns grouped by date
            var returnsQuery = from o in _context.Orders
                               join r in _context.Returns on o.Id equals r.OrderId
                               where o.OrderDate >= startOfDay && o.OrderDate <= endOfDay
                                     && o.Status == "Completed"
                                     && o.IsDeleted == false
                               group r by o.OrderDate.Date into g
                               select new
                               {
                                   Date = g.Key,
                                   TotalRefund = g.Sum(x => x.RefundAmount)
                               };

            var payments = await paymentsQuery.AsNoTracking().ToListAsync();
            var returns = await returnsQuery.AsNoTracking().ToListAsync();

            // Combine the results
            var result = payments.Select(p => new RevenueReportDto
            {
                Date = p.Date,
                TotalRevenue = p.TotalPayment - (returns.FirstOrDefault(r => r.Date == p.Date)?.TotalRefund ?? 0),
                OrderCount = p.OrderCount
            }).ToList();

            return result;
        }

        // Báo cáo doanh thu theo tháng
        public async Task<List<RevenueReportDto>> GetRevenueByMonthAsync(int year, int month)
        {
            var startOfMonth = new DateTime(year, month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1).Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            // Get payments grouped by day
            var paymentsQuery = from o in _context.Orders
                                join p in _context.Payments on o.Id equals p.OrderId
                                where o.OrderDate >= startOfMonth && o.OrderDate <= endOfMonth
                                      && o.Status == "Completed"
                                      && o.IsDeleted == false
                                group new { o, p } by new { o.OrderDate.Year, o.OrderDate.Month, o.OrderDate.Day } into g
                                select new
                                {
                                    Year = g.Key.Year,
                                    Month = g.Key.Month,
                                    Day = g.Key.Day,
                                    TotalPayment = g.Sum(x => x.p.Amount),
                                    OrderCount = g.Select(x => x.o.Id).Distinct().Count()
                                };

            // Get returns grouped by day
            var returnsQuery = from o in _context.Orders
                               join r in _context.Returns on o.Id equals r.OrderId
                               where o.OrderDate >= startOfMonth && o.OrderDate <= endOfMonth
                                     && o.Status == "Completed"
                                     && o.IsDeleted == false
                               group r by new { o.OrderDate.Year, o.OrderDate.Month, o.OrderDate.Day } into g
                               select new
                               {
                                   Year = g.Key.Year,
                                   Month = g.Key.Month,
                                   Day = g.Key.Day,
                                   TotalRefund = g.Sum(x => x.RefundAmount)
                               };

            var payments = await paymentsQuery.AsNoTracking().ToListAsync();
            var returns = await returnsQuery.AsNoTracking().ToListAsync();

            // Combine the results
            var result = payments.Select(p => new RevenueReportDto
            {
                Date = new DateTime(p.Year, p.Month, p.Day),
                TotalRevenue = p.TotalPayment - (returns.FirstOrDefault(r => r.Year == p.Year && r.Month == p.Month && r.Day == p.Day)?.TotalRefund ?? 0),
                OrderCount = p.OrderCount
            }).OrderBy(x => x.Date).ToList();

            return result;
        }

        // Báo cáo doanh thu theo năm (theo tháng trong năm)
        public async Task<List<RevenueReportDto>> GetRevenueByYearAsync(int year)
        {
            var startOfYear = new DateTime(year, 1, 1);
            var endOfYear = new DateTime(year, 12, 31, 23, 59, 59);

            // Get payments grouped by month
            var paymentsQuery = from o in _context.Orders
                                join p in _context.Payments on o.Id equals p.OrderId
                                where o.OrderDate >= startOfYear && o.OrderDate <= endOfYear
                                      && o.Status == "Completed"
                                      && o.IsDeleted == false
                                group new { o, p } by new { o.OrderDate.Year, o.OrderDate.Month } into g
                                select new
                                {
                                    Year = g.Key.Year,
                                    Month = g.Key.Month,
                                    TotalPayment = g.Sum(x => x.p.Amount),
                                    OrderCount = g.Select(x => x.o.Id).Distinct().Count()
                                };

            // Get returns grouped by month
            var returnsQuery = from o in _context.Orders
                               join r in _context.Returns on o.Id equals r.OrderId
                               where o.OrderDate >= startOfYear && o.OrderDate <= endOfYear
                                     && o.Status == "Completed"
                                     && o.IsDeleted == false
                               group r by new { o.OrderDate.Year, o.OrderDate.Month } into g
                               select new
                               {
                                   Year = g.Key.Year,
                                   Month = g.Key.Month,
                                   TotalRefund = g.Sum(x => x.RefundAmount)
                               };

            var payments = await paymentsQuery.AsNoTracking().ToListAsync();
            var returns = await returnsQuery.AsNoTracking().ToListAsync();

            // Combine the results
            var result = payments.Select(p => new RevenueReportDto
            {
                Date = new DateTime(p.Year, p.Month, 1), // Ngày đầu tháng
                TotalRevenue = p.TotalPayment - (returns.FirstOrDefault(r => r.Year == p.Year && r.Month == p.Month)?.TotalRefund ?? 0),
                OrderCount = p.OrderCount
            }).OrderBy(x => x.Date).ToList();

            return result;
        }

        public async Task<List<RevenueByEmployeeDto>> GetRevenueByEmployeeAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            Console.WriteLine("⏰ Thời gian backend nhận request: " + fromDate);
            Console.WriteLine("⏰ Thời gian trong payload: " + toDate);
            // Lấy tất cả nhân viên và tính tổng thanh toán (kể cả không có đơn)
            var paymentsQuery =
                from e in _context.Employees

                join u in _context.Users on e.Id equals u.EmployeeId into userGroup
                from u in userGroup.DefaultIfEmpty()

                join o in _context.Orders on u.Id equals o.UserId into orderGroup
                from o in orderGroup.DefaultIfEmpty()

                join p in _context.Payments on o.Id equals p.OrderId into paymentGroup
                from p in paymentGroup.DefaultIfEmpty()

                group new { o, p } by new { e.Id, e.FullName } into g
                select new
                {
                    EmployeeId = g.Key.Id,
                    EmployeeName = g.Key.FullName,

                    TotalPayment = g
                        .Where(x =>
                            x.o != null &&
                            (fromDate == null || x.o.OrderDate >= fromDate) &&
                            (toDate == null || x.o.OrderDate <= toDate) &&
                            x.o.Status == "Completed" &&
                            !x.o.IsDeleted &&
                            x.p != null
                        )
                        .Sum(x => (decimal?)x.p.Amount) ?? 0,

                    TotalOrderAmount = g
                        .Where(x =>
                            x.o != null &&
                            (fromDate == null || x.o.OrderDate >= fromDate) &&
                            (toDate == null || x.o.OrderDate <= toDate) &&
                            x.o.Status == "Completed" &&
                            !x.o.IsDeleted
                        )
                        .Sum(x => (decimal?)((x.o.TotalAmount) - (x.o.DiscountAmount))) ?? 0,

                    OrderCount = g
                        .Where(x =>
                            x.o != null &&
                            (fromDate == null || x.o.OrderDate >= fromDate) &&
                            (toDate == null || x.o.OrderDate <= toDate) &&
                            x.o.Status == "Completed" &&
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

                group new { o, r } by new { e.Id, e.FullName } into g
                select new
                {
                    EmployeeId = g.Key.Id,
                    EmployeeName = g.Key.FullName,
                    TotalRefund = g
                        .Where(x =>
                            x.o != null &&
                            (fromDate == null || x.o.OrderDate >= fromDate) &&
                            (toDate == null || x.o.OrderDate <= toDate) &&
                            x.o.Status == "Completed" &&
                            !x.o.IsDeleted &&
                            x.r != null
                        )
                        .Sum(x => (decimal?)x.r.RefundAmount) ?? 0
                };

            var payments = await paymentsQuery.AsNoTracking().ToListAsync();
            var returns = await returnsQuery.AsNoTracking().ToListAsync();

            // Ghép kết quả, đảm bảo mọi nhân viên đều có mặt
            var result = (
                from p in payments
                join r in returns on p.EmployeeId equals r.EmployeeId into returnGroup
                from r in returnGroup.DefaultIfEmpty()
                select new RevenueByEmployeeDto
                {
                    EmployeeName = p.EmployeeName,
                    TotalRevenue = p.TotalPayment - (r?.TotalRefund ?? 0),
                    Commission = p.TotalOrderAmount - p.TotalPayment - (r?.TotalRefund ?? 0),
                    OrderCount = p.OrderCount
                }
            )
            .OrderByDescending(x => x.TotalRevenue)
            .ToList();

            return result;
        }



        // Báo cáo doanh thu theo khách hàng
        public async Task<List<RevenueByCustomerDto>> GetRevenueByCustomerAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            
            // Get payments grouped by customer
            var paymentsQuery = from o in _context.Orders
                                join c in _context.Customers on o.CustomerId equals c.Id
                                join p in _context.Payments on o.Id equals p.OrderId
                                where (fromDate == null || o.OrderDate >= fromDate) && (toDate == null || o.OrderDate <= toDate)
                                      && o.Status == "Completed"
                                      && o.IsDeleted == false
                                group new { o, p } by c.Name into g
                                select new
                                {
                                    CustomerName = g.Key,
                                    TotalPayment = g.Sum(x => x.p.Amount),
                                    OrderCount = g.Select(x => x.o.Id).Distinct().Count()
                                };

            // Get returns grouped by customer
            var returnsQuery = from o in _context.Orders
                               join c in _context.Customers on o.CustomerId equals c.Id
                               join r in _context.Returns on o.Id equals r.OrderId
                               where (fromDate == null || o.OrderDate >= fromDate) && (toDate == null || o.OrderDate <= toDate)
                                     && o.Status == "Completed"
                                     && o.IsDeleted == false
                               group r by c.Name into g
                               select new
                               {
                                   CustomerName = g.Key,
                                   TotalRefund = g.Sum(x => x.RefundAmount)
                               };

            var payments = await paymentsQuery.AsNoTracking().ToListAsync();
            var returns = await returnsQuery.AsNoTracking().ToListAsync();

            // Combine the results
            var result = payments.Select(p => new RevenueByCustomerDto
            {
                CustomerName = p.CustomerName,
                TotalRevenue = p.TotalPayment - (returns.FirstOrDefault(r => r.CustomerName == p.CustomerName)?.TotalRefund ?? 0),
                OrderCount = p.OrderCount
            }).OrderByDescending(x => x.TotalRevenue).ToList();

            return result;
        }

        // Báo cáo doanh thu theo nhóm khách hàng (group dựa trên Address, ví dụ tỉnh/thành)
        public async Task<List<RevenueByCustomerGroupDto>> GetRevenueByCustomerGroupAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            // Get payments grouped by customer group
            var paymentsQuery = from o in _context.Orders
                                join c in _context.Customers on o.CustomerId equals c.Id
                                join p in _context.Payments on o.Id equals p.OrderId
                                where (fromDate == null || o.OrderDate >= fromDate) && (toDate == null || o.OrderDate <= toDate)
                                      && o.Status == "Completed"
                                      && o.IsDeleted == false
                                let groupName = c.Address.Contains("Hà Nội") ? "Hà Nội" :
                                                c.Address.Contains("TP.HCM") ? "TP.HCM" :
                                                c.Address.Contains("Đà Nẵng") ? "Đà Nẵng" : "Khác"
                                group new { p, customerId = c.Id } by groupName into g
                                select new
                                {
                                    GroupName = g.Key,
                                    TotalPayment = g.Sum(x => x.p.Amount),
                                    CustomerCount = g.Select(x => x.customerId).Distinct().Count()
                                };

            // Get returns grouped by customer group
            var returnsQuery = from o in _context.Orders
                               join c in _context.Customers on o.CustomerId equals c.Id
                               join r in _context.Returns on o.Id equals r.OrderId
                               where (fromDate == null || o.OrderDate >= fromDate) && (toDate == null || o.OrderDate <= toDate)
                                     && o.Status == "Completed"
                                     && o.IsDeleted == false
                               let groupName = c.Address.Contains("Hà Nội") ? "Hà Nội" :
                                               c.Address.Contains("TP.HCM") ? "TP.HCM" :
                                               c.Address.Contains("Đà Nẵng") ? "Đà Nẵng" : "Khác"
                               group r by groupName into g
                               select new
                               {
                                   GroupName = g.Key,
                                   TotalRefund = g.Sum(x => x.RefundAmount)
                               };

            var payments = await paymentsQuery.AsNoTracking().ToListAsync();
            var returns = await returnsQuery.AsNoTracking().ToListAsync();

            // Combine the results
            var result = payments.Select(p => new RevenueByCustomerGroupDto
            {
                GroupName = p.GroupName,
                TotalRevenue = p.TotalPayment - (returns.FirstOrDefault(r => r.GroupName == p.GroupName)?.TotalRefund ?? 0),
                CustomerCount = p.CustomerCount
            }).OrderByDescending(x => x.TotalRevenue).ToList();

            return result;
        }

        // Báo cáo sản phẩm bán chạy (top theo quantity hoặc revenue)
        public async Task<List<TopSellingProductDto>> GetTopSellingProductsAsync(int topN = 10, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = from oi in _context.OrderItems
                        join o in _context.Orders on oi.OrderId equals o.Id
                        join prod in _context.Products on oi.ProductId equals prod.Id
                        where (fromDate == null || o.OrderDate >= fromDate) && (toDate == null || o.OrderDate <= toDate)
                              && o.Status == "Completed"
                              && o.IsDeleted == false
                        group oi by prod.Name into g
                        select new TopSellingProductDto
                        {
                            ProductName = g.Key,
                            TotalQuantity = g.Sum(x => x.Quantity),
                            TotalRevenue = g.Sum(x => x.Subtotal)
                        };

            return await query.AsNoTracking()
                             .OrderByDescending(x => x.TotalQuantity)
                             .Take(topN)
                             .ToListAsync();
        }

        // Báo cáo sản phẩm tồn kho
        public async Task<List<InventoryReportDto>> GetInventoryReportAsync()
        {
            var query = from inv in _context.Inventories
                        join prod in _context.Products on inv.ProductId equals prod.Id
                        join cat in _context.Categories on prod.CategoryId equals cat.Id into categories
                        from category in categories.DefaultIfEmpty()
                        where inv.IsDeleted == false && prod.IsDeleted == false
                        select new InventoryReportDto
                        {
                            ProductName = prod.Name,
                            Quantity = inv.Quantity,
                            CategoryName = category != null ? category.Name : "Không phân loại"
                        };

            return await query.AsNoTracking().OrderBy(x => x.CategoryName).ThenBy(x => x.ProductName).ToListAsync();
        }
    }
}