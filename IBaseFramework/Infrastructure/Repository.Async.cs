using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Dapper;
using IBaseFramework.Base;
using IBaseFramework.DapperExtension;

namespace IBaseFramework.Infrastructure
{
    public partial class Repository<TEntity> where TEntity : EntityBase
    {
        #region Add Async

        /// <inheritdoc />
        public async Task<bool> AddAsync(TEntity instance, IDbTransaction transaction = null)
        {
            return await ExecuteHelper.Execute(async connection =>
            {
                if (instance == null)
                    return false;

                instance.SetCreateAudit(instance.CreateUserId > 0 ? instance.CreateUserId : UserId);

                var queryResult = SqlGenerator.GetInsert(instance);
                if (SqlGenerator.IsIdentity)
                {
                    var newId = (await connection.QueryAsync<long>(queryResult.GetSql(), queryResult.Param, transaction).ConfigureAwait(false))
                        .FirstOrDefault();
                    return SetValue(newId, instance);
                }

                return await connection.ExecuteAsync(queryResult.GetSql(), instance, transaction).ConfigureAwait(false) > 0;
            });
        }


        /// <inheritdoc />
        public async Task<bool> AddAsync(IEnumerable<TEntity> instances, IDbTransaction transaction = null)
        {
            return await ExecuteHelper.Execute(async connection =>
            {
                if (instances == null || !instances.Any())
                    return false;

                foreach (var instance in instances)
                {
                    instance.SetCreateAudit(instance.CreateUserId > 0 ? instance.CreateUserId : UserId);
                }

                var listGroup = new List<List<TEntity>>();
                var j = 2000;//每 2k 条执行一次
                for (var i = 0; i < instances.Count(); i += 2000)
                {
                    var cList = instances.Take(j).Skip(i).ToList();
                    j += 2000;
                    listGroup.Add(cList);
                }

                var result = 0;
                foreach (var groupList in listGroup)
                {
                    var queryResult = SqlGenerator.GetBulkInsert(groupList);
                    result += await connection.ExecuteAsync(queryResult.GetSql(), queryResult.Param, transaction).ConfigureAwait(false);
                }

                return result == instances.Count();
            });
        }

        #endregion

        #region Function Async

        /// <inheritdoc />
        public async Task<int> CountAsync(IDbTransaction transaction = null)
        {
            return await CountAsync(null, transaction).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, IDbTransaction transaction = null)
        {
            return await ExecuteHelper.Execute(async connection =>
            {
                var queryResult = SqlGenerator.GetCount(predicate);
                return await connection.QueryFirstOrDefaultAsync<int>(queryResult.GetSql(), queryResult.Param, transaction).ConfigureAwait(false);
            });
        }

        /// <inheritdoc />
        public async Task<int> CountAsync(Expression<Func<TEntity, object>> distinctField, IDbTransaction transaction = null)
        {
            return await CountAsync(distinctField, null, transaction).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<int> CountAsync(Expression<Func<TEntity, object>> distinctField, Expression<Func<TEntity, bool>> predicate, IDbTransaction transaction = null)
        {
            return await ExecuteHelper.Execute(async connection =>
            {
                var queryResult = SqlGenerator.GetCount(predicate, distinctField);
                return await connection.QueryFirstOrDefaultAsync<int>(queryResult.GetSql(), queryResult.Param, transaction).ConfigureAwait(false);
            });
        }

        /// <inheritdoc />
        public virtual async Task<TResult?> SumAsync<TResult>(Expression<Func<TEntity, TResult>> predicate, Expression<Func<TEntity, bool>> condition, IDbTransaction transaction = null) where TResult : struct
        {
            return await ExecuteHelper.Execute(async connection =>
            {
                var queryResult = SqlGenerator.GetSum(predicate, condition);
                return await connection.QueryFirstOrDefaultAsync<TResult?>(queryResult.GetSql(), queryResult.Param, transaction).ConfigureAwait(false);
            });
        }

        /// <inheritdoc />
        public virtual async Task<TResult?> MinAsync<TResult>(Expression<Func<TEntity, TResult>> predicate, Expression<Func<TEntity, bool>> condition, IDbTransaction transaction = null) where TResult : struct
        {
            return await ExecuteHelper.Execute(async connection =>
            {
                var queryResult = SqlGenerator.GetMin(predicate, condition);
                return await connection.QueryFirstOrDefaultAsync<TResult?>(queryResult.GetSql(), queryResult.Param, transaction).ConfigureAwait(false);
            });
        }

        /// <inheritdoc />
        public virtual async Task<TResult?> MaxAsync<TResult>(Expression<Func<TEntity, TResult>> predicate, Expression<Func<TEntity, bool>> condition, IDbTransaction transaction = null) where TResult : struct
        {
            return await ExecuteHelper.Execute(async connection =>
            {
                var queryResult = SqlGenerator.GetMax(predicate, condition);
                return await connection.QueryFirstOrDefaultAsync<TResult?>(queryResult.GetSql(), queryResult.Param, transaction).ConfigureAwait(false);
            });
        }

        /// <inheritdoc />
        public virtual async Task<TResult?> AvgAsync<TResult>(Expression<Func<TEntity, TResult>> predicate, Expression<Func<TEntity, bool>> condition, IDbTransaction transaction = null) where TResult : struct
        {
            return await ExecuteHelper.Execute(async connection =>
            {
                var queryResult = SqlGenerator.GetAvg(predicate, condition);
                return await connection.QueryFirstOrDefaultAsync<TResult?>(queryResult.GetSql(), queryResult.Param, transaction).ConfigureAwait(false);
            });
        }

        #endregion

        #region Delete Async

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(Expression<Func<TEntity, bool>> predicate, bool isLogicDelete = true, object updateUserId = null, IDbTransaction transaction = null)
        {
            return await ExecuteHelper.Execute(async connection =>
            {
                var userId = updateUserId ?? UserId;

                var queryResult = SqlGenerator.GetDelete(predicate, userId as object, isLogicDelete);
                return await connection.ExecuteAsync(queryResult.GetSql(), queryResult.Param, transaction).ConfigureAwait(false) > 0;
            });
        }


        /// <inheritdoc />
        public async Task<bool> DeleteAsync(TEntity instance, bool isLogicDelete = true, IDbTransaction transaction = null)
        {
            return await ExecuteHelper.Execute(async connection =>
            {
                if (isLogicDelete)
                {
                    instance.SetUpdateAudit(instance.UpdateUserId > 0 ? instance.UpdateUserId : UserId);
                }

                var queryResult = SqlGenerator.GetDelete(instance, isLogicDelete);
                return await connection.ExecuteAsync(queryResult.GetSql(), queryResult.Param, transaction).ConfigureAwait(false) > 0;
            });
        }

        #endregion

        #region Update Async

        /// <inheritdoc />
        public async Task<bool> UpdateAsync(TEntity instance, IDbTransaction transaction = null)
        {
            return await ExecuteHelper.Execute(async connection =>
            {
                instance.SetUpdateAudit(instance.UpdateUserId > 0 ? instance.UpdateUserId : UserId);

                var sqlQuery = SqlGenerator.GetUpdate(instance);
                return await connection.ExecuteAsync(sqlQuery.GetSql(), instance, transaction).ConfigureAwait(false) > 0;
            });
        }

        /// <inheritdoc />
        public async Task<bool> UpdateAsync(TEntity instance, Expression<Func<TEntity, bool>> predicate, IDbTransaction transaction = null)
        {
            return await ExecuteHelper.Execute(async connection =>
            {
                var userId = instance.UpdateUserId > 0 ? instance.UpdateUserId : UserId;

                var sqlQuery = SqlGenerator.GetUpdate(predicate, instance, userId as object);
                return await connection.ExecuteAsync(sqlQuery.GetSql(), sqlQuery.Param, transaction).ConfigureAwait(false) > 0;
            });
        }

        /// <inheritdoc />
        public async Task<bool> UpdateAsync(IEnumerable<TEntity> instances, IDbTransaction transaction = null)
        {
            return await ExecuteHelper.Execute(async connection =>
            {
                var entities = instances.ToList();
                foreach (var instance in entities)
                {
                    instance.SetUpdateAudit(instance.UpdateUserId > 0 ? instance.UpdateUserId : UserId);
                }

                var queryResult = SqlGenerator.GetBulkUpdate(entities);
                return await connection.ExecuteAsync(queryResult.GetSql(), queryResult.Param, transaction).ConfigureAwait(false) > 0;
            });
        }

        /// <inheritdoc />
        public async Task<bool> UpdateAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TEntity>> updateValues, object updateUserId = null, IDbTransaction transaction = null)
        {
            return await ExecuteHelper.Execute(async connection =>
            {
                var userId = updateUserId ?? UserId;

                var queryResult = SqlGenerator.GetUpdate(updateValues, predicate, userId as object);
                return await connection.ExecuteAsync(queryResult.GetSql(), queryResult.Param, transaction).ConfigureAwait(false) > 0;
            });
        }

        #endregion

        #region Get Async

        /// <inheritdoc />
        public async Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> filterColumns = null, IDbTransaction transaction = null)
        {
            return await ExecuteHelper.Execute(async connection =>
            {
                var queryResult = SqlGenerator.GetSelect(predicate, filterColumns, true);
                return await connection.QueryFirstOrDefaultAsync<TEntity>(queryResult.GetSql(), queryResult.Param, transaction).ConfigureAwait(false);
            });
        }

        /// <inheritdoc />
        public async Task<TEntity> GetAsync(object id, Expression<Func<TEntity, object>> filterColumns = null, IDbTransaction transaction = null)
        {
            return await ExecuteHelper.Execute(async connection =>
            {
                var queryResult = SqlGenerator.GetSelectById(id, filterColumns);
                return await connection.QuerySingleOrDefaultAsync<TEntity>(queryResult.GetSql(), queryResult.Param, transaction).ConfigureAwait(false);
            });
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, object>> filterColumns = null, IDbTransaction transaction = null)
        {
            return await GetListAsync(null, filterColumns, transaction).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> filterColumns = null, IDbTransaction transaction = null)
        {
            return await ExecuteHelper.Execute(async connection =>
            {
                var queryResult = SqlGenerator.GetSelect(predicate, filterColumns, false);
                return await connection.QueryAsync<TEntity>(queryResult.GetSql(), queryResult.Param, transaction).ConfigureAwait(false);
            });
        }

        /// <inheritdoc />
        public async Task<PagedList<TEntity>> GetPageListAsync(Expression<Func<TEntity, bool>> predicate, int pageIndex, int pageSize, string orderBy = null, Expression<Func<TEntity, object>> filterColumns = null)
        {
            return await ExecuteHelper.Execute(async connection =>
            {
                var sqlQuery = SqlGenerator.GetSelect(predicate, filterColumns, false);
                if (!string.IsNullOrEmpty(orderBy))
                {
                    sqlQuery.SqlBuilder.Append(" order by " + orderBy);
                }

                var result = PageHelper.Page(sqlQuery.GetSql(), sqlQuery.Param, pageIndex, pageSize);

                var total = await connection.QueryFirstOrDefaultAsync<int>(result.Item1.GetSql(), result.Item1.Param).ConfigureAwait(false);
                var data = await connection.QueryAsync<TEntity>(result.Item2.GetSql(), result.Item2.Param).ConfigureAwait(false);

                return new PagedList<TEntity>()
                {
                    PageIndex = result.Item3,
                    PageSize = result.Item4,
                    TotalCount = total,
                    Items = data
                };
            });
        }

        #endregion

        #region Exist Async

        /// <inheritdoc />
        public async Task<bool> ExistAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await ExecuteHelper.Execute(async connection =>
            {
                var queryResult = SqlGenerator.GetCount(predicate);
                return await connection.QueryFirstOrDefaultAsync<int>(queryResult.GetSql(), queryResult.Param).ConfigureAwait(false) > 0;
            });
        }

        #endregion

    }
}
