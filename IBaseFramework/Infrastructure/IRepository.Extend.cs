using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Dapper;
using IBaseFramework.Base;
using IBaseFramework.ExpressionVisit;

namespace IBaseFramework.Infrastructure
{
    public static class RepositoryExtend
    {
        public static IQueryRepository<TSource> Where<TSource>(this IQueryRepository<TSource> repository, Expression<Func<TSource, bool>> predicate)
        {
            repository.QueryBuilder.ExpressionWhereList.Add(predicate);

            return repository;
        }

        public static IQueryRepository<TSource> Where<TSource>(this IQueryRepository<TSource> repository, string whereStr, Parameters param = null)
        {
            repository.QueryBuilder.WhereStrList.Add((whereStr, param));

            return repository;
        }

        public static IQueryRepository<TSource> WhereIf<TSource>(this IQueryRepository<TSource> repository, bool condition, Expression<Func<TSource, bool>> predicate)
        {
            if (condition)
            {
                repository.QueryBuilder.ExpressionWhereList.Add(predicate);
            }

            return repository;
        }

        public static IQueryRepository<TSource> WhereIf<TSource>(this IQueryRepository<TSource> repository, bool condition, Expression<Func<TSource, bool>> predicateByConditionTrue, Expression<Func<TSource, bool>> predicateByConditionFalse)
        {
            repository.QueryBuilder.ExpressionWhereList.Add(condition ? predicateByConditionTrue : predicateByConditionFalse);

            return repository;
        }

        public static IQueryRepository<TSource> OrderBy<TSource, TP>(this IQueryRepository<TSource> repository, Expression<Func<TSource, TP>> property)
        {
            repository.QueryBuilder.ExpressionOrderList.Add((property, true));

            return repository;
        }

        public static IQueryRepository<TSource> OrderByDescending<TSource, TP>(this IQueryRepository<TSource> repository, Expression<Func<TSource, TP>> property)
        {
            repository.QueryBuilder.ExpressionOrderList.Add((property, false));

            return repository;
        }

        public static IQueryRepository<TResult> Select<TSource, TResult>(this IQueryRepository<TSource> repository, Expression<Func<TSource, TResult>> properties)
        {
            repository.QueryBuilder.ExpressionSelectList.Add(properties);

            return new QueryRepository<TResult>(repository.QueryBuilder);
        }

        public static IQueryRepository<(TSource, T2)> Join<TSource, T2>(this IQueryRepository<TSource> repository, Expression<Func<TSource, T2, bool>> joinOnPredicate, JoinType joinType = JoinType.Join) where T2 : class
        {
            repository.QueryBuilder.ExpressionJoinList.Add((typeof(T2), joinOnPredicate, joinType));

            return new QueryRepository<(TSource, T2)>(repository.QueryBuilder);
        }

        public static TSource FirstOrDefault<TSource>(this IQueryRepository<TSource> repository, Expression<Func<TSource, bool>> predicate = null)
        {
            if (predicate != null)
                repository.QueryBuilder.ExpressionWhereList.Add(predicate);

            return ExecuteHelper.Execute(connection =>
            {
                var queryResult = repository.QueryBuilder.ParseSelect().ParseLimit(repository.QueryBuilder.ExpressionParser, 1, 1);

                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResult, connection);

                var result = connection.QueryFirstOrDefault<TSource>(queryResult.GetSql(), queryResult.Params);

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return result;
            });
        }
        public static Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryRepository<TSource> repository, Expression<Func<TSource, bool>> predicate = null)
        {
            if (predicate != null)
                repository.QueryBuilder.ExpressionWhereList.Add(predicate);

            return ExecuteHelper.Execute(connection =>
            {
                var queryResult = repository.QueryBuilder.ParseSelect().ParseLimit(repository.QueryBuilder.ExpressionParser, 1, 1);

                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResult, connection);

                var result = connection.QueryFirstOrDefaultAsync<TSource>(queryResult.GetSql(), queryResult.Params);

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return result;
            });
        }

        public static IEnumerable<TSource> ToList<TSource>(this IQueryRepository<TSource> repository)
        {
            return ExecuteHelper.Execute(connection =>
            {
                var queryResult = repository.QueryBuilder.ParseSelect();

                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResult, connection);

                var result = connection.Query<TSource>(queryResult.GetSql(), queryResult.Params);

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return result;
            });
        }
        public static Task<IEnumerable<TSource>> ToListAsync<TSource>(this IQueryRepository<TSource> repository)
        {
            return ExecuteHelper.Execute(connection =>
            {
                var queryResult = repository.QueryBuilder.ParseSelect();

                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResult, connection);

                var result = connection.QueryAsync<TSource>(queryResult.GetSql(), queryResult.Params);

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return result;
            });
        }

        public static PagedList<TSource> ToPageList<TSource>(this IQueryRepository<TSource> repository, int pageIndex, int pageSize)
        {
            return ExecuteHelper.Execute(connection =>
            {
                var queryResultForTotal = repository.QueryBuilder.ParseSelect().ParseTotalCount();
                var queryResult = repository.QueryBuilder.ParseSelect().ParseLimit(repository.QueryBuilder.ExpressionParser, pageIndex, pageSize);

                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResultForTotal, connection);
                diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResult, connection);

                var total = connection.QueryFirstOrDefault<int>(queryResultForTotal.GetSql(), queryResultForTotal.Params);
                var items = connection.Query<TSource>(queryResult.GetSql(), queryResult.Params);
                var result = new PagedList<TSource>() { Items = items, TotalCount = total, PageIndex = pageIndex, PageSize = pageSize };

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return result;
            });
        }

        public static Task<PagedList<TSource>> ToPageListAsync<TSource>(this IQueryRepository<TSource> repository, int pageIndex, int pageSize)
        {
            return ExecuteHelper.Execute(async connection =>
            {
                var queryResultForTotal = repository.QueryBuilder.ParseSelect().ParseTotalCount();
                var queryResult = repository.QueryBuilder.ParseSelect().ParseLimit(repository.QueryBuilder.ExpressionParser, pageIndex, pageSize);

                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResultForTotal, connection);
                diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResult, connection);

                var total = await connection.QueryFirstOrDefaultAsync<int>(queryResultForTotal.GetSql(), queryResultForTotal.Params);
                var items = await connection.QueryAsync<TSource>(queryResult.GetSql(), queryResult.Params);

                var result = new PagedList<TSource>() { Items = items, TotalCount = total, PageIndex = pageIndex, PageSize = pageSize };

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return result;
            });
        }

        public static SqlQuery ToSql<TSource>(this IQueryRepository<TSource> repository)
        {
            return repository.QueryBuilder.ParseSelect();
        }

        public static int Count<TSource>(this IQueryRepository<TSource> repository, Expression<Func<TSource, object>> distinctField = null)
        {
            if (distinctField != null)
                repository.QueryBuilder.ExpressionSelectList.Add(distinctField);

            return ExecuteHelper.Execute(connection =>
            {
                var queryResultForTotal = repository.QueryBuilder.ParseSelect(SqlFunction.Count);

                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResultForTotal, connection);

                var result = connection.QueryFirstOrDefault<int>(queryResultForTotal.GetSql(), queryResultForTotal.Params);

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return result;
            });
        }

        public static Task<int> CountAsync<TSource>(this IQueryRepository<TSource> repository, Expression<Func<TSource, object>> distinctField)
        {
            repository.QueryBuilder.ExpressionSelectList.Add(distinctField);

            return ExecuteHelper.Execute(connection =>
            {
                var queryResultForTotal = repository.QueryBuilder.ParseSelect(SqlFunction.Count);

                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResultForTotal, connection);

                var result = connection.QueryFirstOrDefaultAsync<int>(queryResultForTotal.GetSql(), queryResultForTotal.Params);

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return result;
            });
        }

        public static TP? Sum<TSource, TP>(this IQueryRepository<TSource> repository, Expression<Func<TSource, TP>> property) where TP : struct
        {
            repository.QueryBuilder.ExpressionSelectList.Add(property);

            return ExecuteHelper.Execute(connection =>
            {
                var queryResultForTotal = repository.QueryBuilder.ParseSelect(SqlFunction.Sum);

                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResultForTotal, connection);

                var result = connection.QueryFirstOrDefault<TP?>(queryResultForTotal.GetSql(), queryResultForTotal.Params);

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return result;
            });
        }

        public static Task<TP?> SumAsync<TSource, TP>(this IQueryRepository<TSource> repository, Expression<Func<TSource, TP>> property) where TP : struct
        {
            repository.QueryBuilder.ExpressionSelectList.Add(property);

            return ExecuteHelper.Execute(connection =>
            {
                var queryResultForTotal = repository.QueryBuilder.ParseSelect(SqlFunction.Sum);

                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResultForTotal, connection);

                var result = connection.QueryFirstOrDefaultAsync<TP?>(queryResultForTotal.GetSql(), queryResultForTotal.Params);

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return result;
            });
        }

        public static TP? Min<TSource, TP>(this IQueryRepository<TSource> repository, Expression<Func<TSource, TP>> property) where TP : struct
        {
            repository.QueryBuilder.ExpressionSelectList.Add(property);

            return ExecuteHelper.Execute(connection =>
            {
                var queryResultForTotal = repository.QueryBuilder.ParseSelect(SqlFunction.Min);

                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResultForTotal, connection);

                var result = connection.QueryFirstOrDefault<TP?>(queryResultForTotal.GetSql(), queryResultForTotal.Params);

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return result;
            });
        }

        public static Task<TP?> MinAsync<TSource, TP>(this IQueryRepository<TSource> repository, Expression<Func<TSource, TP>> property) where TP : struct
        {
            repository.QueryBuilder.ExpressionSelectList.Add(property);

            return ExecuteHelper.Execute(connection =>
            {
                var queryResultForTotal = repository.QueryBuilder.ParseSelect(SqlFunction.Min);

                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResultForTotal, connection);

                var result = connection.QueryFirstOrDefaultAsync<TP?>(queryResultForTotal.GetSql(), queryResultForTotal.Params);

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return result;
            });
        }

        public static TP? Max<TSource, TP>(this IQueryRepository<TSource> repository, Expression<Func<TSource, TP>> property) where TP : struct
        {
            repository.QueryBuilder.ExpressionSelectList.Add(property);

            return ExecuteHelper.Execute(connection =>
            {
                var queryResultForTotal = repository.QueryBuilder.ParseSelect(SqlFunction.Max);

                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResultForTotal, connection);

                var result = connection.QueryFirstOrDefault<TP?>(queryResultForTotal.GetSql(), queryResultForTotal.Params);

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return result;
            });
        }

        public static Task<TP?> MaxAsync<TSource, TP>(this IQueryRepository<TSource> repository, Expression<Func<TSource, TP>> property) where TP : struct
        {
            repository.QueryBuilder.ExpressionSelectList.Add(property);

            return ExecuteHelper.Execute(connection =>
            {
                var queryResultForTotal = repository.QueryBuilder.ParseSelect(SqlFunction.Max);

                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResultForTotal, connection);

                var result = connection.QueryFirstOrDefaultAsync<TP?>(queryResultForTotal.GetSql(), queryResultForTotal.Params);

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return result;
            });
        }

        public static TP? Avg<TSource, TP>(this IQueryRepository<TSource> repository, Expression<Func<TSource, TP>> property) where TP : struct
        {
            repository.QueryBuilder.ExpressionSelectList.Add(property);

            return ExecuteHelper.Execute(connection =>
            {
                var queryResultForTotal = repository.QueryBuilder.ParseSelect(SqlFunction.Avg);

                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResultForTotal, connection);

                var result = connection.QueryFirstOrDefault<TP?>(queryResultForTotal.GetSql(), queryResultForTotal.Params);

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return result;
            });
        }

        public static Task<TP?> AvgAsync<TSource, TP>(this IQueryRepository<TSource> repository, Expression<Func<TSource, TP>> property) where TP : struct
        {
            repository.QueryBuilder.ExpressionSelectList.Add(property);

            return ExecuteHelper.Execute(connection =>
            {
                var queryResultForTotal = repository.QueryBuilder.ParseSelect(SqlFunction.Avg);

                var diagnosticsMessage = Diagnostics.DiagnosticsHelper.ExecuteBefore(queryResultForTotal, connection);

                var result = connection.QueryFirstOrDefaultAsync<TP?>(queryResultForTotal.GetSql(), queryResultForTotal.Params);

                Diagnostics.DiagnosticsHelper.ExecuteAfter(diagnosticsMessage);

                return result;
            });
        }
    }
}