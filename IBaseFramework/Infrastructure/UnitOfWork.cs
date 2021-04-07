using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using IBaseFramework.Base;
using IBaseFramework.DapperExtension;
using IBaseFramework.Ioc;

namespace IBaseFramework.Infrastructure
{
    public partial class UnitOfWork : IUnitOfWork
    {
        private readonly ConcurrentDictionary<Type, dynamic> _repositoryCache = new ConcurrentDictionary<Type, dynamic>();
        public IRepository<TEntity> GetRepository<TEntity>() where TEntity : EntityBase
        {
            var type = typeof(TEntity);
            if (_repositoryCache.ContainsKey(type))
            {
                return (IRepository<TEntity>)_repositoryCache[type];
            }

            var repository = IocManager.Resolve<IRepository<TEntity>>();
            if (repository != null)
            {
                _repositoryCache.TryAdd(type, repository);
            }
            return repository;
        }

        public void Transaction(Action<IDbTransaction> action)
        {
            ExecuteHelper.Transaction(action);
        }

        #region Execute Sql
        public int SqlExecute(string sql, Parameters parameters = null, IDbTransaction transaction = null)
        {
            return ExecuteHelper.Execute(connection => connection.Execute(sql, parameters, transaction));
        }

        public TEntity QueryFirstOrDefault<TEntity>(string sql, Parameters parameters = null, IDbTransaction transaction = null)
        {
            return ExecuteHelper.Execute(connection => connection.QueryFirstOrDefault<TEntity>(sql, parameters, transaction));
        }

        public IEnumerable<TEntity> SqlQuery<TEntity>(string sql, Parameters parameters = null, IDbTransaction transaction = null)
        {
            return ExecuteHelper.Execute(connection => connection.Query<TEntity>(sql, parameters, transaction));
        }

        public PagedList<T> Page<T>(string sql, Parameters param = null, int pageIndex = 1, int pageSize = 20)
        {
            return ExecuteHelper.Execute(connection =>
           {
               var dic = new Dictionary<string, object>();
               if (param != null)
               {
                   foreach (var paramParameterName in param.ParameterNames)
                   {
                       dic.Add(paramParameterName, param.Get<object>(paramParameterName));
                   }
               }

               var result = PageHelper.Page(sql, dic, pageIndex, pageSize);

               var total = connection.QueryFirstOrDefault<int>(result.Item1.GetSql(), result.Item1.Param);
               var data = connection.Query<T>(result.Item2.GetSql(), result.Item2.Param);

               return new PagedList<T>()
               {
                   PageIndex = result.Item3,
                   PageSize = result.Item4,
                   TotalCount = total,
                   Items = data
               };
           });
        }

        public PagedList<dynamic> Page(string sql, Parameters param = null, int pageIndex = 1, int pageSize = 20)
        {
            return Page<dynamic>(sql, param, pageIndex, pageSize);
        }

        #endregion

        #region Execute Sql Async

        public async Task<int> SqlExecuteAsync(string sql, Parameters parameters = null, IDbTransaction transaction = null)
        {
            return await ExecuteHelper.Execute(async connection => await connection.ExecuteAsync(sql, parameters, transaction).ConfigureAwait(false));
        }

        public async Task<TEntity> QueryFirstOrDefaultAsync<TEntity>(string sql, Parameters parameters = null, IDbTransaction transaction = null)
        {
            return await ExecuteHelper.Execute(async connection => await connection.QueryFirstOrDefaultAsync<TEntity>(sql, parameters, transaction).ConfigureAwait(false));
        }

        public async Task<IEnumerable<TEntity>> SqlQueryAsync<TEntity>(string sql, Parameters parameters = null, IDbTransaction transaction = null)
        {
            return await ExecuteHelper.Execute(async connection => await connection.QueryAsync<TEntity>(sql, parameters, transaction).ConfigureAwait(false));
        }

        public async Task<PagedList<T>> PageAsync<T>(string sql, Parameters param = null, int pageIndex = 1, int pageSize = 10)
        {
            return await ExecuteHelper.Execute(async connection =>
           {
               var dic = new Dictionary<string, object>();
               if (param != null)
               {
                   foreach (var paramParameterName in param.ParameterNames)
                   {
                       dic.Add(paramParameterName, param.Get<object>(paramParameterName));
                   }
               }

               var result = PageHelper.Page(sql, dic, pageIndex, pageSize);

               var total = await connection.QueryFirstOrDefaultAsync<int>(result.Item1.GetSql(), result.Item1.Param).ConfigureAwait(false);
               var data = await connection.QueryAsync<T>(result.Item2.GetSql(), result.Item2.Param).ConfigureAwait(false);

               return new PagedList<T>()
               {
                   PageIndex = result.Item3,
                   PageSize = result.Item4,
                   TotalCount = total,
                   Items = data
               };
           });
        }

        public async Task<PagedList<dynamic>> PageAsync(string sql, Parameters param = null, int pageIndex = 1, int pageSize = 10)
        {
            return await PageAsync<dynamic>(sql, param, pageIndex, pageSize);
        }

        #endregion        
    }
}
