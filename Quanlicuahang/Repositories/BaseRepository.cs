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
        return await _context.SaveChangesAsync();
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
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActiveAsync(string id)
    {
        var entity = await _dbSet.FirstOrDefaultAsync(e => e.Id == id);
        if (entity == null) return false;

        entity.IsDeleted = false;
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
