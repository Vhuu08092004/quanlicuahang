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
        /// <param name="input">Input với Date</param>
        /// <returns>List of RevenueReportDto</returns>
        [HttpPost("revenue/day")]
        public async Task<ActionResult<List<RevenueReportDto>>> GetRevenueByDayAsync([FromBody] DayRevenueInputDto input)
        {
            var result = await _service.GetRevenueByDayAsync(input.Date);
            return Ok(result);
        }

        /// <summary>
        /// Báo cáo doanh thu theo tháng
        /// </summary>
        /// <param name="input">Input với Year và Month</param>
        /// <returns>List of RevenueReportDto</returns>
        [HttpPost("revenue/month")]
        public async Task<ActionResult<List<RevenueReportDto>>> GetRevenueByMonthAsync([FromBody] MonthRevenueInputDto input)
        {
            if (input.Month < 1 || input.Month > 12)
            {
                return BadRequest("Invalid month. Must be between 1 and 12.");
            }
            var result = await _service.GetRevenueByMonthAsync(input.Year, input.Month);
            return Ok(result);
        }

        /// <summary>
        /// Báo cáo doanh thu theo năm
        /// </summary>
        /// <param name="input">Input với Year</param>
        /// <returns>List of RevenueReportDto</returns>
        [HttpPost("revenue/year")]
        public async Task<ActionResult<List<RevenueReportDto>>> GetRevenueByYearAsync([FromBody] YearRevenueInputDto input)
        {
            var result = await _service.GetRevenueByYearAsync(input.Year);
            return Ok(result);
        }

        /// <summary>
        /// Báo cáo doanh thu theo nhân viên
        /// </summary>
        /// <param name="filter">Filter với FromDate và ToDate</param>
        /// <returns>List of RevenueByEmployeeDto</returns>
        [HttpPost("revenue/employee")]
        public async Task<ActionResult<List<RevenueByEmployeeDto>>> GetRevenueByEmployeeAsync([FromBody] RevenueFilterDto filter)
        {
            var result = await _service.GetRevenueByEmployeeAsync(filter.FromDate, filter.ToDate);
            return Ok(result);
        }

        /// <summary>
        /// Báo cáo doanh thu theo khách hàng
        /// </summary>
        /// <param name="filter">Filter với FromDate và ToDate</param>
        /// <returns>List of RevenueByCustomerDto</returns>
        [HttpPost("revenue/customer")]
        public async Task<ActionResult<List<RevenueByCustomerDto>>> GetRevenueByCustomerAsync([FromBody] RevenueFilterDto filter)
        {
            var result = await _service.GetRevenueByCustomerAsync(filter.FromDate, filter.ToDate);
            return Ok(result);
        }

        /// <summary>
        /// Báo cáo doanh thu theo nhóm khách hàng
        /// </summary>
        /// <param name="filter">Filter với FromDate và ToDate</param>
        /// <returns>List of RevenueByCustomerGroupDto</returns>
        [HttpPost("revenue/customergroup")]
        public async Task<ActionResult<List<RevenueByCustomerGroupDto>>> GetRevenueByCustomerGroupAsync([FromBody] RevenueFilterDto filter)
        {
            var result = await _service.GetRevenueByCustomerGroupAsync(filter.FromDate, filter.ToDate);
            return Ok(result);
        }

        /// <summary>
        /// Báo cáo sản phẩm bán chạy
        /// </summary>
        /// <param name="filter">Filter với TopN, FromDate và ToDate</param>
        /// <returns>List of TopSellingProductDto</returns>
        [HttpPost("topproducts")]
        public async Task<ActionResult<List<TopSellingProductDto>>> GetTopSellingProductsAsync([FromBody] TopProductsFilterDto filter)
        {
            if (filter.TopN <= 0)
            {
                return BadRequest("TopN must be greater than 0.");
            }
            var result = await _service.GetTopSellingProductsAsync(filter.TopN, filter.FromDate, filter.ToDate);
            return Ok(result);
        }

        /// <summary>
        /// Báo cáo sản phẩm tồn kho
        /// </summary>
        /// <returns>List of InventoryReportDto</returns>
        [HttpPost("inventory")]
        public async Task<ActionResult<List<InventoryReportDto>>> GetInventoryReportAsync()
        {
            var result = await _service.GetInventoryReportAsync();
            return Ok(result);
        }
    }
}