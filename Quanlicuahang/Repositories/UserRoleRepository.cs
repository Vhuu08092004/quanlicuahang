using Microsoft.EntityFrameworkCore;
using Quanlicuahang.Data;
using Quanlicuahang.Models;

namespace Quanlicuahang.Repositories
{
    public interface IUserRoleRepository : IBaseRepository<UserRole>
    {
        Task AddUserRoleAsync(string userId, string roleId);
        Task RemoveAllRolesByUserIdAsync(string userId);
    }

    public class UserRoleRepository : BaseRepository<UserRole>, IUserRoleRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRoleRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task AddUserRoleAsync(string userId, string roleId)
        {
            var entity = new UserRole { UserId = userId, RoleId = roleId };
            await _context.UserRoles.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAllRolesByUserIdAsync(string userId)
        {
            var roles = await _context.UserRoles.Where(ur => ur.UserId == userId).ToListAsync();
            if (roles.Any())
            {
                _context.UserRoles.RemoveRange(roles);
                await _context.SaveChangesAsync();
            }
        }
    }
}
