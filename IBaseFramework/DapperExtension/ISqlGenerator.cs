using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace IBaseFramework.DapperExtension
{
    /// <summary>
    ///     Universal SqlGenerator for Tables
    /// </summary>
    public interface ISqlGenerator<TEntity> where TEntity : class
    {
        /// <summary>
        ///     Is Autoincrement table
        /// </summary>
        bool IsIdentity { get; }

        /// <summary>
        ///     Identity Metadata property
        /// </summary>
        PropertyInfo IdentitySqlProperty { get; }

        /// <summary>
        ///     Get SQL for COUNT Query
        /// </summary>
        SqlQuery GetCount(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        ///     Get SQL for COUNT with DISTINCT Query
        /// </summary>
        SqlQuery GetCount(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> distinctField);

        /// <summary>
        ///     Get SQL for INSERT Query
        /// </summary>
        SqlQuery GetInsert(TEntity entity);

        /// <summary>
        ///     Get SQL for bulk INSERT Query
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        SqlQuery GetBulkInsert(IEnumerable<TEntity> entities);

        /// <summary>
        ///     Get SQL for UPDATE Query
        /// </summary>
        SqlQuery GetUpdate(TEntity entity);

        /// <summary>
        ///     Get SQL for UPDATE Query
        /// </summary>
        SqlQuery GetUpdate(Expression<Func<TEntity, bool>> predicate, TEntity entity, object userId);

        /// <summary>
        ///     Get SQL for bulk UPDATE Query
        /// </summary>
        SqlQuery GetBulkUpdate(IEnumerable<TEntity> entities);

        /// <summary>
        ///    Get SQL for UPDATE Query
        /// </summary>
        SqlQuery GetUpdate(Expression<Func<TEntity, TEntity>> updateValues, Expression<Func<TEntity, bool>> predicate, object userId);

        /// <summary>
        ///     Get SQL for SELECT Query by Id
        /// </summary>
        SqlQuery GetSelectById(object id);
        
        /// <summary>
        ///     Get SQL for SELECT Query
        /// </summary>
        SqlQuery GetSelect(Expression<Func<TEntity, bool>> predicate, bool getFirst);

        /// <summary>
        ///     Get SQL for DELETE Query
        /// </summary>
        SqlQuery GetDelete(TEntity entity, bool isLogicDelete = true);

        /// <summary>
        ///     Get SQL for DELETE Query
        /// </summary>
        SqlQuery GetDelete(Expression<Func<TEntity, bool>> predicate, object userId, bool isLogicDelete = true);

        /// <summary>
        ///    Get in
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        SqlQuery GetIn(IEnumerable<dynamic> keys, Expression<Func<TEntity, object>> field = null);
    }
}
