using IBaseFramework.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IBaseFramework.ExpressionVisit
{
    /// <summary>
    ///     A object with the generated sql and dynamic params.
    /// </summary>
    public class SqlQuery
    {
        /// <summary>
        ///     Initializes a new instance of the class.
        /// </summary>
        public SqlQuery()
        {
            SqlBuilder = new StringBuilder();
            Params = new Dictionary<string, object>();
        }

        /// <summary>
        ///     SqlBuilder
        /// </summary>
        public StringBuilder SqlBuilder { get; set; }

        /// <summary>
        ///     Gets the param
        /// </summary>
        public Dictionary<string, object> Params { get; set; }

        /// <summary>
        ///     Gets the SQL.
        /// </summary>
        internal string GetSql()
        {
            return SqlBuilder.ToString().Trim();
        }

        /// <summary>
        ///     Set alternative param
        /// </summary>
        /// <param name="param">The param.</param>
        internal void SetParam(object param)
        {
            if (param == null)
                return;

            if (param is Dictionary<string, object> paraDic)
            {
                foreach (var prop in paraDic)
                {
                    if (Params.ContainsKey(prop.Key))
                    {
                        continue;
                    }
                    Params.Add(prop.Key, prop.Value);
                }
            }
            else if (param is Parameters parameters)
            {
                foreach (var item in parameters.ParameterNames)
                {
                    if (Params.ContainsKey(item))
                    {
                        continue;
                    }
                    Params.Add(item, parameters.Get<object>(item));
                }
            }
            else
            {
                var props = param.GetType().GetProperties();
                if (!props.Any())
                    return;

                foreach (var prop in props)
                {
                    if (Params.ContainsKey(prop.Name))
                    {
                        continue;
                    }
                    Params.Add(prop.Name, prop.GetValue(param));
                }
            }
        }

        internal void SetParam(string key, object value)
        {
            if (Params.ContainsKey(key))
            {
                return;
            }
            Params.Add(key, value);
        }
    }

    public static class SqlQueryExtend
    {
        public static SqlQuery ParseLimit(this SqlQuery queryResult, ExpressionParser expressionParser, int pageIndex, int pageSize)
        {
            if (pageIndex == 0 || expressionParser == null)
                return queryResult;

            var sql = expressionParser.LimitFormatSql
                .Replace("{SkipCount}", ((pageIndex - 1) * pageSize).ToString())
                .Replace("{RowsPerPage}", pageSize.ToString());

            queryResult.SqlBuilder.Append(" " + sql);

            return queryResult;
        }

        public static SqlQuery ParseTotalCount(this SqlQuery queryResult)
        {
            var sqlCount = $@"SELECT COUNT(1) FROM 
                                (
                                {queryResult.SqlBuilder}
                                )tempCountTable";

            return new SqlQuery() { SqlBuilder = new StringBuilder(sqlCount), Params = queryResult.Params };
        }
    }
}