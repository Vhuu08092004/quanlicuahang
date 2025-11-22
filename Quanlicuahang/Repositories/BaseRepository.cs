using Microsoft.EntityFrameworkCore;
using Quanlicuahang.Data;
using Quanlicuahang.DTOs;
using Quanlicuahang.Models;
using System.Linq.Expressions;


public interface IBaseRepository<T> where T : BasePrimary
{
    IQueryable<T> GetAll(bool includeDeleted = false);
    Task<T?> GetByIdAsync(string id);
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);

    Task<List<SelectBoxDto>> GetSelectBoxAsync(Expression<Func<T, string>> nameSelector,
                                               Expression<Func<T, string>> codeSelector,
                                               bool isDeleted = false);

    Task<bool> DeActiveAsync(string id);
    Task<bool> ActiveAsync(string id);

    Task<int> SaveChangesAsync();
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, string? excludeId = null);
}

public class BaseRepository<T> : IBaseRepository<T> where T : BasePrimary
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public BaseRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public virtual IQueryable<T> GetAll(bool includeDeleted = false)
    {
        if (includeDeleted)
            return _dbSet.AsQueryable();

        return _dbSet.Where(e => !e.IsDeleted);
    }

    public virtual async Task<T?> GetByIdAsync(string id)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Id == id);
    }

    public virtual async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public virtual void Delete(T entity)
    {
        entity.IsDeleted = true;
        _dbSet.Update(entity);
    }

    public virtual async Task<int> SaveChangesAsync()
    {
        try
        {
            return await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            // Chuyển đổi thông báo lỗi Entity Framework sang tiếng Việt
            var errorMessage = ex.InnerException?.Message ?? ex.Message;

            if (errorMessage.Contains("An error occurred while saving the entity changes"))
            {
                throw new System.Exception("Đã xảy ra lỗi khi lưu dữ liệu. Vui lòng kiểm tra lại thông tin và thử lại.", ex);
            }

            // Xử lý các lỗi phổ biến khác
            if (errorMessage.Contains("Cannot insert duplicate key"))
            {
                throw new System.Exception("Dữ liệu đã tồn tại trong hệ thống. Vui lòng kiểm tra lại.", ex);
            }

            if (errorMessage.Contains("Foreign key constraint"))
            {
                throw new System.Exception("Không thể xóa dữ liệu này vì đang được sử dụng ở nơi khác.", ex);
            }

            if (errorMessage.Contains("Cannot delete or update"))
            {
                throw new System.Exception("Không thể xóa hoặc cập nhật dữ liệu này vì đang được sử dụng.", ex);
            }

            // Nếu không phải các lỗi đã xử lý, throw lại với thông báo gốc nhưng có thêm context
            throw new System.Exception($"Lỗi khi lưu dữ liệu: {errorMessage}", ex);
        }
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, string? excludeId = null)
    {
        var query = _dbSet.AsQueryable();

        query = query.Where(predicate);
        if (!string.IsNullOrEmpty(excludeId))
        {
            query = query.Where(e => e.Id != excludeId);
        }
        query = query.Where(e => !e.IsDeleted);

        return await query.AnyAsync();
    }

    public async Task<List<SelectBoxDto>> GetSelectBoxAsync(Expression<Func<T, string>> nameSelector,
                                                            Expression<Func<T, string>> codeSelector,
                                                            bool isDeleted = false)
    {
        var query = _dbSet.AsQueryable();
        query = query.Where(e => e.IsDeleted == isDeleted);

        var list = await query
            .OrderBy(nameSelector)
            .Select(e => new SelectBoxDto
            {
                Id = e.Id,
                Name = EF.Property<string>(e, ((MemberExpression)nameSelector.Body).Member.Name),
                Code = EF.Property<string>(e, ((MemberExpression)codeSelector.Body).Member.Name)
            })
            .ToListAsync();

        return list;
    }


    public async Task<bool> DeActiveAsync(string id)
    {
        var entity = await _dbSet.FirstOrDefaultAsync(e => e.Id == id);
        if (entity == null) return false;

        entity.IsDeleted = true;
        _dbSet.Update(entity);
        await SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActiveAsync(string id)
    {
        var entity = await _dbSet.FirstOrDefaultAsync(e => e.Id == id);
        if (entity == null) return false;

        entity.IsDeleted = false;
        _dbSet.Update(entity);
        await SaveChangesAsync();
        return true;
    }
}
