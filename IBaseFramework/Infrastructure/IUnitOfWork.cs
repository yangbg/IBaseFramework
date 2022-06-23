using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IBaseFramework.Base;

namespace IBaseFramework.Infrastructure
{
    public interface IUnitOfWork
    {
        IRepository<TEntity> GetRepository<TEntity>() where TEntity : EntityBaseEmpty;
        void Transaction(Action action);

        int Execute(string sql, Parameters parameters = null);
        TEntity Get<TEntity>(string sql, Parameters parameters = null);
        IEnumerable<TEntity> GetList<TEntity>(string sql, Parameters parameters = null);
        PagedList<T> GetByPage<T>(string sql, Parameters param = null, int pageIndex = 1, int pageSize = 10);
        PagedList<dynamic> GetByPage(string sql, Parameters param = null, int pageIndex = 1, int pageSize = 10);

        Task<int> ExecuteAsync(string sql, Parameters parameters = null);
        Task<TEntity> GetAsync<TEntity>(string sql, Parameters parameters = null);
        Task<IEnumerable<TEntity>> GetListAsync<TEntity>(string sql, Parameters parameters = null);
        Task<PagedList<T>> GetByPageAsync<T>(string sql, Parameters param = null, int pageIndex = 1, int pageSize = 10);
        Task<PagedList<dynamic>> GetByPageAsync(string sql, Parameters param = null, int pageIndex = 1, int pageSize = 10);
    }
}