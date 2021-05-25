using System;
using System.Threading.Tasks;

namespace Netfox.Core.Database
{
    public interface IRepository<T>
    {
        T Single(Guid key);
        T SingleOrDefault(Guid key);
        bool Exists(Guid key);
        Task<T> InsertAsync(T entity);
        T Insert(T entity);
        Task UpdateAsync(T entity);
        void Update(T entity);
        void Delete(T entity);

        //IEnumerable<T> List { get; }
        T FindById(Guid Id);
    }
}