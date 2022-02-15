using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using MQTT_SERVER.Data;
using MQTT_SERVER.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_SERVER.Repositories.Implementation
{
        public class IoTRepositoryBase<TEntity> : IIoTRepository<TEntity> where TEntity : class
        {
            protected readonly IMongoDatabase Database;
        protected readonly IMongoCollection<TEntity> _context;

        protected IoTRepositoryBase(IIMongoContext context)
        {
            Database = context.Database;
            var type = typeof(TEntity);
            _context = Database.GetCollection<TEntity>(type.Name.ToLower());
        }

        public virtual async Task<TEntity> Add(TEntity obj)
        {
            await _context.InsertOneAsync(obj);
            return obj;
        }

        public virtual async Task<TEntity> GetById(string id)
        {
            var data = await _context.Find(FilterId(id)).SingleOrDefaultAsync();
            return data;
        }

        public virtual async Task<IEnumerable<TEntity>> GetAll()
        {
            var all = await _context.FindAsync(Builders<TEntity>.Filter.Empty);
            return all.ToList();
        }

        public async virtual Task<TEntity> Update(string id, TEntity obj)
        {
            await _context.ReplaceOneAsync(FilterId(id), obj);
            return obj;
        }

        public async virtual Task<bool> Remove(string id)
        {
            var result = await _context.DeleteOneAsync(FilterId(id));
            return result.IsAcknowledged;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        private static FilterDefinition<TEntity> FilterId(string key)
        {
            return Builders<TEntity>.Filter.Eq("Id", key);
        }
        public IEnumerable<TEntity> FilterBy(
            Expression<Func<TEntity, bool>> filterExpression)
        {
            var filter = Builders<TEntity>.Filter.Where(filterExpression);

            return _context.Find(filter).ToEnumerable();
        }
        public IEnumerable<TProjected> FilterBy<TProjected>(Expression<Func<TEntity, bool>> filterExpression, Expression<Func<TEntity, TProjected>> projectionExpression)
        {
            return _context.Find(filterExpression).Project(projectionExpression).ToEnumerable();
        }

        public TEntity FindById(string id)
        {
            var filter = Builders<TEntity>.Filter.Eq("Id", id);
            return _context.Find(filter).SingleOrDefault();
        }

        public TEntity FindOne(Expression<Func<TEntity, bool>> filterExpression)
        {
            return _context.Find(filterExpression).FirstOrDefault();
        }

        public virtual Task<TEntity> FindOneAsync(Expression<Func<TEntity, bool>> filterExpression)
        {
            return Task.Run(() => _context.Find(filterExpression).FirstOrDefaultAsync());
        }

        public IQueryable<TEntity> FindAll(Expression<Func<TEntity, bool>> filterExpression)
        {
            return _context.AsQueryable<TEntity>().Where(filterExpression);
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> filterExpression)
        {
            return await _context.AsQueryable().Where(filterExpression).ToListAsync();
        }
    }
    
}
