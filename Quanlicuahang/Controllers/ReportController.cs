using Microsoft.AspNetCore.Mvc;
using Quanlicuahang.DTOs.Report;
using Quanlicuahang.Repositories;
using Quanlicuahang.Services;

namespace Quanlicuahang.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _service;

        public ReportController(IReportService service)
        {
            _service = service;
        }

        /// <summary>
        /// Báo cáo doanh thu theo ngày
        /// </summary>
        /// <param name="input">Input với Date, Skip, Take</param>
        /// <returns>Paginated list of RevenueReportDto</returns>
        [HttpPost("revenue/day")]
        public async Task<ActionResult<object>> GetRevenueByDayAsync([FromBody] DayRevenueInputDto input)
        {
            var result = await _service.GetRevenueByDayAsync(input.Date, input.Skip, input.Take);
            return Ok(result);
        }

        /// <summary>
        /// Báo cáo doanh thu theo tháng
        /// </summary>
        /// <param name="input">Input với Year, Month, Skip, Take</param>
        /// <returns>Paginated list of RevenueReportDto</returns>
        [HttpPost("revenue/month")]
        public async Task<ActionResult<object>> GetRevenueByMonthAsync([FromBody] MonthRevenueInputDto input)
        {
            if (input.Month < 1 || input.Month > 12)
            {
                return BadRequest("Invalid month. Must be between 1 and 12.");
            }
            var result = await _service.GetRevenueByMonthAsync(input.Year, input.Month, input.Skip, input.Take);
            return Ok(result);
        }

        /// <summary>
        /// Báo cáo doanh thu theo năm
        /// </summary>
        /// <param name="input">Input với Year, Skip, Take</param>
        /// <returns>Paginated list of RevenueReportDto</returns>
        [HttpPost("revenue/year")]
        public async Task<ActionResult<object>> GetRevenueByYearAsync([FromBody] YearRevenueInputDto input)
        {
            var result = await _service.GetRevenueByYearAsync(input.Year, input.Skip, input.Take);
            return Ok(result);
        }

        /// <summary>
        /// Báo cáo doanh thu theo nhân viên
        /// </summary>
        /// <param name="filter">Filter với FromDate, ToDate, Skip, Take</param>
        /// <returns>Paginated list of RevenueByEmployeeDto</returns>
        [HttpPost("revenue/employee")]
        public async Task<ActionResult<object>> GetRevenueByEmployeeAsync([FromBody] RevenueFilterDto filter)
        {
            var result = await _service.GetRevenueByEmployeeAsync(filter.FromDate, filter.ToDate, filter.Skip, filter.Take);
            return Ok(result);
        }

        /// <summary>
        /// Báo cáo doanh thu theo khách hàng
        /// </summary>
        /// <param name="filter">Filter với FromDate, ToDate, Skip, Take</param>
        /// <returns>Paginated list of RevenueByCustomerDto</returns>
        [HttpPost("revenue/customer")]
        public async Task<ActionResult<object>> GetRevenueByCustomerAsync([FromBody] RevenueFilterDto filter)
        {
            var result = await _service.GetRevenueByCustomerAsync(filter.FromDate, filter.ToDate, filter.Skip, filter.Take);
            return Ok(result);
        }

        /// <summary>
        /// Báo cáo doanh thu theo nhóm khách hàng
        /// </summary>
        /// <param name="filter">Filter với FromDate, ToDate, Skip, Take</param>
        /// <returns>Paginated list of RevenueByCustomerGroupDto</returns>
        [HttpPost("revenue/customergroup")]
        public async Task<ActionResult<object>> GetRevenueByCustomerGroupAsync([FromBody] RevenueFilterDto filter)
        {
            var result = await _service.GetRevenueByCustomerGroupAsync(filter.FromDate, filter.ToDate, filter.Skip, filter.Take);
            return Ok(result);
        }

        /// <summary>
        /// Báo cáo sản phẩm bán chạy
        /// </summary>
        /// <param name="filter">Filter với TopN, FromDate, ToDate, Skip, Take</param>
        /// <returns>Paginated list of TopSellingProductDto</returns>
        [HttpPost("topproducts")]
        public async Task<ActionResult<object>> GetTopSellingProductsAsync([FromBody] TopProductsFilterDto filter)
        {
            if (filter.TopN <= 0)
            {
                return BadRequest("TopN must be greater than 0.");
            }
            var result = await _service.GetTopSellingProductsAsync(filter.TopN, filter.FromDate, filter.ToDate, filter.Skip, filter.Take);
            return Ok(result);
        }

        /// <summary>
        /// Báo cáo sản phẩm tồn kho
        /// </summary>
        /// <param name="filter">Filter với Skip, Take</param>
        /// <returns>Paginated list of InventoryReportDto</returns>
        [HttpPost("inventory")]
        public async Task<ActionResult<object>> GetInventoryReportAsync([FromBody] InventoryFilterDto filter)
        {
            var result = await _service.GetInventoryReportAsync(filter.Skip, filter.Take);
            return Ok(result);
        }
    }
}