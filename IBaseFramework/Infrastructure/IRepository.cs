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

        #region Count

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

        #endregion

        #region Get

        /// <summary>
        ///     Get first object
        /// </summary>
        TEntity Get(Expression<Func<TEntity, bool>> predicate, IDbTransaction transaction = null);

        /// <summary>
        ///     Get object by Id
        /// </summary>
        TEntity Get(object id, IDbTransaction transaction = null);

        /// <summary>
        ///     Get all objects
        /// </summary>
        IEnumerable<TEntity> GetAll(IDbTransaction transaction = null);

        /// <summary>
        ///     Get all objects
        /// </summary>
        IEnumerable<TEntity> GetList(Expression<Func<TEntity, bool>> predicate, IDbTransaction transaction = null);

        /// <summary>
        ///     Get page objects
        /// </summary>
        PagedList<TEntity> GetPageList(Expression<Func<TEntity, bool>> predicate, int pageIndex, int pageSize, string orderBy = null);

        #endregion

        #region In

        /// <summary>
        ///     Get objects by keys
        /// </summary>
        IEnumerable<TEntity> In(IEnumerable<dynamic> keys);

        /// <summary>
        ///     Get objects by keys
        /// </summary>
        IEnumerable<TEntity> In(Expression<Func<TEntity, object>> field, IEnumerable<dynamic> keys);

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
        bool Update(Expression<Func<TEntity, TEntity>> updateValues, Expression<Func<TEntity, bool>> predicate, object updateUserId = null, IDbTransaction transaction = null);

        #endregion

    }

    public partial interface IRepository<TEntity> where TEntity : EntityBase
    {
        #region Count

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

        #endregion

        #region Get

        /// <summary>
        ///     Get object by Id
        /// </summary>
        Task<TEntity> GetAsync(object id, IDbTransaction transaction = null);

        /// <summary>
        ///     Get first object
        /// </summary>
        Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate, IDbTransaction transaction = null);

        /// <summary>
        ///     Get all objects
        /// </summary>
        Task<IEnumerable<TEntity>> GetAllAsync(IDbTransaction transaction = null);

        /// <summary>
        ///     Get all objects
        /// </summary>
        Task<IEnumerable<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, IDbTransaction transaction = null);

        /// <summary>
        ///     Get page objects
        /// </summary>
        Task<PagedList<TEntity>> GetPageListAsync(Expression<Func<TEntity, bool>> predicate, int pageIndex, int pageSize, string orderBy = null);

        #endregion

        #region In

        /// <summary>
        ///     Get objects by keys
        /// </summary>
        Task<IEnumerable<TEntity>> InAsync(IEnumerable<dynamic> keys);

        /// <summary>
        ///     Get objects by keys
        /// </summary>
        Task<IEnumerable<TEntity>> InAsync(Expression<Func<TEntity, object>> field, IEnumerable<dynamic> keys);

        #endregion

        #region Exist

        /// <summary>
        ///     exist
        /// </summary>
        Task<bool> ExistAsync(Expression<Func<TEntity, bool>> predicate);

        #endregion

        #region Add

        /// <summary>
        ///     Insert object to DB
        /// </summary>
        Task<bool> AddAsync(TEntity instance, IDbTransaction transaction = null);

        /// <summary>
        ///     Bulk Insert objects to DB
        /// </summary>
        Task<bool> AddAsync(IEnumerable<TEntity> instances, IDbTransaction transaction = null);

        #endregion

        #region Delete

        /// <summary>
        ///     Delete object from DB
        /// </summary>
        Task<bool> DeleteAsync(TEntity instance, bool isLogicDelete = true, IDbTransaction transaction = null);

        /// <summary>
        ///     Delete objects from DB
        /// </summary>
        Task<bool> DeleteAsync(Expression<Func<TEntity, bool>> predicate, bool isLogicDelete = true, object updateUserId = null, IDbTransaction transaction = null);

        #endregion

        #region Update

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
        Task<bool> UpdateAsync(Expression<Func<TEntity, TEntity>> updateValues, Expression<Func<TEntity, bool>> predicate, object updateUserId = null, IDbTransaction transaction = null);

        #endregion
    }
}