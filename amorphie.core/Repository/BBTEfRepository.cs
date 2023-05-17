using amorphie.core.Base;
using Dapr.Client;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace amorphie.core.Repository
{
    public class BBTRepository <TModel,TContext> : IBBTRepository<TModel, TContext> 
        where TModel : EntityBase
        where TContext : DbContext
    {
        internal TContext Context;
        internal DbSet<TModel> dbSet;

        public BBTRepository(TContext context)
        {
            Context = context;
            dbSet = Context.Set<TModel>();
        }
        public ValueTask<TModel?> GetById(Guid id,CancellationToken cancellationToken) => dbSet.FindAsync(id, cancellationToken);
        public ValueTask<TModel?> GetById(Guid id) => dbSet.FindAsync(id);
        public Task<TModel?> FirstOrDefault(Expression<Func<TModel, bool>> predicate)
            => dbSet.FirstOrDefaultAsync(predicate);
        public async Task<int> Insert(TModel entity)
        {
            await dbSet.AddAsync(entity);
            return await SaveChangesAsync();
        }
        public async Task<int> Update(TModel entity)
        {
            Context.Entry(entity).State = EntityState.Modified;
            return await SaveChangesAsync();
        }
        public async Task<int> Delete(TModel entity)
        {
            dbSet.Remove(entity);
            return await SaveChangesAsync();
        }
        public async Task<int> Delete(Guid id)
        {
            TModel? model = await GetById(id);
            return await Delete(model);
        }
        public IQueryable<TModel> GetAll()
        {
            return dbSet.AsNoTracking();
        }
        public IQueryable<TModel> GetAll(int page,int pageSize)
        {
            return GetAll().Skip(page * pageSize).Take(pageSize);
        }
        public IQueryable<TModel> GetWhere(Expression<Func<TModel, bool>> predicate)
        {
            return dbSet.AsNoTracking().Where(predicate);
        }
        public IQueryable<TModel> GetWhere(Expression<Func<TModel, bool>> predicate, int page, int pageSize)
        {
            return GetWhere(predicate).Skip(page * pageSize).Take(pageSize);
        }
        public async Task<int> SaveChangesAsync()
        {
            return await Context.SaveChangesAsync();
        }
    }
}