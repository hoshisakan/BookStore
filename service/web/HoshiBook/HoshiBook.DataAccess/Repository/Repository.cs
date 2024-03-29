using HoshiBook.DataAccess.Repository.IRepository;

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using EFCore.BulkExtensions;

namespace HoshiBook.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _db;
        internal DbSet<T> dbSet;

        public Repository(ApplicationDbContext db)
        {
            _db = db;
            this.dbSet = _db.Set<T>();
        }

        public void Add(T entity)
        {
            dbSet.Add(entity);
        }

        public void AddRange(List<T> entity)
        {
            dbSet.AddRange(entity);
        }

        public void BulkAdd(List<T> entity)
        {
            _db.BulkInsert(entity);
        }

        //includeProp - "Category,CoverType
        public List<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperties = null)
        {
            IQueryable<T> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (includeProperties != null)
            {
                foreach(var includeProp in includeProperties.Split(
                    new char[] {','}, StringSplitOptions.RemoveEmptyEntries)
                )
                {
                    query = query.Include(includeProp);
                }
            }
            return query.ToList();
        }

        public T GetFirstOrDefault(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = true)
        {
            IQueryable<T> query;
            
            if (tracked)
            {
                query = dbSet;
            }
            else
            {
                query = dbSet.AsNoTracking();
            }

            query = query.Where(filter);
            if (includeProperties != null)
            {
                foreach(var includeProp in includeProperties.Split(
                    new char[] {','}, StringSplitOptions.RemoveEmptyEntries)
                )
                {
                    query = query.Include(includeProp);
                }
            }
            return query.FirstOrDefault();
        }

        public void Remove(T entity)
        {
            dbSet.Remove(entity);
        }

        public void RemoveRange(List<T> entity)
        {
            dbSet.RemoveRange(entity);
        }
    }
}