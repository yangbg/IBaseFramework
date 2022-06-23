using Dapper;
using IBaseFramework.Base;
using IBaseFramework.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IBaseFramework.Infrastructure
{
    public partial class QueryRepository<TEntity> : IQueryRepository<TEntity>
    {
        public QueryRepository()
        {
            QueryBuilder = new QueryBuilder(typeof(TEntity));
        }

        public QueryRepository(IQueryBuilder sourceRepository)
        {
            QueryBuilder = new QueryBuilder(sourceRepository);
        }

        public IQueryBuilder QueryBuilder { get; set; }
    }

    public partial class Repository<TEntity> : QueryRepository<TEntity>, IRepository<TEntity> where TEntity : EntityBaseEmpty
    {
        private bool? _hasCommonProperty = null;
        private bool HasCommonProperty
        {
            get
            {
                if (_hasCommonProperty.HasValue)
                    return _hasCommonProperty.Value;

                _hasCommonProperty = QueryBuilder.ExpressionParser.GetAllColumnsList(QueryBuilder.ThisType).Any(u => u.Name.Equals("Version", StringComparison.InvariantCultureIgnoreCase));

                return _hasCommonProperty.Value;
            }
        }

        private Dictionary<string, object> GetUpdateCommonProperty(object userId)
        {
            if (!HasCommonProperty)
                return default;

            var now = DateTime.Now;
            if (userId == null)
            {
                userId = DapperContext.GetUserId();
            }

            return new Dictionary<string, object>()
            {
                {
                    "UpdateUserId",
                    userId
                },
                {
                    "UpdateTime",
                    now
                },
                {
                    "Version",
                    now
                }
            };
        }


        public Expression<Func<TEntity, bool>> ExpressionTrue() => f => true;

        public Expression<Func<TEntity, bool>> ExpressionFalse() => f => false;

        public TEntity Add(TEntity instance)
        {
            return ExecuteHelper.Execute(connection =>
            {
                if (instance == null)
                    return default;

                instance.SetCreateAudit();
                var queryResult = QueryBuilder.ExpressionParser.GetInsert(instance);

                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResult, connection);

                var indetityKey = QueryBuilder.ExpressionParser.GetIdentityKeyColumn(instance.GetType());

                if (indetityKey != null)
                {
                    var fieldId = connection.ExecuteScalar(queryResult.GetSql(), instance);

                    // Set Property Value  
                    indetityKey.SetValue(instance, Convert.ChangeType(fieldId, indetityKey.PropertyType));
                }
                else
                {
                    connection.Execute(queryResult.GetSql(), instance);
                }

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return instance;
            });
        }

        public bool Add(IEnumerable<TEntity> instances)
        {
            return ExecuteHelper.Execute(connection =>
            {
                if (instances == null || !instances.Any())
                    return false;

                var j = 2000;//每 2k 条执行一次
                var result = 0;
                for (var i = 0; i < instances.Count(); i += 2000)
                {
                    var groupList = instances.Take(j).Skip(i).ToList();
                    j += 2000;

                    foreach (var instance in groupList)
                    {
                        instance.SetCreateAudit();
                    }

                    var queryResult = QueryBuilder.ExpressionParser.GetBulkInsert(groupList);

                    var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResult, connection);

                    result += connection.Execute(queryResult.GetSql(), queryResult.Params);

                    Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);
                }

                return result == instances.Count();
            });
        }

        public Task<TEntity> AddAsync(TEntity instance)
        {
            return ExecuteHelper.Execute(async connection =>
            {
                if (instance == null)
                    return default;

                instance.SetCreateAudit();

                var queryResult = QueryBuilder.ExpressionParser.GetInsert(instance);

                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResult, connection);

                var indetityKey = QueryBuilder.ExpressionParser.GetIdentityKeyColumn(instance.GetType());

                if (indetityKey != null)
                {
                    var fieldId = await connection.ExecuteScalarAsync(queryResult.GetSql(), instance).ConfigureAwait(false);
                    // Set Property Value  
                    indetityKey.SetValue(instance, Convert.ChangeType(fieldId, indetityKey.PropertyType));
                }

                await connection.ExecuteAsync(queryResult.GetSql(), instance).ConfigureAwait(false);

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return instance;
            });
        }

        public Task<bool> AddAsync(IEnumerable<TEntity> instances)
        {
            return ExecuteHelper.Execute(async connection =>
              {
                  if (instances == null || !instances.Any())
                      return false;

                  var j = 2000;//每 2k 条执行一次
                  var result = 0;
                  for (var i = 0; i < instances.Count(); i += 2000)
                  {
                      var groupList = instances.Take(j).Skip(i).ToList();
                      j += 2000;

                      foreach (var instance in groupList)
                      {
                          instance.SetCreateAudit();
                      }

                      var queryResult = QueryBuilder.ExpressionParser.GetBulkInsert(groupList);

                      var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResult, connection);

                      result += await connection.ExecuteAsync(queryResult.GetSql(), queryResult.Params);

                      Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);
                  }

                  return result == instances.Count();
              });
        }

        public bool Delete(Expression<Func<TEntity, bool>> predicate, bool isLogicDelete = true, object updateUserId = default)
        {
            return ExecuteHelper.Execute(connection =>
            {
                var queryResult = QueryBuilder.ExpressionParser.GetDelete(predicate, isLogicDelete, isLogicDelete ? GetUpdateCommonProperty(updateUserId) : default);

                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResult, connection);

                var result = connection.Execute(queryResult.GetSql(), queryResult.Params) > 0;

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return result;
            });
        }

        public bool Delete(TEntity instance, bool isLogicDelete = true)
        {
            return ExecuteHelper.Execute(connection =>
            {
                var queryResult = QueryBuilder.ExpressionParser.GetDelete(instance, isLogicDelete, isLogicDelete ? GetUpdateCommonProperty(null) : default);

                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResult, connection);

                var result = connection.Execute(queryResult.GetSql(), queryResult.Params) > 0;

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return result;
            });
        }

        public Task<bool> DeleteAsync(TEntity instance, bool isLogicDelete = true)
        {
            return ExecuteHelper.Execute(async connection =>
            {
                var queryResult = QueryBuilder.ExpressionParser.GetDelete(instance, isLogicDelete, isLogicDelete ? GetUpdateCommonProperty(null) : default);

                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResult, connection);

                var result = await connection.ExecuteAsync(queryResult.GetSql(), queryResult.Params) > 0;

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return result;
            });
        }

        public Task<bool> DeleteAsync(Expression<Func<TEntity, bool>> predicate, bool isLogicDelete = true, object updateUserId = default)
        {
            return ExecuteHelper.Execute(async connection =>
            {
                var queryResult = QueryBuilder.ExpressionParser.GetDelete(predicate, isLogicDelete, isLogicDelete ? GetUpdateCommonProperty(updateUserId) : default);

                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResult, connection);

                var result = await connection.ExecuteAsync(queryResult.GetSql(), queryResult.Params) > 0;

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return result;
            });
        }

        public bool Exist(Expression<Func<TEntity, bool>> predicate)
        {
            return ExecuteHelper.Execute(connection =>
            {
                var queryResult = QueryBuilder.ExpressionParser.GetExist(predicate);

                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResult, connection);

                var result = connection.QueryFirstOrDefault<int>(queryResult.GetSql(), queryResult.Params) > 0;

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return result;
            });
        }

        public Task<bool> ExistAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return ExecuteHelper.Execute(async connection =>
            {
                var queryResult = QueryBuilder.ExpressionParser.GetExist(predicate);

                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResult, connection);

                var result = await connection.QueryFirstOrDefaultAsync<int>(queryResult.GetSql(), queryResult.Params) > 0;

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return result;
            });
        }

        public TEntity Get(Expression<Func<TEntity, bool>> predicate)
        {
            return ExecuteHelper.Execute(connection =>
            {
                var queryResult = QueryBuilder.ExpressionParser.GetSelect(predicate, true);

                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResult, connection);

                var result = connection.QueryFirstOrDefault<TEntity>(queryResult.GetSql(), queryResult.Params);

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return result;
            });
        }

        public TEntity Get<TKey>(TKey id)
        {
            return ExecuteHelper.Execute(connection =>
            {
                var queryResult = QueryBuilder.ExpressionParser.GetSelectById<TEntity, TKey>(id);

                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResult, connection);

                var result = connection.QuerySingleOrDefault<TEntity>(queryResult.GetSql(), queryResult.Params);

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return result;
            });
        }

        public Task<TEntity> GetAsync<TKey>(TKey id)
        {
            return ExecuteHelper.Execute(connection =>
            {
                var queryResult = QueryBuilder.ExpressionParser.GetSelectById<TEntity, TKey>(id);

                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResult, connection);

                var result = connection.QuerySingleOrDefaultAsync<TEntity>(queryResult.GetSql(), queryResult.Params);

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return result;
            });
        }

        public Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return ExecuteHelper.Execute(connection =>
            {
                var queryResult = QueryBuilder.ExpressionParser.GetSelect(predicate, true);

                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResult, connection);

                var result = connection.QueryFirstOrDefaultAsync<TEntity>(queryResult.GetSql(), queryResult.Params);

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return result;
            });
        }

        public IEnumerable<TEntity> GetList(Expression<Func<TEntity, bool>> predicate)
        {
            return ExecuteHelper.Execute(connection =>
            {
                var queryResult = QueryBuilder.ExpressionParser.GetSelect(predicate, false);

                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResult, connection);

                var result = connection.Query<TEntity>(queryResult.GetSql(), queryResult.Params);

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return result;
            });
        }

        public Task<IEnumerable<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return ExecuteHelper.Execute(connection =>
            {
                var queryResult = QueryBuilder.ExpressionParser.GetSelect(predicate, false);

                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResult, connection);

                var result = connection.QueryAsync<TEntity>(queryResult.GetSql(), queryResult.Params);

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return result;
            });
        }

        public bool Update(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TEntity>> updateValues, object updateUserId = default)
        {
            return ExecuteHelper.Execute(connection =>
            {
                var queryResult = QueryBuilder.ExpressionParser.GetUpdate(updateValues, predicate, GetUpdateCommonProperty(updateUserId));

                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResult, connection);

                var result = connection.Execute(queryResult.GetSql(), queryResult.Params) > 0;

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return result;
            });
        }

        public bool Update(TEntity instance)
        {
            return ExecuteHelper.Execute(connection =>
            {
                instance.SetUpdateAudit();

                var queryResult = QueryBuilder.ExpressionParser.GetUpdate(instance);

                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResult, connection);

                var result = connection.Execute(queryResult.GetSql(), instance) > 0;

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return result;
            });
        }

        public bool Update(TEntity instance, Expression<Func<TEntity, bool>> predicate)
        {
            return ExecuteHelper.Execute(connection =>
            {
                var queryResult = QueryBuilder.ExpressionParser.GetUpdate(predicate, instance);

                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResult, connection);

                var result = connection.Execute(queryResult.GetSql(), queryResult.Params) > 0;

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return result;
            });
        }

        public bool Update(IEnumerable<TEntity> instances)
        {
            return ExecuteHelper.Execute(connection =>
            {
                if (instances == null || !instances.Any())
                    return false;

                var j = 2000;//每 2k 条执行一次
                var result = 0;
                for (var i = 0; i < instances.Count(); i += 2000)
                {
                    var groupList = instances.Take(j).Skip(i).ToList();
                    j += 2000;

                    foreach (var instance in groupList)
                    {
                        instance.SetCreateAudit();
                    }

                    var queryResult = QueryBuilder.ExpressionParser.GetBulkUpdate(groupList);

                    var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResult, connection);

                    result += connection.Execute(queryResult.GetSql(), queryResult.Params);

                    Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);
                }

                return result == instances.Count();
            });
        }

        public Task<bool> UpdateAsync(TEntity instance)
        {
            return ExecuteHelper.Execute(async connection =>
            {
                instance.SetUpdateAudit();

                var queryResult = QueryBuilder.ExpressionParser.GetUpdate(instance);

                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResult, connection);

                var result = await connection.ExecuteAsync(queryResult.GetSql(), instance) > 0;

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return result;
            });
        }

        public Task<bool> UpdateAsync(TEntity instance, Expression<Func<TEntity, bool>> predicate)
        {
            return ExecuteHelper.Execute(async connection =>
            {
                var queryResult = QueryBuilder.ExpressionParser.GetUpdate(predicate, instance);

                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResult, connection);

                var result = await connection.ExecuteAsync(queryResult.GetSql(), queryResult.Params) > 0;

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return result;
            });
        }

        public Task<bool> UpdateAsync(IEnumerable<TEntity> instances)
        {
            return ExecuteHelper.Execute(async connection =>
            {
                if (instances == null || !instances.Any())
                    return false;

                var j = 2000;//每 2k 条执行一次
                var result = 0;
                for (var i = 0; i < instances.Count(); i += 2000)
                {
                    var groupList = instances.Take(j).Skip(i).ToList();
                    j += 2000;

                    foreach (var instance in groupList)
                    {
                        instance.SetCreateAudit();
                    }

                    var queryResult = QueryBuilder.ExpressionParser.GetBulkUpdate(groupList);

                    var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResult, connection);

                    result += await connection.ExecuteAsync(queryResult.GetSql(), queryResult.Params);

                    Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);
                }

                return result == instances.Count();
            });
        }

        public Task<bool> UpdateAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TEntity>> updateValues, object updateUserId = default)
        {
            return ExecuteHelper.Execute(async connection =>
            {
                var queryResult = QueryBuilder.ExpressionParser.GetUpdate(updateValues, predicate, GetUpdateCommonProperty(updateUserId));

                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResult, connection);

                var result = await connection.ExecuteAsync(queryResult.GetSql(), queryResult.Params) > 0;

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return result;
            });
        }
    }
}
