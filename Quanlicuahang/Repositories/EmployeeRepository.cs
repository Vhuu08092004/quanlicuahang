using Quanlicuahang.Data;
using Quanlicuahang.Models;

namespace Quanlicuahang.Repositories
{
    public interface IEmployeeRepository : IBaseRepository<Employee> { }

    public class EmployeeRepository : BaseRepository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(ApplicationDbContext context) : base(context) { }
    }
}
