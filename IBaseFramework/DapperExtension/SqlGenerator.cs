using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using IBaseFramework.Base;
using IBaseFramework.DapperExtension.ExpressionTree;

namespace IBaseFramework.DapperExtension
{
    /// <inheritdoc />
    public partial class SqlGenerator<TEntity> : ISqlGenerator<TEntity> where TEntity : EntityBase
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public SqlGenerator(SqlGeneratorConfig sqlGeneratorConfig)
        {
            Config = sqlGeneratorConfig;

            var entityType = typeof(TEntity);
            if (TableNameCache.ContainsKey(entityType))
            {
                TableName = TableNameCache[entityType];
            }
            else
            {
                var entityTypeInfo = entityType.GetTypeInfo();
                var tableAttribute = entityTypeInfo.GetCustomAttribute<TableAttribute>();

                TableName = Config.StartQuotationMark + (tableAttribute != null ? tableAttribute.Name : entityTypeInfo.Name) + Config.EndQuotationMark;
                TableNameCache.TryAdd(entityType, TableName);
            }

            PropertyInfo[] props;
            if (ReflectionPropertyCache.ContainsKey(entityType))
            {
                props = ReflectionPropertyCache[entityType];
            }
            else
            {
                props = entityType.GetProperties().Where(p => p.CanWrite && (p.PropertyType.IsValueType || p.PropertyType == typeof(string) || p.PropertyType == typeof(byte[]))).ToArray();
                ReflectionPropertyCache.TryAdd(entityType, props);
            }

            // Filter the non stored properties
            SqlProperties = props.Where(p => !p.GetCustomAttributes<IgnoreAttribute>().Any()).ToArray();

            // Filter key properties
            KeySqlProperties = props.Where(p => p.GetCustomAttributes<KeyAttribute>().Any()).ToArray();

            // Use identity as key pattern
            IdentitySqlProperty = props.FirstOrDefault(p => p.GetCustomAttributes<AutoIncrementKeyAttribute>().Any());
        }

        // ReSharper disable once StaticMemberInGenericType
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> ReflectionPropertyCache = new ConcurrentDictionary<Type, PropertyInfo[]>();
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ConcurrentDictionary<Type, string> TableNameCache = new ConcurrentDictionary<Type, string>();
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ConcurrentDictionary<string, string> SqlStringCache = new ConcurrentDictionary<string, string>();

        /// <inheritdoc />
        public bool IsIdentity => IdentitySqlProperty != null;
        public string TableName { get; protected set; }
        public PropertyInfo IdentitySqlProperty { get; protected set; }
        public PropertyInfo[] KeySqlProperties { get; protected set; }
        public PropertyInfo[] SqlProperties { get; protected set; }
        public SqlGeneratorConfig Config { get; protected set; }

        private readonly string _sqlCacheKey = $"{typeof(TEntity).FullName}";

        #region Build Sql

        #region GetCount

        /// <inheritdoc />
        public virtual SqlQuery GetCount(Expression<Func<TEntity, bool>> predicate)
        {
            var sqlQuery = new SqlQuery();
            sqlQuery.SqlBuilder.Append($"SELECT COUNT(1) FROM {TableName} ");

            AppendWherePredicateQuery(sqlQuery, predicate);

            return sqlQuery;
        }

        /// <inheritdoc />
        public virtual SqlQuery GetCount(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> distinctField)
        {
            var propertyName = GetPropertyName(distinctField);

            var query = new SqlQuery();
            query.SqlBuilder.Append($"SELECT COUNT(DISTINCT {GetPropertyName(propertyName)}) FROM {TableName} ");

            AppendWherePredicateQuery(query, predicate);

            return query;
        }
        #endregion

        #region GetSelect
        /// <inheritdoc />
        public virtual SqlQuery GetSelect(Expression<Func<TEntity, bool>> predicate, bool getFirst)
        {
            var sqlQuery = InitBuilderSelect(getFirst);

            AppendWherePredicateQuery(sqlQuery, predicate);

            if (getFirst && (Config.SqlProvider == SqlProvider.MySQL || Config.SqlProvider == SqlProvider.PostgreSQL))
                sqlQuery.SqlBuilder.Append(" LIMIT 1 ");

            return sqlQuery;
        }

        /// <inheritdoc />
        public SqlQuery GetSelectById(object id)
        {
            var keyPropertyInfo = IsIdentity ? new[] { IdentitySqlProperty } : KeySqlProperties;

            if (keyPropertyInfo.Length != 1)
                throw new NotSupportedException("This method support only 1 key");

            var keyProperty = keyPropertyInfo[0];

            var sqlQuery = new SqlQuery();

            var cacheKey = string.Concat(_sqlCacheKey, "_GetSelectById");
            if (SqlStringCache.ContainsKey(cacheKey))
            {
                sqlQuery.SqlBuilder.Append(SqlStringCache[cacheKey]);
            }
            else
            {
                sqlQuery = InitBuilderSelect(true);
                sqlQuery.SqlBuilder.Append($" WHERE {GetPropertyName(keyProperty.Name)} = @{keyProperty.Name} ");

                if (Config.SqlProvider == SqlProvider.MySQL || Config.SqlProvider == SqlProvider.PostgreSQL)
                    sqlQuery.SqlBuilder.Append(" LIMIT 1 ");

                SqlStringCache.TryAdd(cacheKey, sqlQuery.GetSql());
            }

            IDictionary<string, object> dictionary = new Dictionary<string, object>
            {
                { keyProperty.Name, id }
            };
            sqlQuery.SetParam(dictionary);

            return sqlQuery;
        }

        public SqlQuery GetIn(IEnumerable<dynamic> keys, Expression<Func<TEntity, object>> field = null)
        {
            var keyPropertyInfo = IsIdentity ? new[] { IdentitySqlProperty } : KeySqlProperties;

            if (field == null && keyPropertyInfo.Length != 1)
                throw new NotSupportedException("This method support only 1 key");

            string fieldName;
            if (field != null)
            {
                fieldName = GetPropertyName(field);
            }
            else
            {
                var keyProperty = keyPropertyInfo[0];
                fieldName = keyProperty.Name;
            }

            if (string.IsNullOrEmpty(fieldName))
                throw new NotSupportedException("fieldName can't be empty");

            var sqlQuery = new SqlQuery();

            var cacheKey = string.Concat(_sqlCacheKey, "_GetIn_", fieldName);
            if (SqlStringCache.ContainsKey(cacheKey))
            {
                sqlQuery.SqlBuilder.Append(SqlStringCache[cacheKey]);
            }
            else
            {
                sqlQuery = InitBuilderSelect(true);
                sqlQuery.SqlBuilder.Append($" WHERE {GetPropertyName(fieldName)} in @{fieldName} ");

                SqlStringCache.TryAdd(cacheKey, sqlQuery.GetSql());
            }

            IDictionary<string, object> dictionary = new Dictionary<string, object>
            {
                { fieldName, keys }
            };
            sqlQuery.SetParam(dictionary);

            return sqlQuery;
        }
        #endregion

        #region GetDelete
        /// <inheritdoc />
        public virtual SqlQuery GetDelete(TEntity entity, bool isLogicDelete = true)
        {
            var sqlQuery = new SqlQuery();

            var keyPropertyInfo = IsIdentity ? new[] { IdentitySqlProperty } : KeySqlProperties;

            var cacheKey = string.Concat(_sqlCacheKey, "_GetDelete_", isLogicDelete);
            if (SqlStringCache.ContainsKey(cacheKey))
            {
                sqlQuery.SqlBuilder.Append(SqlStringCache[cacheKey]);
            }
            else
            {
                var whereSql = $" WHERE {string.Join(" AND ", keyPropertyInfo.Select(p => $" {GetPropertyName(p.Name)} = @{p.Name} "))}";
                if (!isLogicDelete)
                {
                    sqlQuery.SqlBuilder.Append($"DELETE FROM {TableName} {whereSql}");
                }
                else
                {
                    sqlQuery.SqlBuilder.Append($"UPDATE {TableName} SET {Config.LogicDeleteSql} ");
                    foreach (var prop in Config.UpdateCommonProperty(entity.UpdateUserId))
                    {
                        sqlQuery.SqlBuilder.Append($", {GetPropertyName(prop.Key)} = @{prop.Key} ");
                    }
                    sqlQuery.SqlBuilder.Append(whereSql);
                }

                SqlStringCache.TryAdd(cacheKey, sqlQuery.GetSql());
            }

            var dic = new Dictionary<string, object>();
            foreach (var keySqlProperty in keyPropertyInfo)
            {
                dic.Add(keySqlProperty.Name, keySqlProperty.GetValue(entity));
            }

            if (isLogicDelete)
            {
                foreach (var prop in Config.UpdateCommonProperty(entity.UpdateUserId))
                {
                    dic.Add(prop.Key, prop.Value);
                }
            }

            sqlQuery.SetParam(dic);

            return sqlQuery;
        }

        /// <inheritdoc />
        public virtual SqlQuery GetDelete(Expression<Func<TEntity, bool>> predicate, object userId, bool isLogicDelete = true)
        {
            var sqlQuery = new SqlQuery();

            var cacheKey = string.Concat(_sqlCacheKey, "_GetDeleteByExpression_", isLogicDelete);
            if (SqlStringCache.ContainsKey(cacheKey))
            {
                sqlQuery.SqlBuilder.Append(SqlStringCache[cacheKey]);
            }
            else
            {
                if (!isLogicDelete)
                {
                    sqlQuery.SqlBuilder.Append($"DELETE FROM {TableName} ");
                }
                else
                {
                    sqlQuery.SqlBuilder.Append($"UPDATE {TableName} SET {Config.LogicDeleteSql} ");
                    foreach (var prop in Config.UpdateCommonProperty(userId))
                    {
                        sqlQuery.SqlBuilder.Append($", {GetPropertyName(prop.Key)} = @{prop.Key} ");
                    }
                }

                SqlStringCache.TryAdd(cacheKey, sqlQuery.GetSql());
            }

            if (isLogicDelete)
            {
                var parameters = new Dictionary<string, object>();
                foreach (var prop in Config.UpdateCommonProperty(userId))
                {
                    parameters.Add(prop.Key, prop.Value);
                }

                sqlQuery.SetParam(parameters);
            }

            AppendWherePredicateQuery(sqlQuery, predicate);

            return sqlQuery;
        }
        #endregion

        #region GetInsert
        /// <inheritdoc />
        public virtual SqlQuery GetInsert(TEntity entity)
        {
            var query = new SqlQuery(entity);

            var cacheKey = string.Concat(_sqlCacheKey, "_GetInsert");
            if (SqlStringCache.ContainsKey(cacheKey))
            {
                query.SqlBuilder.Append(SqlStringCache[cacheKey]);
            }
            else
            {
                var properties = (IsIdentity ? SqlProperties.Where(p => !p.Name.Equals(IdentitySqlProperty.Name, StringComparison.OrdinalIgnoreCase)) : SqlProperties).ToList();
                query.SqlBuilder.Append($"INSERT INTO {TableName} ({string.Join(", ", properties.Select(p => GetPropertyName(p.Name)))}) VALUES ({string.Join(", ", properties.Select(p => "@" + p.Name))})");

                if (IsIdentity)
                    switch (Config.SqlProvider)
                    {
                        case SqlProvider.MSSQL:
                            query.SqlBuilder.Append(" SELECT SCOPE_IDENTITY() AS " + IdentitySqlProperty.Name);
                            break;

                        case SqlProvider.MySQL:
                            query.SqlBuilder.Append("; SELECT CONVERT(LAST_INSERT_ID(), SIGNED INTEGER) AS " + IdentitySqlProperty.Name);
                            break;

                        case SqlProvider.PostgreSQL:
                            query.SqlBuilder.Append(" RETURNING " + IdentitySqlProperty.Name);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                SqlStringCache.TryAdd(cacheKey, query.GetSql());
            }

            return query;
        }

        /// <inheritdoc />
        public virtual SqlQuery GetBulkInsert(IEnumerable<TEntity> entities)
        {
            var entitiesArray = entities as TEntity[] ?? entities.ToArray();
            if (!entitiesArray.Any())
                throw new ArgumentException("collection is empty");

            var properties = (IsIdentity ? SqlProperties.Where(p => !p.Name.Equals(IdentitySqlProperty.Name, StringComparison.OrdinalIgnoreCase)) : SqlProperties).ToList();

            var query = new SqlQuery();

            var cacheKey = string.Concat(_sqlCacheKey, "_GetBulkInsert");
            if (SqlStringCache.ContainsKey(cacheKey))
            {
                query.SqlBuilder.Append(SqlStringCache[cacheKey]);
            }
            else
            {
                query.SqlBuilder.Append($"INSERT INTO {TableName} ({string.Join(", ", properties.Select(p => GetPropertyName(p.Name)))}) VALUES ");

                SqlStringCache.TryAdd(cacheKey, query.GetSql());
            }

            var values = new List<string>();
            var parameters = new Dictionary<string, object>();

            for (var i = 0; i < entitiesArray.Length; i++)
            {
                foreach (var property in properties)
                    parameters.Add(property.Name + i, property.GetValue(entitiesArray[i]));

                values.Add($"({string.Join(", ", properties.Select(p => $"@{p.Name}{i}"))})");
            }

            query.SqlBuilder.Append(" " + string.Join(",", values));
            query.SetParam(parameters);

            return query;
        }
        #endregion

        #region GetUpdate
        /// <inheritdoc />
        public virtual SqlQuery GetUpdate(TEntity entity)
        {
            var query = new SqlQuery(entity);

            var keyPropertyInfo = IsIdentity ? new[] { IdentitySqlProperty } : KeySqlProperties;

            var cacheKey = string.Concat(_sqlCacheKey, "_GetUpdate");
            if (SqlStringCache.ContainsKey(cacheKey))
            {
                query.SqlBuilder.Append(SqlStringCache[cacheKey]);
            }
            else
            {
                var properties = SqlProperties.Where(p => !keyPropertyInfo.Any(k => k.Name.Equals(p.Name, StringComparison.OrdinalIgnoreCase))).ToArray();
                if (!properties.Any())
                    throw new ArgumentException("Can't update without [Key]");

                query.SqlBuilder.Append($"UPDATE {TableName} SET {string.Join(", ", properties.Select(p => $"{GetPropertyName(p.Name)} = @{p.Name}"))} WHERE {string.Join(" AND ", keyPropertyInfo.Select(p => $"{GetPropertyName(p.Name)} = @{p.Name}"))}");

                SqlStringCache.TryAdd(cacheKey, query.GetSql());
            }

            return query;
        }

        /// <inheritdoc />
        public virtual SqlQuery GetUpdate(Expression<Func<TEntity, bool>> predicate, TEntity entity)
        {
            var query = new SqlQuery(entity);

            var keyPropertyInfo = IsIdentity ? new[] { IdentitySqlProperty } : KeySqlProperties;

            var cacheKey = string.Concat(_sqlCacheKey, "_GetUpdateByExpression");
            if (SqlStringCache.ContainsKey(cacheKey))
            {
                query.SqlBuilder.Append(SqlStringCache[cacheKey]);
            }
            else
            {
                var properties = SqlProperties.Where(p => !keyPropertyInfo.Any(k => k.Name.Equals(p.Name, StringComparison.OrdinalIgnoreCase)));
                query.SqlBuilder.Append($"UPDATE {TableName} SET {string.Join(", ", properties.Select(p => $"{GetPropertyName(p.Name)} = @{p.Name}"))} ");

                SqlStringCache.TryAdd(cacheKey, query.GetSql());
            }

            AppendWherePredicateQuery(query, predicate);

            return query;
        }

        public virtual SqlQuery GetUpdate(Expression<Func<TEntity, TEntity>> updateValues, Expression<Func<TEntity, bool>> predicate, object userId)
        {
            var parameters = new Dictionary<string, object>();

            if (updateValues.Body is MemberInitExpression member)
            {
                if (member.Bindings.Count > 0)
                {
                    foreach (var item in member.Bindings)
                    {
                        var memberAssignment = (MemberAssignment)item;

                        var express = memberAssignment.Expression;
                        if (express is UnaryExpression unaryExpression)
                            express = unaryExpression.Operand;

                        var node = ResolveQuery((dynamic)express);

                        parameters.Add(memberAssignment.Member.Name, node is ValueNode ? node.Value : node.Child.Value);
                    }
                }
            }

            //处理公共字段
            var commonValues = Config.UpdateCommonProperty(userId);
            foreach (var common in commonValues)
            {
                if (parameters.Keys.Contains(common.Key))
                {
                    continue;
                }
                parameters.Add(common.Key, common.Value);
            }

            var query = new SqlQuery();
            query.SqlBuilder.Append($"UPDATE {TableName} SET {string.Join(", ", parameters.Keys.Select(p => $"{GetPropertyName(p)} = @{p}"))} ");
            query.SetParam(parameters);

            AppendWherePredicateQuery(query, predicate);

            return query;
        }

        /// <inheritdoc />
        public virtual SqlQuery GetBulkUpdate(IEnumerable<TEntity> entities)
        {
            var entitiesArray = entities as TEntity[] ?? entities.ToArray();
            if (!entitiesArray.Any())
                throw new ArgumentException("collection is empty");


            var query = new SqlQuery();

            var keyPropertyInfo = IsIdentity ? new[] { IdentitySqlProperty } : KeySqlProperties;
            var properties = SqlProperties.Where(p => !keyPropertyInfo.Any(k => k.Name.Equals(p.Name, StringComparison.OrdinalIgnoreCase))).ToArray();

            var parameters = new Dictionary<string, object>();
            for (var i = 0; i < entitiesArray.Length; i++)
            {
                if (i > 0)
                    query.SqlBuilder.Append("; ");

                query.SqlBuilder.Append($"UPDATE {TableName} SET {string.Join(", ", properties.Select(p => $"{GetPropertyName(p.Name)} = @{p.Name + i}"))} WHERE {string.Join(" AND ", keyPropertyInfo.Select(p => $"{GetPropertyName(p.Name)} = @{p.Name + i}"))} ");

                foreach (var property in properties)
                    parameters.Add(property.Name + i, property.GetValue(entitiesArray[i]));

                foreach (var property in keyPropertyInfo)
                    parameters.Add(property.Name + i, property.GetValue(entitiesArray[i]));
            }

            query.SetParam(parameters);

            return query;
        }
        #endregion

        #endregion

        private void AppendWherePredicateQuery(SqlQuery sqlQuery, Expression<Func<TEntity, bool>> predicate)
        {
            if (predicate == null)
                return;

            IDictionary<string, object> dictionaryParams = new Dictionary<string, object>();

            var sqlWhere = ResolveQuery(predicate, ref dictionaryParams);
            if (!string.IsNullOrEmpty(sqlWhere))
            {
                sqlQuery.SqlBuilder.Append(" WHERE ");
                sqlQuery.SqlBuilder.Append(sqlWhere + " ");
            }
            sqlQuery.SetParam(dictionaryParams);
        }

        private SqlQuery InitBuilderSelect(bool firstOnly)
        {
            var query = new SqlQuery();

            var cacheKey = string.Concat(_sqlCacheKey, "_InitBuilderSelect_", firstOnly);
            if (SqlStringCache.ContainsKey(cacheKey))
            {
                query.SqlBuilder.Append(SqlStringCache[cacheKey]);
            }
            else
            {
                query.SqlBuilder.Append($"SELECT {(firstOnly && Config.SqlProvider == SqlProvider.MSSQL ? "TOP 1 " : "")} {string.Join(", ", SqlProperties.Select(p => GetPropertyName(p.Name)))} FROM {TableName} ");

                SqlStringCache.TryAdd(cacheKey, query.GetSql());
            }

            return query;
        }

        private string GetPropertyName<TSource, TField>(Expression<Func<TSource, TField>> field)
        {
            if (Equals(field, null))
                throw new ArgumentNullException(nameof(field), "field can't be null");

            MemberExpression expr;

            switch (field.Body)
            {
                case MemberExpression body:
                    expr = body;
                    break;
                case UnaryExpression expression:
                    expr = (MemberExpression)expression.Operand;
                    break;
                default:
                    throw new ArgumentException("Expression field isn't supported", nameof(field));
            }

            return expr.Member.Name;
        }

        private string GetPropertyName(string propName)
        {
            return Config.StartQuotationMark + propName + Config.EndQuotationMark;
        }
    }
}