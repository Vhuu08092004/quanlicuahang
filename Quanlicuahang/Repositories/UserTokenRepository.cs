using Microsoft.EntityFrameworkCore;
using Quanlicuahang.Data;
using Quanlicuahang.Models;

namespace Quanlicuahang.Repositories
{
    public interface IUserTokenRepository
    {
        Task<UserToken?> GetTokenAsync(string token);
        Task<IEnumerable<UserToken>> GetTokensByUserIdAsync(string userId);
        Task AddTokenAsync(UserToken userToken);
        Task UpdateTokenAsync(UserToken userToken);
        Task RemoveTokenAsync(UserToken userToken);
        Task RemoveUserTokensAsync(string userId);
        Task<string?> GetUserIdByTokenAsync(string token);
    }

    public class UserTokenRepository : IUserTokenRepository
    {
        private readonly ApplicationDbContext _context;

        public UserTokenRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserToken?> GetTokenAsync(string token)
        {
            return await _context.UserTokens
                .FirstOrDefaultAsync(t => t.Token == token && !t.IsRevoked);
        }

        public async Task<IEnumerable<UserToken>> GetTokensByUserIdAsync(string userId)
        {
            return await _context.UserTokens
                .Where(t => t.UserId == userId && !t.IsRevoked)
                .ToListAsync();
        }

        public async Task AddTokenAsync(UserToken userToken)
        {
            _context.UserTokens.Add(userToken);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTokenAsync(UserToken userToken)
        {
            _context.UserTokens.Update(userToken);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveTokenAsync(UserToken userToken)
        {
            _context.UserTokens.Remove(userToken);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveUserTokensAsync(string userId)
        {
            var tokens = await _context.UserTokens
                .Where(t => t.UserId == userId)
                .ToListAsync();
            _context.UserTokens.RemoveRange(tokens);
            await _context.SaveChangesAsync();
        }

        public async Task<string?> GetUserIdByTokenAsync(string token)
        {
            var userId = await _context.UserTokens
                .Where(t => t.Token == token && !t.IsRevoked && t.Expiration > DateTime.UtcNow)
                .Select(t => t.UserId)
                .FirstOrDefaultAsync();

            return userId;
        }
    }
}