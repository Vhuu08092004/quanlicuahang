using Quanlicuahang.Repositories;
using Quanlicuahang.DTOs.Report;

namespace Quanlicuahang.Services
{
    public interface IReportService
    {
        Task<object> GetRevenueByDayAsync(DateTime date, int skip, int take);
        Task<object> GetRevenueByMonthAsync(int year, int month, int skip, int take);
        Task<object> GetRevenueByYearAsync(int year, int skip, int take);
        Task<object> GetRevenueByEmployeeAsync(DateTime? fromDate, DateTime? toDate, int skip, int take);
        Task<object> GetRevenueByCustomerAsync(DateTime? fromDate, DateTime? toDate, string? region, int skip, int take);
        Task<object> GetRevenueByCustomerGroupAsync(DateTime? fromDate, DateTime? toDate, int skip, int take);
        Task<object> GetTopSellingProductsAsync(int topN, DateTime? fromDate, DateTime? toDate, int skip, int take);
        Task<object> GetInventoryReportAsync(int skip, int take);
    }

    public class ReportService : IReportService
    {
        private readonly IReportRepository _repository;

        public ReportService(IReportRepository repository)
        {
            _repository = repository;
        }

        /** Báo cáo doanh thu theo ngày */
        public async Task<object> GetRevenueByDayAsync(DateTime date, int skip, int take)
        {
            return await _repository.GetRevenueByDayAsync(date, skip, take);
        }

        /** Báo cáo doanh thu theo tháng */
        public async Task<object> GetRevenueByMonthAsync(int year, int month, int skip, int take)
        {
            return await _repository.GetRevenueByMonthAsync(year, month, skip, take);
        }

        /** Báo cáo doanh thu theo năm */
        public async Task<object> GetRevenueByYearAsync(int year, int skip, int take)
        {
            return await _repository.GetRevenueByYearAsync(year, skip, take);
        }

        /** Báo cáo doanh thu theo nhân viên */
        public async Task<object> GetRevenueByEmployeeAsync(DateTime? fromDate, DateTime? toDate, int skip, int take)
        {
            return await _repository.GetRevenueByEmployeeAsync(fromDate, toDate, skip, take);
        }

        /** Báo cáo doanh thu theo khách hàng */
        public async Task<object> GetRevenueByCustomerAsync(DateTime? fromDate, DateTime? toDate, string? region, int skip, int take)
        {
            return await _repository.GetRevenueByCustomerAsync(fromDate, toDate, region, skip, take);
        }

        /** Báo cáo doanh thu theo nhóm khách hàng */
        public async Task<object> GetRevenueByCustomerGroupAsync(DateTime? fromDate, DateTime? toDate, int skip, int take)
        {
            // Đây là method tổng hợp, gọi GetRevenueByCustomerAsync với các region khác nhau
            // Hoặc có thể implement riêng nếu cần
            throw new NotImplementedException("Method này đã được thay thế bởi GetRevenueByCustomerAsync với tham số region");
        }

        /** Báo cáo sản phẩm bán chạy */
        public async Task<object> GetTopSellingProductsAsync(int topN, DateTime? fromDate, DateTime? toDate, int skip, int take)
        {
            return await _repository.GetTopSellingProductsAsync(topN, fromDate, toDate, skip, take);
        }

        /** Báo cáo sản phẩm tồn kho */
        public async Task<object> GetInventoryReportAsync(int skip, int take)
        {
            return await _repository.GetInventoryReportAsync(skip, take);
        }
    }
}