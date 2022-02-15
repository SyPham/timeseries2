using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_SERVER.Repositories.Interface
{
    public interface IIoTRepository<TEntity> : IDisposable where TEntity : class
    {
        Task<TEntity> Add(TEntity obj);
        Task<TEntity> GetById(string id);
        Task<IEnumerable<TEntity>> GetAll();
        Task<TEntity> Update(string id, TEntity obj);
        Task<bool> Remove(string id);
        IEnumerable<TProjected> FilterBy<TProjected>(
        Expression<Func<TEntity, bool>> filterExpression,
        Expression<Func<TEntity, TProjected>> projectionExpression);
        IEnumerable<TEntity> FilterBy(
        Expression<Func<TEntity, bool>> filterExpression);
        IQueryable<TEntity> FindAll(
        Expression<Func<TEntity, bool>> filterExpression);
        Task<IEnumerable<TEntity>> FindAllAsync(
        Expression<Func<TEntity, bool>> filterExpression);
        TEntity FindById(string id);
        TEntity FindOne(Expression<Func<TEntity, bool>> filterExpression);
        Task<TEntity> FindOneAsync(Expression<Func<TEntity, bool>> filterExpression);
    }
}
