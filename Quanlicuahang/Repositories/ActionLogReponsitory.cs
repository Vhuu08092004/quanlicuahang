using Microsoft.EntityFrameworkCore;
using Quanlicuahang.Data;
using Quanlicuahang.Models;

namespace Quanlicuahang.Repositories
{
    public interface IActionLogRepository
    {
        Task<IEnumerable<ActionLog>> GetAllAsync();
        Task AddAsync(ActionLog log);
        Task SaveChangesAsync();
    }

    public class ActionLogRepository : IActionLogRepository
    {
        private readonly ApplicationDbContext _context;

        public ActionLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ActionLog>> GetAllAsync()
        {
            return await _context.ActionLogs
                .Include(x => x.User)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(ActionLog log)
        {
            await _context.ActionLogs.AddAsync(log);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
