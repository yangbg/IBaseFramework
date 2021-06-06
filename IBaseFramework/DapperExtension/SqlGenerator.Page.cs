using System;
using System.Collections.Generic;
using IBaseFramework.Infrastructure;

namespace IBaseFramework.DapperExtension
{
    public class PageHelper
    {
        public static Tuple<SqlQuery, SqlQuery, int, int> Page(string sql, object param, int pageIndex, int pageSize)
        {
            ////查询字段
            //var rxColumns = new Regex(@"\A\s*SELECT\s+((?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|.)*?)(?<!,\s+)\bFROM\b", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);
            ////排序字段
            //var rxOrderBy = new Regex(@"\bORDER\s+BY\s+(?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|[\w\(\)\.])+(?:\s+(?:ASC|DESC))?(?:\s*,\s*(?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|[\w\(\)\.])+(?:\s+(?:ASC|DESC))?)*", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);
            ////去重字段
            //var rxDistinct = new Regex(@"\ADISTINCT\s", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);

            ////替换 select filed  => select count(*)
            //var m = rxColumns.Match(sql);
            //// 获取 count(*)
            //var g = m.Groups[1];

            ////查询field
            //var sqlSelectRemoved = sql.Substring(g.Index);

            //var count = rxDistinct.IsMatch(sqlSelectRemoved) ? m.Groups[1].ToString().Trim() : "1";
            //var sqlCount = $"{sql.Substring(0, g.Index)} COUNT({count}) {sql.Substring(g.Index + g.Length)}";
            ////查找 order by filed
            //m = rxOrderBy.Match(sqlCount);
            //if (m.Success)
            //{
            //    g = m.Groups[0];
            //    sqlCount = sqlCount.Substring(0, g.Index) + sqlCount.Substring(g.Index + g.Length);
            //}

            //if (sql.ToLower().Contains("group by") || sql.ToLower().Contains("DISTINCT(".ToLower()))
            //{
            //    sqlCount = $@"SELECT COUNT(1) FROM 
            //                    (
            //                    {sqlCount}
            //                    )tempCountTable";
            //}
            
            var sqlCount = $@"SELECT COUNT(1) FROM 
                                (
                                {sql}
                                )tempCountTable";

            var countSqlQuery = new SqlQuery(param);
            countSqlQuery.SqlBuilder.Append(sqlCount);

            //分页查询语句
            string sqlPage;
            var dataSqlQuery = new SqlQuery(param);
            var provider = DapperContext.DatabaseConfiguration.SqlProvider;
            switch (provider)
            {
                case SqlProvider.MSSQL:
                    sqlPage = $@"SELECT TOP @pageSize *
                                    FROM( SELECT ROW_NUMBER() OVER (ORDER BY id) AS RowNumber,* FROM ({sql})temp )as A 
                                    WHERE RowNumber > @pageSize*(@pageIndex-1)";

                    dataSqlQuery.SetParam(new Dictionary<string, object>
                    {
                        {"@pageIndex", pageIndex },
                        {"@pageSize", pageSize }
                    });
                    break;
                case SqlProvider.MySQL:
                case SqlProvider.PostgreSQL:
                    var pageLimit = " limit @limit offset @offset "; //分页关键字
                    sqlPage = sql + pageLimit;

                    dataSqlQuery.SetParam(new Dictionary<string, object>
                    {
                        {"offset", (pageIndex - 1) * pageSize },
                        {"limit", pageSize }
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(provider), provider, null);
            }

            dataSqlQuery.SqlBuilder.Append(sqlPage);

            return new Tuple<SqlQuery, SqlQuery, int, int>(countSqlQuery, dataSqlQuery, pageIndex, pageSize);
        }
    }
}