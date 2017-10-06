using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace SS.Models.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly DbContext dbCtx;

        public Repository(DbContext dbCtx)
        {
            this.dbCtx = dbCtx;
        }

        public void Add(TEntity entity)
        {
            dbCtx.Set<TEntity>().Add(entity);
        }

        public void AddRange(IEnumerable<TEntity> entities)
        {
            dbCtx.Set<TEntity>().AddRange(entities);
        }

        public IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
        {
            return dbCtx.Set<TEntity>().Where(predicate);
        }

        public TEntity Get(Guid id)
        {
            return dbCtx.Set<TEntity>().Find(id);
        }

        public IEnumerable<TEntity> GetAll()
        {
            return dbCtx.Set<TEntity>().ToList();
        }

        public void Remove(TEntity entity)
        {
            dbCtx.Set<TEntity>().Remove(entity);
        }

        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            dbCtx.Set<TEntity>().RemoveRange(entities);
        }
    }
}