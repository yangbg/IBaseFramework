using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using Dapper;
using IBaseFramework.Base;
using IBaseFramework.DapperExtension;

namespace IBaseFramework.Infrastructure
{
    public partial class Repository<TEntity> where TEntity : EntityBase
    {
        #region Add

        /// <inheritdoc />
        public bool Add(TEntity instance, IDbTransaction transaction = null)
        {
            return ExecuteHelper.Execute(connection =>
            {
                if (instance == null)
                    return false;

                instance.SetCreateAudit(instance.CreateUserId > 0 ? instance.CreateUserId : UserId);

                var queryResult = SqlGenerator.GetInsert(instance);
                if (SqlGenerator.IsIdentity)
                {
                    var newId = connection.Query<long>(queryResult.GetSql(), queryResult.Param, transaction)
                        .FirstOrDefault();
                    return SetValue(newId, instance);
                }

                return connection.Execute(queryResult.GetSql(), instance, transaction) > 0;
            });
        }

        /// <inheritdoc />
        public bool Add(IEnumerable<TEntity> instances, IDbTransaction transaction = null)
        {
            return ExecuteHelper.Execute(connection =>
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
                    result += connection.Execute(queryResult.GetSql(), queryResult.Param, transaction);
                }

                return result == instances.Count();
            });
        }

        #endregion

        #region Function

        /// <inheritdoc />
        public int Count(IDbTransaction transaction = null)
        {
            return Count(null, transaction);
        }

        /// <inheritdoc />
        public int Count(Expression<Func<TEntity, bool>> predicate, IDbTransaction transaction = null)
        {
            return ExecuteHelper.Execute(connection =>
            {
                var queryResult = SqlGenerator.GetCount(predicate);
                return connection.QueryFirstOrDefault<int>(queryResult.GetSql(), queryResult.Param, transaction);
            });
        }

        /// <inheritdoc />
        public int Count(Expression<Func<TEntity, object>> distinctField, IDbTransaction transaction = null)
        {
            return Count(distinctField, null, transaction);
        }

        /// <inheritdoc />
        public int Count(Expression<Func<TEntity, object>> distinctField, Expression<Func<TEntity, bool>> predicate, IDbTransaction transaction = null)
        {
            return ExecuteHelper.Execute(connection =>
            {
                var queryResult = SqlGenerator.GetCount(predicate, distinctField);
                return connection.QueryFirstOrDefault<int>(queryResult.GetSql(), queryResult.Param, transaction);
            });
        }

        /// <inheritdoc />
        public virtual TResult? Sum<TResult>(Expression<Func<TEntity, TResult>> predicate, Expression<Func<TEntity, bool>> condition, IDbTransaction transaction = null) where TResult : struct
        {
            return ExecuteHelper.Execute(connection =>
            {
                var queryResult = SqlGenerator.GetSum(predicate, condition);
                return connection.QueryFirstOrDefault<TResult?>(queryResult.GetSql(), queryResult.Param, transaction);
            });
        }

        /// <inheritdoc />
        public virtual TResult? Min<TResult>(Expression<Func<TEntity, TResult>> predicate, Expression<Func<TEntity, bool>> condition, IDbTransaction transaction = null) where TResult : struct
        {
            return ExecuteHelper.Execute(connection =>
            {
                var queryResult = SqlGenerator.GetMin(predicate, condition);
                return connection.QueryFirstOrDefault<TResult?>(queryResult.GetSql(), queryResult.Param, transaction);
            });
        }

        /// <inheritdoc />
        public virtual TResult? Max<TResult>(Expression<Func<TEntity, TResult>> predicate, Expression<Func<TEntity, bool>> condition, IDbTransaction transaction = null) where TResult : struct
        {
            return ExecuteHelper.Execute(connection =>
            {
                var queryResult = SqlGenerator.GetMax(predicate, condition);
                return connection.QueryFirstOrDefault<TResult?>(queryResult.GetSql(), queryResult.Param, transaction);
            });
        }

        /// <inheritdoc />
        public virtual TResult? Avg<TResult>(Expression<Func<TEntity, TResult>> predicate, Expression<Func<TEntity, bool>> condition, IDbTransaction transaction = null) where TResult : struct
        {
            return ExecuteHelper.Execute(connection =>
            {
                var queryResult = SqlGenerator.GetAvg(predicate, condition);
                return connection.QueryFirstOrDefault<TResult?>(queryResult.GetSql(), queryResult.Param, transaction);
            });
        }

        #endregion

        #region Delete

        /// <inheritdoc />
        public bool Delete(TEntity instance, bool isLogicDelete = true, IDbTransaction transaction = null)
        {
            return ExecuteHelper.Execute(connection =>
            {
                if (isLogicDelete)
                {
                    instance.SetUpdateAudit(instance.UpdateUserId > 0 ? instance.UpdateUserId : UserId);
                }

                var queryResult = SqlGenerator.GetDelete(instance, isLogicDelete);
                return connection.Execute(queryResult.GetSql(), queryResult.Param, transaction) > 0;
            });
        }

        /// <inheritdoc />
        public bool Delete(Expression<Func<TEntity, bool>> predicate, bool isLogicDelete = true, object updateUserId = null, IDbTransaction transaction = null)
        {
            return ExecuteHelper.Execute(connection =>
            {
                var userId = updateUserId ?? UserId;

                var queryResult = SqlGenerator.GetDelete(predicate, userId as object, isLogicDelete);
                return connection.Execute(queryResult.GetSql(), queryResult.Param, transaction) > 0;
            });
        }

        #endregion

        #region Update

        /// <inheritdoc />
        public bool Update(TEntity instance, IDbTransaction transaction = null)
        {
            return ExecuteHelper.Execute(connection =>
            {
                instance.SetUpdateAudit(instance.UpdateUserId > 0 ? instance.UpdateUserId : UserId);

                var sqlQuery = SqlGenerator.GetUpdate(instance);
                return connection.Execute(sqlQuery.GetSql(), instance, transaction) > 0;
            });
        }

        /// <inheritdoc />
        public bool Update(TEntity instance, Expression<Func<TEntity, bool>> predicate, IDbTransaction transaction = null)
        {
            return ExecuteHelper.Execute(connection =>
            {
                var userId = instance.UpdateUserId > 0 ? instance.UpdateUserId : UserId;

                var sqlQuery = SqlGenerator.GetUpdate(predicate, instance, userId as object);
                return connection.Execute(sqlQuery.GetSql(), sqlQuery.Param, transaction) > 0;
            });
        }

        /// <inheritdoc />
        public bool Update(IEnumerable<TEntity> instances, IDbTransaction transaction = null)
        {
            return ExecuteHelper.Execute(connection =>
            {
                var entities = instances.ToList();
                foreach (var instance in entities)
                {
                    instance.SetUpdateAudit(instance.UpdateUserId > 0 ? instance.UpdateUserId : UserId);
                }

                var queryResult = SqlGenerator.GetBulkUpdate(entities);
                return connection.Execute(queryResult.GetSql(), queryResult.Param, transaction) > 0;
            });
        }

        /// <inheritdoc />
        public bool Update(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TEntity>> updateValues, object updateUserId = null, IDbTransaction transaction = null)
        {
            return ExecuteHelper.Execute(connection =>
            {
                var userId = updateUserId ?? UserId;

                var queryResult = SqlGenerator.GetUpdate(updateValues, predicate, userId as object);
                return connection.Execute(queryResult.GetSql(), queryResult.Param, transaction) > 0;
            });
        }

        #endregion

        #region Get

        /// <inheritdoc />
        public TEntity Get(object id, Expression<Func<TEntity, object>> filterColumns = null, IDbTransaction transaction = null)
        {
            return ExecuteHelper.Execute(connection =>
            {
                var queryResult = SqlGenerator.GetSelectById(id, filterColumns);
                return connection.QuerySingleOrDefault<TEntity>(queryResult.GetSql(), queryResult.Param, transaction);
            });
        }

        /// <inheritdoc />
        public TEntity Get(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> filterColumns = null, IDbTransaction transaction = null)
        {
            return ExecuteHelper.Execute(connection =>
            {
                var queryResult = SqlGenerator.GetSelect(predicate, filterColumns, true);
                return connection.QueryFirstOrDefault<TEntity>(queryResult.GetSql(), queryResult.Param, transaction);
            });
        }

        /// <inheritdoc />
        public IEnumerable<TEntity> GetAll(Expression<Func<TEntity, object>> filterColumns = null, IDbTransaction transaction = null)
        {
            return GetList(null, filterColumns, transaction);
        }

        /// <inheritdoc />
        public IEnumerable<TEntity> GetList(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> filterColumns = null, IDbTransaction transaction = null)
        {
            return ExecuteHelper.Execute(connection =>
            {
                var queryResult = SqlGenerator.GetSelect(predicate, filterColumns, false);
                return connection.Query<TEntity>(queryResult.GetSql(), queryResult.Param, transaction);
            });
        }

        /// <inheritdoc />
        public PagedList<TEntity> GetPageList(Expression<Func<TEntity, bool>> predicate, int pageIndex, int pageSize, string orderBy = null, Expression<Func<TEntity, object>> filterColumns = null)
        {
            return ExecuteHelper.Execute(connection =>
            {
                var sqlQuery = SqlGenerator.GetSelect(predicate, filterColumns, false);
                if (!string.IsNullOrEmpty(orderBy))
                {
                    sqlQuery.SqlBuilder.Append(" order by " + orderBy);
                }

                var result = PageHelper.Page(sqlQuery.GetSql(), sqlQuery.Param, pageIndex, pageSize);

                var total = connection.QueryFirstOrDefault<int>(result.Item1.GetSql(), result.Item1.Param);
                var data = connection.Query<TEntity>(result.Item2.GetSql(), result.Item2.Param);

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

        #region Exist

        /// <inheritdoc />
        public bool Exist(Expression<Func<TEntity, bool>> predicate)
        {
            return ExecuteHelper.Execute(connection =>
            {
                var queryResult = SqlGenerator.GetCount(predicate);
                return connection.QueryFirstOrDefault<int>(queryResult.GetSql(), queryResult.Param) > 0;
            });
        }

        #endregion

    }
}
