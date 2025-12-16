using Microsoft.EntityFrameworkCore;
using Quanlicuahang.DTOs.Invoice;
using Quanlicuahang.Helpers;
using Quanlicuahang.Models;
using Quanlicuahang.Repositories;

namespace Quanlicuahang.Services
{
    public interface IInvoiceSettingService
    {
        Task<InvoiceSettingDto> GetAsync();
        Task<InvoiceSettingDto> UpsertAsync(InvoiceSettingUpsertDto dto);
    }

    public class InvoiceSettingService : IInvoiceSettingService
    {
        private readonly IActionLogService _logService;
        private readonly IHttpContextAccessor _httpContext;
        private readonly ITokenHelper _tokenHelper;
        private readonly IInvoiceSettingRepository _repo;

        public InvoiceSettingService(
            IActionLogService logService,
            IHttpContextAccessor httpContext,
            ITokenHelper tokenHelper,
            IInvoiceSettingRepository repo
        )
        {
            _logService = logService;
            _httpContext = httpContext;
            _tokenHelper = tokenHelper;
            _repo = repo;
        }

        public async Task<InvoiceSettingDto> GetAsync()
        {
            var entity = await _repo.GetAll(false)
                .OrderBy(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (entity == null)
            {
                return new InvoiceSettingDto
                {
                    Id = string.Empty,
                    StoreName = string.Empty,
                    StoreAddress = string.Empty,
                    Phone = string.Empty,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
            }

            return Map(entity);
        }

        public async Task<InvoiceSettingDto> UpsertAsync(InvoiceSettingUpsertDto dto)
        {
            var userId = await _tokenHelper.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Vui lòng đăng nhập lại!");

            var entity = await _repo.GetAll(true)
                .OrderBy(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            object? oldValue = entity == null ? null : new
            {
                entity.Id,
                entity.StoreName,
                entity.StoreAddress,
                entity.Phone,
                entity.IsDeleted
            };

            if (entity == null)
            {
                entity = new InvoiceSetting
                {
                    Id = Guid.NewGuid().ToString(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = userId,
                    UpdatedBy = userId,
                    IsDeleted = false
                };
                Apply(entity, dto);
                await _repo.AddAsync(entity);
            }
            else
            {
                entity.IsDeleted = false;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = userId;
                Apply(entity, dto);
                _repo.Update(entity);
            }

            await _repo.SaveChangesAsync();

            var newValue = new
            {
                entity.Id,
                entity.StoreName,
                entity.StoreAddress,
                entity.Phone,
                entity.IsDeleted
            };

            await _logService.LogAsync(Guid.NewGuid().ToString(), "Upsert", "InvoiceSetting", entity.Id,
                "Cập nhật cấu hình hóa đơn", oldValue, newValue, userId,
                _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString());

            return Map(entity);
        }

        private static void Apply(InvoiceSetting entity, InvoiceSettingUpsertDto dto)
        {
            entity.StoreName = dto.StoreName ?? string.Empty;
            entity.StoreAddress = dto.StoreAddress ?? string.Empty;
            entity.Phone = dto.Phone ?? string.Empty;
        }

        private static InvoiceSettingDto Map(InvoiceSetting entity)
        {
            return new InvoiceSettingDto
            {
                Id = entity.Id,
                StoreName = entity.StoreName ?? string.Empty,
                StoreAddress = entity.StoreAddress ?? string.Empty,
                Phone = entity.Phone ?? string.Empty,
                IsDeleted = entity.IsDeleted,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }
    }
}

