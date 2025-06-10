using System;
using System.Linq.Expressions;
using messaging.Domain.Models;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

namespace messaging.Application.Interfaces;

public interface IGenericRepository<T>
    where T : class
{
    public Task<T> AddAsync(T entity);

    public Task<bool> ExistsAsync(Expression<Func<T, bool>> expression);

    public Task<T?> GetByIdAsync(
        Guid id,
        Func<IQueryable<T>, IIncludableQueryable<T, object?>>? include = null
    );

    public Task<T?> GetAsync(
        Expression<Func<T, bool>> expression,
        Func<IQueryable<T>, IIncludableQueryable<T, object?>>? include = null
    );

    Task<PagedResult<T>> GetAllAsync(
        int pageNumber,
        int pageSize,
        List<Expression<Func<T, bool>>>? filters = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object?>>? include = null
    );

    public void Update(T entity);
    public void Delete(T entity);
    public Task SaveChangesAsync();
    public Task<int> CountAsync(List<Expression<Func<T, bool>>> filters);

    public Task<IDbContextTransaction> BeginTransactionAsync();
}
