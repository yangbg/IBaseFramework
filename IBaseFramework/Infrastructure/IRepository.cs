using IBaseFramework.Base;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IBaseFramework.Infrastructure
{
    public partial interface IRepository
    {
        IQueryBuilder QueryBuilder { get; set; }
    }

    public partial interface IQueryRepository<out T> : IRepository
    {
    }

    public partial interface IRepository<TEntity> : IQueryRepository<TEntity> where TEntity : EntityBaseEmpty
    {
        Expression<Func<TEntity, bool>> ExpressionTrue();
        Expression<Func<TEntity, bool>> ExpressionFalse();

        TEntity Add(TEntity instance);
        bool Add(IEnumerable<TEntity> instances);

        bool Delete(TEntity instance, bool isLogicDelete = true);
        bool Delete(Expression<Func<TEntity, bool>> predicate, bool isLogicDelete = true, object updateUserId = default);

        bool Update(TEntity instance);
        bool Update(TEntity instance, Expression<Func<TEntity, bool>> predicate);
        bool Update(IEnumerable<TEntity> instances);
        bool Update(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TEntity>> updateValues, object updateUserId = default);

        bool Exist(Expression<Func<TEntity, bool>> predicate);

        TEntity Get<TKey>(TKey id);
        TEntity Get(Expression<Func<TEntity, bool>> predicate);
        IEnumerable<TEntity> GetList(Expression<Func<TEntity, bool>> predicate);


        Task<TEntity> AddAsync(TEntity instance);
        Task<bool> AddAsync(IEnumerable<TEntity> instances);

        Task<bool> DeleteAsync(TEntity instance, bool isLogicDelete = true);
        Task<bool> DeleteAsync(Expression<Func<TEntity, bool>> predicate, bool isLogicDelete = true, object updateUserId = default);

        Task<bool> UpdateAsync(TEntity instance);
        Task<bool> UpdateAsync(TEntity instance, Expression<Func<TEntity, bool>> predicate);
        Task<bool> UpdateAsync(IEnumerable<TEntity> instances);
        Task<bool> UpdateAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TEntity>> updateValues, object updateUserId = default);
        Task<bool> ExistAsync(Expression<Func<TEntity, bool>> predicate);

        Task<TEntity> GetAsync<TKey>(TKey id);
        Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate);
        Task<IEnumerable<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate);
    }

}
