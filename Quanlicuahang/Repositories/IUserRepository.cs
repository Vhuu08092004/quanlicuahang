
using Quanlicuahang.Models;

namespace Quanlicuahang.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> getAllAsync();
        Task<User?> getByIdAsync(int id);
        Task<User?> AddAsync(User user);
        Task<User?> Update(User user);
        void Delete(User user);
    }
}
