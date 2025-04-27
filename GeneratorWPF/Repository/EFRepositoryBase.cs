using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using GeneratorWPF.Context;

namespace GeneratorWPF.Repository
{
    public class EFRepositoryBase<TEntity> where TEntity : class
    {

        public virtual TEntity Get(Expression<Func<TEntity, bool>> filter, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null)
        { 
            using var _context = new LocalContext();
            IQueryable<TEntity> queryable = _context.Set<TEntity>();
            if (include != null) queryable = include(queryable);
            return queryable.FirstOrDefault(filter)!;
        }

        public virtual TEntity Add(TEntity entity)
        {
            using var _context = new LocalContext();
            _context.Entry(entity).State = EntityState.Added;
            _context.SaveChanges();
            return entity;
        }

        public virtual void Delete(TEntity entity)
        {
            using var _context = new LocalContext();
            _context.Entry(entity).State = EntityState.Deleted;
            _context.SaveChanges();
        }

        public virtual TEntity Update(TEntity entity)
        {
            using var _context = new LocalContext();
            _context.Entry(entity).State = EntityState.Modified;
            _context.SaveChanges();
            return entity;
        }

        public virtual bool IsExist(Expression<Func<TEntity, bool>> filter)
        {
            using var _context = new LocalContext();
            return _context.Set<TEntity>().Any(filter);
        }

        public virtual List<TEntity> GetAll(
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
            bool enableTracking = true)
        {
            using var _context = new LocalContext();
            IQueryable<TEntity> queryable = _context.Set<TEntity>();
            if (!enableTracking) queryable = queryable.AsNoTracking();
            if (include != null) queryable = include(queryable);
            if (filter != null) queryable = queryable.Where(filter);
            if (orderBy != null)
                return orderBy(queryable).ToList();
            return queryable.ToList();
        }

        public virtual void DeleteByFilter(Expression<Func<TEntity, bool>> filter)
        {
            using var _context = new LocalContext();
            var entity = _context.Set<TEntity>().Where(filter).FirstOrDefault();
            if (entity == null) throw new InvalidOperationException("The specified entity to delete could not be found.");
            _context.Entry(entity).State = EntityState.Deleted;
            _context.SaveChanges();
        }
    }
}
