using Microsoft.EntityFrameworkCore;
using Quanlicuahang.Data;
using Quanlicuahang.Enum;
using Quanlicuahang.Models;

namespace Quanlicuahang.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<User?> AddAsync(User user)
        {
            if (user.Role == null || !user.Role.Any())
            {
                user.Role = new List<Role> { Role.Staff };
            }
            var entityEntry = await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return entityEntry.Entity;
        }

        public void Delete(User user)
        {
            _context.Users.Remove(user);
            _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<User>> getAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User?> getByIdAsync(int id)
        {
            return await _context.Users.FirstAsync(u => u.Id == id);
        }

        public async Task<User?> Update(User user)
        {
           var entityEntry = _context.Users.Update(user);
           await _context.SaveChangesAsync();
           return entityEntry.Entity;
        }
    }
}
