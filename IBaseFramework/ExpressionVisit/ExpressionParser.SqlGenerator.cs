using IBaseFramework.Infrastructure;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace IBaseFramework.ExpressionVisit
{
    public partial class ExpressionParser
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ConcurrentDictionary<string, string> SqlStringCache = new ConcurrentDictionary<string, string>();

        public string IdentityQueryFormatSql
        {
            get
            {
                return _sqlProvider switch
                {
                    SqlProvider.SQLServer => "SELECT SCOPE_IDENTITY() AS [id]",
                    SqlProvider.MySQL => "SELECT LAST_INSERT_ID() AS id",
                    SqlProvider.PostgreSQL => "SELECT LASTVAL() AS id",
                    _ => "SELECT LAST_INSERT_ROWID() AS id",
                };
            }
        }

        public string LimitFormatSql
        {
            get
            {
                return _sqlProvider switch
                {
                    SqlProvider.SQLServer => "OFFSET {SkipCount} ROWS FETCH FIRST {RowsPerPage} ROWS ONLY ",
                    SqlProvider.MySQL => "LIMIT {RowsPerPage} OFFSET {SkipCount} ",
                    SqlProvider.PostgreSQL => "LIMIT {RowsPerPage} OFFSET {SkipCount} ",
                    _ => "LIMIT {RowsPerPage} OFFSET {SkipCount} ",
                };
            }
        }

        public string GetTableName(Type type)
        {
            if (TableNameCache.ContainsKey(type))
            {
                return TableNameCache[type];
            }
            else
            {
                var entityTypeInfo = type.GetTypeInfo();
                var tableAttribute = type.GetTypeInfo().GetCustomAttribute<TableAttribute>();

                var tableName = tableAttribute?.Name ?? entityTypeInfo.Name;

                TableNameCache.TryAdd(type, tableName);

                return tableName;
            }
        }

        public IEnumerable<PropertyInfo> GetAllColumnsList(Type type)
        {
            if (ReflectionPropertyCache.ContainsKey(type))
            {
                return ReflectionPropertyCache[type];
            }
            else
            {
                var props = type.GetProperties().Where(p => p.CanWrite && (p.PropertyType.IsValueType || p.PropertyType == typeof(string) || p.PropertyType == typeof(byte[]))).ToArray();
                ReflectionPropertyCache.TryAdd(type, props);

                return props;
            }
        }

        public IEnumerable<PropertyInfo> GetPrimaryKeysColumnList(Type type)
        {
            if (KeysPropertyCache.ContainsKey(type))
            {
                return KeysPropertyCache[type];
            }
            else
            {
                var keys = GetAllColumnsList(type).Where(p => p.GetCustomAttributes<KeyAttribute>().Any()).ToArray();
                KeysPropertyCache.TryAdd(type, keys);

                return keys;
            }
        }

        public PropertyInfo GetIdentityKeyColumn(Type type)
        {
            if (IdentityKeyPropertyCache.ContainsKey(type))
            {
                return IdentityKeyPropertyCache[type];
            }
            else
            {
                var identityKey = GetAllColumnsList(type).FirstOrDefault(p => p.GetCustomAttributes<AutoIncrementKeyAttribute>().Any());
                IdentityKeyPropertyCache.TryAdd(type, identityKey);

                return identityKey;
            }
        }


        public IEnumerable<PropertyInfo> GetSelectColumnList(Type type)
        {
            return GetAllColumnsList(type).Where(p => !p.GetCustomAttributes<IgnoreAttribute>().Any());
        }

        public IEnumerable<PropertyInfo> GetUpdateColumnList(Type type)
        {
            return GetAllColumnsList(type).Where(p => !p.GetCustomAttributes<IgnoreAttribute>().Any() && !p.GetCustomAttributes<KeyAttribute>().Any() && !p.GetCustomAttributes<AutoIncrementKeyAttribute>().Any());
        }

        public IEnumerable<PropertyInfo> GetInsertColumnList(Type type)
        {
            return GetAllColumnsList(type).Where(p => !p.GetCustomAttributes<IgnoreAttribute>().Any() && !p.GetCustomAttributes<AutoIncrementKeyAttribute>().Any());
        }

        public PropertyInfo GetDeleteColumn(Type type)
        {
            return GetAllColumnsList(type).FirstOrDefault(p => p.GetCustomAttributes<DeleteAttribute>().Any());
        }

        public Dictionary<string, object> ExpressionToDic(Expression expression)
        {
            if (expression is LambdaExpression lambdaExpression)
            {
                if (lambdaExpression.Body is MemberInitExpression memberInitExpression)
                {
                    if (memberInitExpression.Bindings.Count > 0)
                    {
                        var dic = new Dictionary<string, object>();

                        foreach (var item in memberInitExpression.Bindings)
                        {
                            if (item is MemberAssignment memberAssignment)
                            {
                                dic.Add(memberAssignment.Member.Name, QueryHelper.GetValueFromExpression(memberAssignment.Expression));
                            }
                        }

                        return dic;
                    }
                }
            }

            return null;
        }


        #region Insert

        public SqlQuery GetInsert<TEntity>(TEntity entity)
        {
            var queryResult = new SqlQuery();

            var _type = entity.GetType();
            var _tableName = GetTableName(_type);

            var cacheKey = string.Concat(_tableName, "_Insert");
            if (SqlStringCache.ContainsKey(cacheKey))
            {
                queryResult.SqlBuilder.Append(SqlStringCache[cacheKey]);
            }
            else
            {
                var insertProperties = GetInsertColumnList(_type);

                queryResult.SqlBuilder.Append($"INSERT INTO {_tableName} ({string.Join(", ", insertProperties.Select(p => Encapsulation(p.Name)))}) VALUES ({string.Join(", ", insertProperties.Select(p => "@" + p.Name))})");

                if (GetIdentityKeyColumn(_type) != null)
                    queryResult.SqlBuilder.Append("; " + IdentityQueryFormatSql);

                SqlStringCache.TryAdd(cacheKey, queryResult.SqlBuilder.ToString());
            }

            return queryResult;
        }

        public SqlQuery GetBulkInsert<TEntity>(IEnumerable<TEntity> entities)
        {
            var entitiesArray = entities as TEntity[] ?? entities.ToArray();
            if (!entitiesArray.Any())
                throw new ArgumentException("collection is empty");

            var queryResult = new SqlQuery();
            var _type = typeof(TEntity);
            var _tableName = GetTableName(_type);

            var insertProperties = GetInsertColumnList(_type);

            var cacheKey = string.Concat(_tableName, "_BulkInsert");
            if (SqlStringCache.ContainsKey(cacheKey))
            {
                queryResult.SqlBuilder.Append(SqlStringCache[cacheKey]);
            }
            else
            {
                queryResult.SqlBuilder.Append($"INSERT INTO {_tableName} ({string.Join(", ", insertProperties.Select(p => Encapsulation(p.Name)))}) VALUES ");

                SqlStringCache.TryAdd(cacheKey, queryResult.SqlBuilder.ToString());
            }

            var values = new List<string>();

            for (var i = 0; i < entitiesArray.Length; i++)
            {
                foreach (var property in insertProperties)
                    queryResult.SetParam(property.Name + i, property.GetValue(entitiesArray[i]));

                values.Add($"({string.Join(", ", insertProperties.Select(p => $"@{p.Name}{i}"))})");
            }

            queryResult.SqlBuilder.Append(" " + string.Join(",", values));

            return queryResult;
        }

        #endregion

        #region Delete

        public SqlQuery GetDelete<TEntity>(TEntity entity, bool isLogicDelete = true, Dictionary<string, object> updateCommonProperty = default)
        {
            var queryResult = new SqlQuery();

            var _type = typeof(TEntity);
            var _tableName = GetTableName(_type);

            var indetityKey = GetIdentityKeyColumn(_type);
            var keysProperties = GetPrimaryKeysColumnList(_type);

            var keyPropertyInfo = indetityKey != null ? new[] { indetityKey } : keysProperties;

            var cacheKey = string.Concat(_tableName, "_Delete_", isLogicDelete);
            if (SqlStringCache.ContainsKey(cacheKey))
            {
                queryResult.SqlBuilder.Append(SqlStringCache[cacheKey]);
            }
            else
            {
                var whereSql = $" WHERE {string.Join(" AND ", keyPropertyInfo.Select(p => $" {Encapsulation(p.Name)} = @{p.Name} "))}";
                if (!isLogicDelete)
                {
                    queryResult.SqlBuilder.Append($"DELETE FROM {_tableName} {whereSql}");
                }
                else
                {
                    var logicDeleteSql = " IsDeleted=1 ";

                    var deletePropertyInfo = GetDeleteColumn(_type);
                    if (deletePropertyInfo != null)
                    {
                        logicDeleteSql = $" {Encapsulation(deletePropertyInfo.Name)}=1 ";
                    }

                    queryResult.SqlBuilder.Append($"UPDATE {_tableName} SET {logicDeleteSql} ");
                    if (updateCommonProperty != null && updateCommonProperty.Any())
                    {
                        foreach (var prop in updateCommonProperty)
                        {
                            queryResult.SqlBuilder.Append($", {Encapsulation(prop.Key)} = @{prop.Key} ");
                        }
                    }
                    queryResult.SqlBuilder.Append(whereSql);
                }

                SqlStringCache.TryAdd(cacheKey, queryResult.SqlBuilder.ToString());
            }

            foreach (var keySqlProperty in keyPropertyInfo)
            {
                queryResult.SetParam(keySqlProperty.Name, keySqlProperty.GetValue(entity));
            }

            if (isLogicDelete)
            {
                if (updateCommonProperty != null && updateCommonProperty.Any())
                {
                    foreach (var prop in updateCommonProperty)
                    {
                        queryResult.SetParam(prop.Key, prop.Value);
                    }
                }
            }

            return queryResult;
        }

        public SqlQuery GetDelete<TEntity>(Expression<Func<TEntity, bool>> predicate, bool isLogicDelete = true, Dictionary<string, object> updateCommonProperty = default)
        {
            var queryResult = new SqlQuery();

            var _type = typeof(TEntity);
            var _tableName = GetTableName(_type);

            var cacheKey = string.Concat(_tableName, "_DeleteByExpression_", isLogicDelete);
            if (SqlStringCache.ContainsKey(cacheKey))
            {
                queryResult.SqlBuilder.Append(SqlStringCache[cacheKey]);
            }
            else
            {
                if (!isLogicDelete)
                {
                    queryResult.SqlBuilder.Append($"DELETE FROM {_tableName} ");
                }
                else
                {
                    var logicDeleteSql = " IsDeleted=1 ";

                    var deletePropertyInfo = GetDeleteColumn(_type);
                    if (deletePropertyInfo != null)
                    {
                        logicDeleteSql = $" {Encapsulation(deletePropertyInfo.Name)}=1 ";
                    }

                    queryResult.SqlBuilder.Append($"UPDATE {_tableName} SET {logicDeleteSql} ");
                    if (updateCommonProperty != null && updateCommonProperty.Any())
                    {
                        foreach (var prop in updateCommonProperty)
                        {
                            queryResult.SqlBuilder.Append($", {Encapsulation(prop.Key)} = @{prop.Key} ");
                        }
                    }
                }

                SqlStringCache.TryAdd(cacheKey, queryResult.SqlBuilder.ToString());
            }

            this.QueryParser(predicate, queryResult);

            if (isLogicDelete)
            {
                if (updateCommonProperty != null && updateCommonProperty.Any())
                {
                    foreach (var prop in updateCommonProperty)
                    {
                        queryResult.SetParam(prop.Key, prop.Value);
                    }
                }
            }

            return queryResult;
        }

        #endregion

        #region Update

        public SqlQuery GetUpdate<TEntity>(TEntity entity)
        {
            var queryResult = new SqlQuery();

            var _type = entity.GetType();
            var _tableName = GetTableName(_type);

            var indetityKey = GetIdentityKeyColumn(_type);
            var keysProperties = GetPrimaryKeysColumnList(_type);

            var keyPropertyInfo = indetityKey != null ? new[] { indetityKey } : keysProperties;

            var cacheKey = string.Concat(_tableName, "_Update");
            if (SqlStringCache.ContainsKey(cacheKey))
            {
                queryResult.SqlBuilder.Append(SqlStringCache[cacheKey]);
            }
            else
            {
                var properties = GetUpdateColumnList(_type);
                if (!properties.Any())
                    throw new ArgumentException("Can't update without [Key]");

                queryResult.SqlBuilder.Append($"UPDATE {_tableName} SET {string.Join(", ", properties.Select(p => $"{Encapsulation(p.Name)} = @{p.Name}"))} WHERE {string.Join(" AND ", keyPropertyInfo.Select(p => $"{Encapsulation(p.Name)} = @{p.Name}"))}");

                SqlStringCache.TryAdd(cacheKey, queryResult.SqlBuilder.ToString());
            }

            return queryResult;
        }

        public SqlQuery GetUpdate<TEntity>(Expression<Func<TEntity, bool>> predicate, TEntity entity)
        {
            var queryResult = new SqlQuery();

            var _type = entity.GetType();
            var _tableName = GetTableName(_type);

            var cacheKey = string.Concat(_tableName, "_UpdateByExpression");
            if (SqlStringCache.ContainsKey(cacheKey))
            {
                queryResult.SqlBuilder.Append(SqlStringCache[cacheKey]);
            }
            else
            {
                var properties = GetUpdateColumnList(_type);
                if (!properties.Any())
                    throw new ArgumentException("Can't update without [Key]");

                queryResult.SqlBuilder.Append($"UPDATE {_tableName} SET {string.Join(", ", properties.Select(p => $"{Encapsulation(p.Name)} = @{p.Name}"))} ");

                SqlStringCache.TryAdd(cacheKey, queryResult.SqlBuilder.ToString());
            }

            queryResult.SetParam(entity);
            this.QueryParser(predicate, queryResult);

            return queryResult;
        }

        public SqlQuery GetUpdate<TEntity>(Expression<Func<TEntity, TEntity>> updateValues, Expression<Func<TEntity, bool>> predicate, Dictionary<string, object> updateCommonProperty = default)
        {
            var updateDic = ExpressionToDic(updateValues);
            if (updateDic == null || !updateDic.Any())
                throw new ArgumentException("Can't update with Empty");

            var queryResult = new SqlQuery();

            var _type = typeof(TEntity);
            var _tableName = GetTableName(_type);

            //处理公共字段
            if (updateCommonProperty != null && updateCommonProperty.Any())
            {
                foreach (var common in updateCommonProperty)
                {
                    updateDic.Add(common.Key, common.Value);
                }
            }

            queryResult.SqlBuilder.Append($"UPDATE {_tableName} SET {string.Join(", ", updateDic.Keys.Select(p => $"{Encapsulation(p)} = @{p}"))} ");

            foreach (var item in updateDic)
            {
                queryResult.SetParam(item.Key, item.Value);
            }

            this.QueryParser(predicate, queryResult);

            return queryResult;
        }

        public SqlQuery GetBulkUpdate<TEntity>(IEnumerable<TEntity> entities)
        {
            var entitiesArray = entities as TEntity[] ?? entities.ToArray();
            if (!entitiesArray.Any())
                throw new ArgumentException("collection is empty");

            var _type = typeof(TEntity);
            var _tableName = GetTableName(_type);

            var indetityKey = GetIdentityKeyColumn(_type);
            var keysProperties = GetPrimaryKeysColumnList(_type);

            var keyPropertyInfo = indetityKey != null ? new[] { indetityKey } : keysProperties;
            var properties = GetUpdateColumnList(_type);
            if (!properties.Any())
                throw new ArgumentException("Can't update without [Key]");

            var queryResult = new SqlQuery();

            for (var i = 0; i < entitiesArray.Length; i++)
            {
                if (i > 0)
                    queryResult.SqlBuilder.Append("; ");

                queryResult.SqlBuilder.Append($"UPDATE {_tableName} SET {string.Join(", ", properties.Select(p => $"{Encapsulation(p.Name)} = @{p.Name + i}"))} WHERE {string.Join(" AND ", keyPropertyInfo.Select(p => $"{Encapsulation(p.Name)} = @{p.Name + i}"))} ");

                foreach (var property in properties)
                    queryResult.SetParam(property.Name + i, property.GetValue(entitiesArray[i]));

                foreach (var property in keyPropertyInfo)
                    queryResult.SetParam(property.Name + i, property.GetValue(entitiesArray[i]));
            }

            return queryResult;
        }
        #endregion

        #region GetExist

        public SqlQuery GetExist<TEntity>(Expression<Func<TEntity, bool>> predicate)
        {
            var queryResult = new SqlQuery();

            var _type = typeof(TEntity);
            var _tableName = GetTableName(_type);

            queryResult.SqlBuilder.Append($"SELECT {(_sqlProvider == SqlProvider.SQLServer ? "TOP 1 " : "")} 1 FROM {_tableName} ");

            this.QueryParser(predicate, queryResult);

            if (_sqlProvider == SqlProvider.MySQL || _sqlProvider == SqlProvider.PostgreSQL)
                queryResult.SqlBuilder.Append(" LIMIT 1 ");

            return queryResult;
        }
        #endregion

        #region GetSelect

        public SqlQuery GetSelect<TEntity>(Expression<Func<TEntity, bool>> predicate, bool getFirst = false)
        {
            var queryResult = new SqlQuery();

            var _type = typeof(TEntity);
            var _tableName = GetTableName(_type);

            var cacheKey = string.Concat(_tableName, "_Select", getFirst);
            if (SqlStringCache.ContainsKey(cacheKey))
            {
                queryResult.SqlBuilder.Append(SqlStringCache[cacheKey]);
            }
            else
            {
                queryResult.SqlBuilder.Append($"SELECT {(getFirst && _sqlProvider == SqlProvider.SQLServer ? "TOP 1 " : "")} {string.Join(", ", GetSelectColumnList(_type).Select(u => Encapsulation(u.Name, _tableName)))} FROM {_tableName} ");

                SqlStringCache.TryAdd(cacheKey, queryResult.SqlBuilder.ToString());
            }

            this.QueryParser(predicate, queryResult);

            if (getFirst && (_sqlProvider == SqlProvider.MySQL || _sqlProvider == SqlProvider.PostgreSQL))
                queryResult.SqlBuilder.Append(" LIMIT 1 ");

            return queryResult;
        }

        public SqlQuery GetSelectById<TEntity, TKey>(TKey id)
        {
            var _type = typeof(TEntity);
            var _tableName = GetTableName(_type);

            var indetityKey = GetIdentityKeyColumn(_type);
            var keysProperties = GetPrimaryKeysColumnList(_type);

            var keyPropertyInfo = indetityKey != null ? new[] { indetityKey } : keysProperties;
            if (keyPropertyInfo == null || keyPropertyInfo.Count() != 1)
                throw new NotSupportedException("This method support only 1 key");

            var queryResult = new SqlQuery();

            var keyProperty = keyPropertyInfo.First();

            var cacheKey = string.Concat(_tableName, "_SelectById");
            if (SqlStringCache.ContainsKey(cacheKey))
            {
                queryResult.SqlBuilder.Append(SqlStringCache[cacheKey]);
            }
            else
            {
                queryResult.SqlBuilder.Append($"SELECT {(_sqlProvider == SqlProvider.SQLServer ? "TOP 1 " : "")} {string.Join(", ", GetSelectColumnList(_type).Select(u => Encapsulation(u.Name, _tableName)))} FROM {_tableName} ");

                SqlStringCache.TryAdd(cacheKey, queryResult.SqlBuilder.ToString());
            }

            queryResult.SqlBuilder.Append($" WHERE {Encapsulation(keyProperty.Name)} = @{keyProperty.Name} ");

            if (_sqlProvider == SqlProvider.MySQL || _sqlProvider == SqlProvider.PostgreSQL)
                queryResult.SqlBuilder.Append(" LIMIT 1 ");

            queryResult.SetParam(keyProperty.Name, id);

            return queryResult;
        }

        #endregion
    }
}
