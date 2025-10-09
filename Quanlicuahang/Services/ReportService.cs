using Microsoft.EntityFrameworkCore;
using Quanlicuahang.DTOs.Report;
using Quanlicuahang.Models;
using Quanlicuahang.Repositories;

namespace Quanlicuahang.Services
{
    public interface IReportService
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

    public class ReportService : IReportService
    {
        private readonly IReportRepository _repository;

        public ReportService(IReportRepository repository)
        {
            _repository = repository;
        }

        /** Báo cáo doanh thu theo ngày */
        public async Task<List<RevenueReportDto>> GetRevenueByDayAsync(DateTime date)
        {
            return await _repository.GetRevenueByDayAsync(date);
        }

        /** Báo cáo doanh thu theo tháng */
        public async Task<List<RevenueReportDto>> GetRevenueByMonthAsync(int year, int month)
        {
            return await _repository.GetRevenueByMonthAsync(year, month);
        }

        /** Báo cáo doanh thu theo năm */
        public async Task<List<RevenueReportDto>> GetRevenueByYearAsync(int year)
        {
            return await _repository.GetRevenueByYearAsync(year);
        }

        /** Báo cáo doanh thu theo nhân viên */
        public async Task<List<RevenueByEmployeeDto>> GetRevenueByEmployeeAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            return await _repository.GetRevenueByEmployeeAsync(fromDate, toDate);
        }

        /** Báo cáo doanh thu theo khách hàng */
        public async Task<List<RevenueByCustomerDto>> GetRevenueByCustomerAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            return await _repository.GetRevenueByCustomerAsync(fromDate, toDate);
        }

        /** Báo cáo doanh thu theo nhóm khách hàng */
        public async Task<List<RevenueByCustomerGroupDto>> GetRevenueByCustomerGroupAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            return await _repository.GetRevenueByCustomerGroupAsync(fromDate, toDate);
        }

        /** Báo cáo sản phẩm bán chạy */
        public async Task<List<TopSellingProductDto>> GetTopSellingProductsAsync(int topN = 10, DateTime? fromDate = null, DateTime? toDate = null)
        {
            return await _repository.GetTopSellingProductsAsync(topN, fromDate, toDate);
        }

        /** Báo cáo sản phẩm tồn kho */
        public async Task<List<InventoryReportDto>> GetInventoryReportAsync()
        {
            return await _repository.GetInventoryReportAsync();
        }
    }
}