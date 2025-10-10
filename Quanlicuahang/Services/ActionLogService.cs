using Microsoft.EntityFrameworkCore;
using Quanlicuahang.Data;
using Quanlicuahang.DTOs.ActionLog;
using Quanlicuahang.Models;
using System.Text.Json;

namespace Quanlicuahang.Services
{
    public interface IActionLogService
    {
        Task<object> GetLogsAsync(ActionLogSearchDto searchDto);

        Task LogAsync(
            string code,
            string action,
            string entityType,
            string? entityId,
            string? description,
            object? oldValue,
            object? newValue,
            string? userId,
            string? ip,
            string? userAgent);
    }

    public class ActionLogService : IActionLogService
    {
        private readonly ApplicationDbContext _context;

        public ActionLogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(
            string code,
            string action,
            string entityType,
            string? entityId,
            string? description,
            object? oldValue,
            object? newValue,
            string? userId,
            string? ip,
            string? userAgent)
        {
            
                var log = new ActionLog
                {
                    Id = Guid.NewGuid().ToString(),
                    Code = code,
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    Description = description,
                    OldValues = oldValue != null ? JsonSerializer.Serialize(oldValue) : null,
                    NewValues = newValue != null ? JsonSerializer.Serialize(newValue) : null,
                    UserId = userId,
                    IpAddress = ip,
                    UserAgent = userAgent,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ActionLogs.Add(log);
                await _context.SaveChangesAsync();
        }

        public async Task<object> GetLogsAsync(ActionLogSearchDto searchDto)
        {
            var query = _context.ActionLogs
                .Include(l => l.User)
                .AsQueryable();

            if (searchDto.Where != null)
            {
                var w = searchDto.Where;

                if (!string.IsNullOrWhiteSpace(w.FunctionType))
                    query = query.Where(x => x.EntityType == w.FunctionType);

                if (!string.IsNullOrWhiteSpace(w.FunctionId))
                    query = query.Where(x => x.EntityId == w.FunctionId);

                if (!string.IsNullOrWhiteSpace(w.Type))
                    query = query.Where(x => x.Action == w.Type);

                if (!string.IsNullOrWhiteSpace(w.CreatedBy))
                    query = query.Where(x => x.UserId == w.CreatedBy);
            }

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip(searchDto.Skip)
                .Take(searchDto.Take)
                .Select(x => new
                {
                    x.Id,
                    x.Action,
                    x.EntityType,
                    x.EntityId,
                    x.Description,
                    x.OldValues,
                    x.NewValues,
                    x.CreatedAt,
                    CreatedByName = x.User != null ? x.User.FullName : null,
                    x.IpAddress,
                    x.UserAgent
                })
                .ToListAsync();

            return new { data, total };
        }
    }
}
