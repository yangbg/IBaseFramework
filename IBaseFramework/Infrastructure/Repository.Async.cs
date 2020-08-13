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
                instance.SetCreateAudit(instance.CreateUserId > 0 ? instance.CreateUserId : UserId);

                var queryResult = SqlGenerator.GetInsert(instance);
                if (SqlGenerator.IsIdentity)
                {
                    var newId = (await connection.QueryAsync<long>(queryResult.GetSql(), queryResult.Param, transaction).ConfigureAwait(false))
                        .FirstOrDefault();
                    return SetValue(newId, instance);
                }

                return await connection.ExecuteAsync(queryResult.GetSql(), instance, transaction) > 0;
            });
        }


        /// <inheritdoc />
        public async Task<bool> AddAsync(IEnumerable<TEntity> instances, IDbTransaction transaction = null)
        {
            return await ExecuteHelper.Execute(async connection =>
            {
                var entities = instances.ToList();
                foreach (var instance in entities)
                {
                    instance.SetCreateAudit(instance.CreateUserId > 0 ? instance.CreateUserId : UserId);
                }

                var listGroup = new List<List<TEntity>>();
                var j = 2000;//每 2k 条执行一次
                for (var i = 0; i < entities.Count; i += 2000)
                {
                    var cList = entities.Take(j).Skip(i).ToList();
                    j += 2000;
                    listGroup.Add(cList);
                }

                var result = 0;
                foreach (var groupList in listGroup)
                {
                    var queryResult = SqlGenerator.GetBulkInsert(groupList);
                    result += await connection.ExecuteAsync(queryResult.GetSql(), queryResult.Param, transaction);
                }

                return result == entities.Count;
            });
        }

        #endregion

        #region Count Async

        /// <inheritdoc />
        public async Task<int> CountAsync(IDbTransaction transaction = null)
        {
            return await CountAsync(null, transaction);
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
            return await CountAsync(distinctField, null, transaction);
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

        #endregion

        #region Delete Async

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(Expression<Func<TEntity, bool>> predicate, bool isLogicDelete = true, object updateUserId = null, IDbTransaction transaction = null)
        {
            return await ExecuteHelper.Execute(async connection =>
            {
                var userId = updateUserId ?? UserId;

                var queryResult = SqlGenerator.GetDelete(predicate, userId as object, isLogicDelete);
                return await connection.ExecuteAsync(queryResult.GetSql(), queryResult.Param, transaction) > 0;
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
                return await connection.ExecuteAsync(queryResult.GetSql(), queryResult.Param, transaction) > 0;
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
                return await connection.ExecuteAsync(sqlQuery.GetSql(), instance, transaction) > 0;
            });
        }

        /// <inheritdoc />
        public async Task<bool> UpdateAsync(TEntity instance, Expression<Func<TEntity, bool>> predicate, IDbTransaction transaction = null)
        {
            return await _unitOfWork.Execute(async connection =>
            {
                var userId = instance.UpdateUserId > 0 ? instance.UpdateUserId : UserId;

                var sqlQuery = SqlGenerator.GetUpdate(predicate, instance, userId as object);
                return await connection.ExecuteAsync(sqlQuery.GetSql(), sqlQuery.Param, transaction) > 0;
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
                return await connection.ExecuteAsync(queryResult.GetSql(), queryResult.Param, transaction) > 0;
            });
        }

        /// <inheritdoc />
        public async Task<bool> UpdateAsync(Expression<Func<TEntity, TEntity>> updateValues, Expression<Func<TEntity, bool>> predicate, object updateUserId = null, IDbTransaction transaction = null)
        {
            return await ExecuteHelper.Execute(async connection =>
            {
                var userId = updateUserId ?? UserId;

                var queryResult = SqlGenerator.GetUpdate(updateValues, predicate, userId as object);
                return await connection.ExecuteAsync(queryResult.GetSql(), queryResult.Param, transaction) > 0;
            });
        }

        #endregion

        #region Get Async

        /// <inheritdoc />
        public async Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate, IDbTransaction transaction = null)
        {
            return await ExecuteHelper.Execute(async connection =>
            {
                var queryResult = SqlGenerator.GetSelect(predicate, true);
                return await connection.QueryFirstOrDefaultAsync<TEntity>(queryResult.GetSql(), queryResult.Param,
                    transaction).ConfigureAwait(false);
            });
        }

        /// <inheritdoc />
        public async Task<TEntity> GetAsync(object id, IDbTransaction transaction = null)
        {
            return await ExecuteHelper.Execute(async connection =>
            {
                var queryResult = SqlGenerator.GetSelectById(id);
                return await connection.QuerySingleOrDefaultAsync<TEntity>(queryResult.GetSql(), queryResult.Param,
                    transaction).ConfigureAwait(false);
            });
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TEntity>> GetAllAsync(IDbTransaction transaction = null)
        {
            return await GetListAsync(null, transaction);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, IDbTransaction transaction = null)
        {
            return await ExecuteHelper.Execute(async connection =>
            {
                var queryResult = SqlGenerator.GetSelect(predicate, false);
                return await connection.QueryAsync<TEntity>(queryResult.GetSql(), queryResult.Param, transaction).ConfigureAwait(false);
            });
        }

        /// <inheritdoc />
        public async Task<PagedList<TEntity>> GetPageListAsync(Expression<Func<TEntity, bool>> predicate, int pageIndex, int pageSize, string orderBy = null)
        {
            return await ExecuteHelper.Execute(async connection =>
            {
                var sqlQuery = SqlGenerator.GetSelect(predicate, false);
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

        #region In Async

        /// <inheritdoc />
        public async Task<IEnumerable<TEntity>> InAsync(IEnumerable<dynamic> keys)
        {
            return await ExecuteHelper.Execute(async connection =>
            {
                var objects = keys.ToList();
                if (!objects.Any())
                    return null;

                var sqlQuery = SqlGenerator.GetIn(objects);

                return await connection.QueryAsync<TEntity>(sqlQuery.GetSql(), sqlQuery.Param).ConfigureAwait(false);
            });
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TEntity>> InAsync(Expression<Func<TEntity, object>> field, IEnumerable<dynamic> keys)
        {
            return await ExecuteHelper.Execute(async connection =>
            {
                var objects = keys.ToList();
                if (!objects.Any())
                    return null;

                var sqlQuery = SqlGenerator.GetIn(objects, field);
                return await connection.QueryAsync<TEntity>(sqlQuery.GetSql(), sqlQuery.Param).ConfigureAwait(false);
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
