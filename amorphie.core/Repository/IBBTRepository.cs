using amorphie.core.Base;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace amorphie.core.Repository
{
    public interface IBBTRepository<TModel, TContext>
        where TModel : EntityBase
        where TContext : DbContext
    {
        ValueTask<TModel?> GetById(Guid id);
        Task<TModel?> FirstOrDefault(Expression<Func<TModel, bool>> predicate);
        Task<int> Insert(TModel entity);
        Task<int> Update(TModel entity);
        Task<int> Delete(TModel entity);
        IQueryable<TModel> GetAll();
        IQueryable<TModel> GetAll(int page, int pageSize);
        IQueryable<TModel> GetWhere(Expression<Func<TModel, bool>> predicate);
        IQueryable<TModel> GetWhere(Expression<Func<TModel, bool>> predicate, int page, int pageSize);
        Task<int> SaveChangesAsync();
    }
}