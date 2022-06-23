using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using IBaseFramework.Base;
using IBaseFramework.ExpressionVisit;
using IBaseFramework.Ioc;

namespace IBaseFramework.Infrastructure
{
    public partial class UnitOfWork : IUnitOfWork
    {
        private readonly ConcurrentDictionary<Type, IRepository> _repositoryCache = new ConcurrentDictionary<Type, IRepository>();
        public IRepository<TEntity> GetRepository<TEntity>() where TEntity : EntityBaseEmpty
        {
            var type = typeof(TEntity);
            if (_repositoryCache.ContainsKey(type))
            {
                var queryRepository = _repositoryCache[type];
                queryRepository.QueryBuilder.InitQuery();

                return (IRepository<TEntity>)queryRepository;
            }

            var repository = IocManager.Resolve<IRepository<TEntity>>();
            if (repository != null)
            {
                _repositoryCache.TryAdd(type, repository);
            }
            return repository;
        }

        public void Transaction(Action action)
        {
            ExecuteHelper.Transaction(action);
        }

        #region Execute Sql
        public int Execute(string sql, Parameters parameters = null)
        {
            return ExecuteHelper.Execute(connection =>
            {
                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(sql, parameters, connection);

                var result = connection.Execute(sql, parameters);

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return result;
            });
        }

        public TEntity Get<TEntity>(string sql, Parameters parameters = null)
        {
            return ExecuteHelper.Execute(connection =>
            {
                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(sql, parameters, connection);

                var result = connection.QueryFirstOrDefault<TEntity>(sql, parameters);

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return result;
            });
        }

        public IEnumerable<TEntity> GetList<TEntity>(string sql, Parameters parameters = null)
        {
            return ExecuteHelper.Execute(connection =>
            {
                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(sql, parameters, connection);

                var result = connection.Query<TEntity>(sql, parameters);

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return result;
            });
        }

        public PagedList<T> GetByPage<T>(string sql, Parameters param = null, int pageIndex = 1, int pageSize = 20)
        {
            return ExecuteHelper.Execute(connection =>
            {
                var sqlQuery = new SqlQuery();
                sqlQuery.SqlBuilder.Append(sql);

                var queryResultForTotal = sqlQuery.ParseTotalCount();
                var queryResult = sqlQuery.ParseLimit(new ExpressionParser(DapperContext.DatabaseConfiguration.SqlProvider), pageIndex, pageSize);

                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResultForTotal, connection);
                diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResult, connection);

                var total = connection.QueryFirstOrDefault<int>(queryResultForTotal.GetSql(), param);
                var items = connection.Query<T>(queryResult.GetSql(), param);

                var result = new PagedList<T>() { Items = items, TotalCount = total, PageIndex = pageIndex, PageSize = pageSize };

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return result;
            });
        }

        public PagedList<dynamic> GetByPage(string sql, Parameters param = null, int pageIndex = 1, int pageSize = 20)
        {
            return GetByPage<dynamic>(sql, param, pageIndex, pageSize);
        }

        #endregion

        #region Execute Sql Async

        public Task<int> ExecuteAsync(string sql, Parameters parameters = null)
        {
            return ExecuteHelper.Execute(connection =>
            {
                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(sql, parameters, connection);

                var result = connection.ExecuteAsync(sql, parameters);

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return result;
            });
        }

        public Task<TEntity> GetAsync<TEntity>(string sql, Parameters parameters = null)
        {
            return ExecuteHelper.Execute(connection =>
            {
                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(sql, parameters, connection);

                var result = connection.QueryFirstOrDefaultAsync<TEntity>(sql, parameters);

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return result;
            });
        }

        public Task<IEnumerable<TEntity>> GetListAsync<TEntity>(string sql, Parameters parameters = null)
        {
            return ExecuteHelper.Execute(connection =>
            {
                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(sql, parameters, connection);

                var result = connection.QueryAsync<TEntity>(sql, parameters);

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return result;
            });
        }

        public Task<PagedList<T>> GetByPageAsync<T>(string sql, Parameters param = null, int pageIndex = 1, int pageSize = 10)
        {
            return ExecuteHelper.Execute(async connection =>
            {
                var sqlQuery = new SqlQuery();
                sqlQuery.SqlBuilder.Append(sql);

                var queryResultForTotal = sqlQuery.ParseTotalCount();
                var queryResult = sqlQuery.ParseLimit(new ExpressionParser(DapperContext.DatabaseConfiguration.SqlProvider), pageIndex, pageSize);

                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResultForTotal, connection);
                diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResult, connection);

                var total = await connection.QueryFirstOrDefaultAsync<int>(queryResultForTotal.GetSql(), param);
                var items = await connection.QueryAsync<T>(queryResult.GetSql(), param);

                var result = new PagedList<T>() { Items = items, TotalCount = total, PageIndex = pageIndex, PageSize = pageSize };

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return result;
            });
        }

        public Task<PagedList<dynamic>> GetByPageAsync(string sql, Parameters param = null, int pageIndex = 1, int pageSize = 10)
        {
            return GetByPageAsync<dynamic>(sql, param, pageIndex, pageSize);
        }

        #endregion
    }
}
