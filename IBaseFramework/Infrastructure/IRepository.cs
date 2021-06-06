using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;
using IBaseFramework.Base;
using IBaseFramework.DapperExtension;

namespace IBaseFramework.Infrastructure
{
    /// <summary>
    ///     interface for repository
    /// </summary>
    public partial interface IRepository<TEntity> where TEntity : EntityBase
    {
        Expression<Func<TEntity, bool>> ExpressionTrue();

        #region Function

        /// <summary>
        ///     Get number of rows
        /// </summary>
        int Count(IDbTransaction transaction = null);

        /// <summary>
        ///     Get number of rows with WHERE clause
        /// </summary>
        int Count(Expression<Func<TEntity, bool>> predicate, IDbTransaction transaction = null);

        /// <summary>
        ///     Get number of rows with DISTINCT clause
        /// </summary>
        int Count(Expression<Func<TEntity, object>> distinctField, IDbTransaction transaction = null);

        /// <summary>
        ///     Get number of rows with DISTINCT and WHERE clause
        /// </summary>
        int Count(Expression<Func<TEntity, object>> distinctField, Expression<Func<TEntity, bool>> predicate, IDbTransaction transaction = null);

        /// <summary>
        ///     Get number of Sum with WHERE clause
        /// </summary>
        TResult? Sum<TResult>(Expression<Func<TEntity, TResult>> predicate, Expression<Func<TEntity, bool>> condition, IDbTransaction transaction = null) where TResult : struct;

        /// <summary>
        ///     Get number of Min with WHERE clause
        /// </summary>
        TResult? Min<TResult>(Expression<Func<TEntity, TResult>> predicate, Expression<Func<TEntity, bool>> condition, IDbTransaction transaction = null) where TResult : struct;

        /// <summary>
        ///     Get number of Max with WHERE clause
        /// </summary>
        TResult? Max<TResult>(Expression<Func<TEntity, TResult>> predicate, Expression<Func<TEntity, bool>> condition, IDbTransaction transaction = null) where TResult : struct;

        /// <summary>
        ///    Get number of Avg with WHERE clause
        /// </summary>
        TResult? Avg<TResult>(Expression<Func<TEntity, TResult>> predicate, Expression<Func<TEntity, bool>> condition, IDbTransaction transaction = null) where TResult : struct;

        #endregion

        #region Get

        /// <summary>
        ///     Get first object
        /// </summary>
        TEntity Get(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> filterColumns = null, IDbTransaction transaction = null);

        /// <summary>
        ///     Get object by Id
        /// </summary>
        TEntity Get(object id, Expression<Func<TEntity, object>> filterColumns = null, IDbTransaction transaction = null);

        /// <summary>
        ///     Get all objects
        /// </summary>
        IEnumerable<TEntity> GetAll(Expression<Func<TEntity, object>> filterColumns = null, IDbTransaction transaction = null);

        /// <summary>
        ///     Get all objects
        /// </summary>
        IEnumerable<TEntity> GetList(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> filterColumns = null, IDbTransaction transaction = null);

        /// <summary>
        ///     Get page objects
        /// </summary>
        PagedList<TEntity> GetPageList(Expression<Func<TEntity, bool>> predicate, int pageIndex, int pageSize, string orderBy = null, Expression<Func<TEntity, object>> filterColumns = null);

        #endregion

        #region Exist

        /// <summary>
        ///     exist
        /// </summary>
        bool Exist(Expression<Func<TEntity, bool>> predicate);

        #endregion

        #region Add

        /// <summary>
        ///     Insert object to DB
        /// </summary>
        bool Add(TEntity instance, IDbTransaction transaction = null);

        /// <summary>
        ///     Bulk Insert objects to DB
        /// </summary>
        bool Add(IEnumerable<TEntity> instances, IDbTransaction transaction = null);

        #endregion

        #region Delete

        /// <summary>
        ///     Delete object from DB
        /// </summary>
        bool Delete(TEntity instance, bool isLogicDelete = true, IDbTransaction transaction = null);

        /// <summary>
        ///     Delete objects from DB
        /// </summary>
        bool Delete(Expression<Func<TEntity, bool>> predicate, bool isLogicDelete = true, object updateUserId = null, IDbTransaction transaction = null);

        #endregion

        #region Update

        /// <summary>
        ///     Update object in DB
        /// </summary>
        bool Update(TEntity instance, IDbTransaction transaction = null);

        /// <summary>
        ///     Update object in DB
        /// </summary>
        bool Update(TEntity instance, Expression<Func<TEntity, bool>> predicate, IDbTransaction transaction = null);

        /// <summary>
        ///     Bulk Update objects in DB
        /// </summary>
        bool Update(IEnumerable<TEntity> instances, IDbTransaction transaction = null);

        /// <summary>
        ///     Update fields in DB
        /// </summary>
        bool Update(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TEntity>> updateValues, object updateUserId = null, IDbTransaction transaction = null);

        #endregion

    }

    public partial interface IRepository<TEntity> where TEntity : EntityBase
    {
        #region FunctionAsync

        /// <summary>
        ///     Get number of rows
        /// </summary>
        Task<int> CountAsync(IDbTransaction transaction = null);

        /// <summary>
        ///     Get number of rows with WHERE clause
        /// </summary>
        Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, IDbTransaction transaction = null);

        /// <summary>
        ///     Get number of rows with DISTINCT clause
        /// </summary>
        Task<int> CountAsync(Expression<Func<TEntity, object>> distinctField, IDbTransaction transaction = null);

        /// <summary>
        ///     Get number of rows with DISTINCT and WHERE clause
        /// </summary>
        Task<int> CountAsync(Expression<Func<TEntity, object>> distinctField, Expression<Func<TEntity, bool>> predicate, IDbTransaction transaction = null);

        /// <summary>
        ///     Get number of Sum with WHERE clause
        /// </summary>
        Task<TResult?> SumAsync<TResult>(Expression<Func<TEntity, TResult>> predicate, Expression<Func<TEntity, bool>> condition, IDbTransaction transaction = null) where TResult : struct;

        /// <summary>
        ///     Get number of Min with WHERE clause
        /// </summary>
        Task<TResult?> MinAsync<TResult>(Expression<Func<TEntity, TResult>> predicate, Expression<Func<TEntity, bool>> condition, IDbTransaction transaction = null) where TResult : struct;

        /// <summary>
        ///     Get number of Max with WHERE clause
        /// </summary>
        Task<TResult?> MaxAsync<TResult>(Expression<Func<TEntity, TResult>> predicate, Expression<Func<TEntity, bool>> condition, IDbTransaction transaction = null) where TResult : struct;

        /// <summary>
        ///    Get number of Avg with WHERE clause
        /// </summary>
        Task<TResult?> AvgAsync<TResult>(Expression<Func<TEntity, TResult>> predicate, Expression<Func<TEntity, bool>> condition, IDbTransaction transaction = null) where TResult : struct;

        #endregion

        #region GetAsync

        /// <summary>
        ///     Get object by Id
        /// </summary>
        Task<TEntity> GetAsync(object id, Expression<Func<TEntity, object>> filterColumns = null, IDbTransaction transaction = null);

        /// <summary>
        ///     Get first object
        /// </summary>
        Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> filterColumns = null, IDbTransaction transaction = null);

        /// <summary>
        ///     Get all objects
        /// </summary>
        Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, object>> filterColumns = null, IDbTransaction transaction = null);

        /// <summary>
        ///     Get all objects
        /// </summary>
        Task<IEnumerable<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> filterColumns = null, IDbTransaction transaction = null);

        /// <summary>
        ///     Get page objects
        /// </summary>
        Task<PagedList<TEntity>> GetPageListAsync(Expression<Func<TEntity, bool>> predicate, int pageIndex, int pageSize, string orderBy = null, Expression<Func<TEntity, object>> filterColumns = null);

        #endregion        

        #region ExistAsync

        /// <summary>
        ///     exist
        /// </summary>
        Task<bool> ExistAsync(Expression<Func<TEntity, bool>> predicate);

        #endregion

        #region AddAsync

        /// <summary>
        ///     Insert object to DB
        /// </summary>
        Task<bool> AddAsync(TEntity instance, IDbTransaction transaction = null);

        /// <summary>
        ///     Bulk Insert objects to DB
        /// </summary>
        Task<bool> AddAsync(IEnumerable<TEntity> instances, IDbTransaction transaction = null);

        #endregion

        #region DeleteAsync

        /// <summary>
        ///     Delete object from DB
        /// </summary>
        Task<bool> DeleteAsync(TEntity instance, bool isLogicDelete = true, IDbTransaction transaction = null);

        /// <summary>
        ///     Delete objects from DB
        /// </summary>
        Task<bool> DeleteAsync(Expression<Func<TEntity, bool>> predicate, bool isLogicDelete = true, object updateUserId = null, IDbTransaction transaction = null);

        #endregion

        #region UpdateAsync

        /// <summary>
        ///     Update object in DB
        /// </summary>
        Task<bool> UpdateAsync(TEntity instance, IDbTransaction transaction = null);

        /// <summary>
        ///     Bulk Update objects in DB
        /// </summary>
        Task<bool> UpdateAsync(IEnumerable<TEntity> instances, IDbTransaction transaction = null);

        /// <summary>
        ///     Update object in DB
        /// </summary>
        Task<bool> UpdateAsync(TEntity instance, Expression<Func<TEntity, bool>> predicate, IDbTransaction transaction = null);

        /// <summary>
        ///     Update fields in DB
        /// </summary>
        Task<bool> UpdateAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TEntity>> updateValues, object updateUserId = null, IDbTransaction transaction = null);

        #endregion

    }
}