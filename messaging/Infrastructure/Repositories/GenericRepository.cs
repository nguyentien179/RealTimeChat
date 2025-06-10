using System;
using System.Linq.Expressions;
using messaging.Application.Interfaces;
using messaging.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

namespace messaging.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T>
    where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public async Task<PagedResult<T>> GetAllAsync(
        int pageIndex,
        int pageSize,
        List<Expression<Func<T, bool>>>? filters = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object?>>? include = null
    )
    {
        var query = _dbSet.AsQueryable();

        if (filters != null)
        {
            foreach (var filter in filters)
            {
                query = query.Where(filter);
            }
        }

        if (include != null)
        {
            query = include(query);
        }

        query =
            orderBy != null
                ? orderBy(query)
                : (IOrderedQueryable<T>)query.OrderBy(x => EF.Property<object>(x, "Id"));

        var totalRecords = await query.CountAsync();
        if (pageSize <= 0)
        {
            pageSize = 5; // default page size
        }

        if (pageIndex <= 0 || pageIndex == null)
        {
            pageIndex = 1;
        }

        var items = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<T>(items, pageIndex, pageSize, totalRecords);
    }

    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> expression)
    {
        return await _dbSet.AnyAsync(expression);
    }

    public async Task<T?> GetByIdAsync(
        Guid id,
        Func<IQueryable<T>, IIncludableQueryable<T, object?>>? include = null
    )
    {
        var query = _dbSet.AsQueryable();
        if (include != null)
        {
            query = include(query);
        }
        return await query.FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id);
    }

    public async Task<T?> GetAsync(
        Expression<Func<T, bool>> expression,
        Func<IQueryable<T>, IIncludableQueryable<T, object?>>? include = null
    )
    {
        var query = _dbSet.AsQueryable();
        if (include != null)
        {
            query = include(query);
        }
        return await query.FirstOrDefaultAsync(expression);
    }

    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
    public async Task<int> CountAsync(List<Expression<Func<T, bool>>> filters)
    {
        IQueryable<T> query = _dbSet;

        if (filters != null)
        {
            foreach (var filter in filters)
            {
                query = query.Where(filter);
            }
        }

        return await query.CountAsync();
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await _context.Database.BeginTransactionAsync();
    }
}
