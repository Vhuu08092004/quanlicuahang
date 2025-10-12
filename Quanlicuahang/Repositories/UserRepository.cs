using Microsoft.EntityFrameworkCore;
using Quanlicuahang.Data;
using Quanlicuahang.Models;

namespace Quanlicuahang.Repositories
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User?> GetByUsernameAsync(string username);
        Task AddUserWithDefaultRoleAsync(User user, string defaultRoleName = "Staff");
        Task<User?> GetByIdWithRolesAsync(string id);  
    }

    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _dbSet
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == username && !u.IsDeleted);
        }

        public async Task<User?> GetByIdWithRolesAsync(string id)
        {
            return await _dbSet
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        }


        public async Task AddUserWithDefaultRoleAsync(User user, string defaultRoleName = "Staff")
        {
            await _dbSet.AddAsync(user);
            await _context.SaveChangesAsync();

            if (!user.UserRoles.Any())
            {
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == defaultRoleName);
                if (role != null)
                {
                    var userRole = new UserRole
                    {
                        UserId = user.Id,
                        RoleId = role.Id
                    };
                    await _context.UserRoles.AddAsync(userRole);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}
