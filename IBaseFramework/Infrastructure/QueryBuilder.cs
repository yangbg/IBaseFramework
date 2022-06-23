using IBaseFramework.ExpressionVisit;
using IBaseFramework.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace IBaseFramework.Infrastructure
{
    public interface IQueryBuilder
    {
        Type ThisType { get; set; }
        ExpressionParser ExpressionParser { get; set; }
        List<Expression> ExpressionWhereList { get; set; }
        List<Expression> ExpressionSelectList { get; set; }
        List<(Type type, Expression property, JoinType joinType)> ExpressionJoinList { get; set; }
        List<(Expression property, bool ascendant)> ExpressionOrderList { get; set; }
        List<(string whereStr, object param)> WhereStrList { get; set; }
        void InitQuery();

        SqlQuery ParseSelect(SqlFunction sqlFunction = SqlFunction.None);
    }

    public class QueryBuilder : IQueryBuilder
    {
        public QueryBuilder(Type type)
        {
            this.ThisType = type;

            this.ExpressionJoinList = new List<(Type type, Expression property, JoinType joinType)>();
            this.ExpressionSelectList = new List<Expression>();
            this.WhereStrList = new List<(string whereStr, object param)>();
            this.ExpressionWhereList = new List<Expression>();
            this.ExpressionOrderList = new List<(Expression property, bool ascendant)>();
        }

        public QueryBuilder(IQueryBuilder queryBuilder)
        {
            if (queryBuilder != null)
            {
                this.ExpressionParser = queryBuilder.ExpressionParser;
                this.ExpressionJoinList = queryBuilder.ExpressionJoinList;
                this.ExpressionSelectList = queryBuilder.ExpressionSelectList;
                this.WhereStrList = queryBuilder.WhereStrList;
                this.ExpressionWhereList = queryBuilder.ExpressionWhereList;
                this.ExpressionOrderList = queryBuilder.ExpressionOrderList;
                this.ThisType = queryBuilder.ThisType;
            }
        }

        public void InitQuery()
        {
            this.ExpressionJoinList = new List<(Type type, Expression property, JoinType joinType)>();
            this.ExpressionSelectList = new List<Expression>();
            this.WhereStrList = new List<(string whereStr, object param)>();
            this.ExpressionWhereList = new List<Expression>();
            this.ExpressionOrderList = new List<(Expression property, bool ascendant)>();
        }

        protected ExpressionParser _expressionParser = null;
        public ExpressionParser ExpressionParser
        {
            get
            {
                if (_expressionParser != null)
                    return _expressionParser;

                _expressionParser = new ExpressionParser(DapperContext.DatabaseConfiguration.SqlProvider);

                return _expressionParser;
            }
            set
            {
                _expressionParser = value;
            }
        }

        public Type ThisType { get; set; }
        public List<Expression> ExpressionWhereList { get; set; }
        public List<Expression> ExpressionSelectList { get; set; }
        public List<(Type type, Expression property, JoinType joinType)> ExpressionJoinList { get; set; }
        public List<(Expression property, bool ascendant)> ExpressionOrderList { get; set; }
        public List<(string whereStr, object param)> WhereStrList { get; set; }

        public SqlQuery ParseSelect(SqlFunction sqlFunction = SqlFunction.None)
        {
            var queryResult = new SqlQuery();

            //SELECT
            ParseSelect(queryResult, sqlFunction);

            //FROM
            ParseFrom(queryResult);

            //JOIN
            ParseJoin(queryResult);

            // WHERE
            ParseWhere(queryResult);

            // ORDER BY
            ParseOrderBy(queryResult);

            // LIMIT
            //ParseLimit(queryResult);

            return queryResult;
        }

        private void ParseSelect(SqlQuery queryResult, SqlFunction sqlFunction = SqlFunction.None)
        {
            switch (sqlFunction)
            {
                case SqlFunction.None:
                    {
                        if (ExpressionSelectList == null || !ExpressionSelectList.Any())
                        {
                            var tableName = this.ExpressionParser.GetTableName(ThisType);

                            var properties = this.ExpressionParser.GetSelectColumnList(ThisType).Select(u => ExpressionParser.Encapsulation(u.Name, tableName));
                            if (properties != null && properties.Any())
                            {
                                queryResult.SqlBuilder.Append($" SELECT {string.Join(", ", properties)} ");
                            }
                        }
                        else
                        {
                            var selectAdded = false;
                            foreach (var expression in ExpressionSelectList)
                            {
                                queryResult.SqlBuilder.Append(selectAdded ? "\n\t, " : " SELECT ");
                                selectAdded = true;

                                this.ExpressionParser.SelectParser(expression, queryResult);
                            }
                        }
                    }
                    break;
                case SqlFunction.Count:
                    {
                        if (ExpressionSelectList != null && ExpressionSelectList.Any())
                        {
                            queryResult.SqlBuilder.Append(" SELECT COUNT(DISTINCT ");
                            this.ExpressionParser.SelectParser(ExpressionSelectList.First(), queryResult);
                            queryResult.SqlBuilder.Append(") ");
                        }
                        else
                        {
                            queryResult.SqlBuilder.Append(" SELECT COUNT(1) ");
                        }
                    }
                    break;
                case SqlFunction.Max:
                    {
                        queryResult.SqlBuilder.Append(" SELECT MAX(");
                        this.ExpressionParser.SelectParser(ExpressionSelectList.First(), queryResult);
                        queryResult.SqlBuilder.Append(") ");
                    }
                    break;
                case SqlFunction.Min:
                    {
                        queryResult.SqlBuilder.Append(" SELECT MIN(");
                        this.ExpressionParser.SelectParser(ExpressionSelectList.First(), queryResult);
                        queryResult.SqlBuilder.Append(") ");
                    }
                    break;
                case SqlFunction.Avg:
                    {
                        queryResult.SqlBuilder.Append(" SELECT AVG(");
                        this.ExpressionParser.SelectParser(ExpressionSelectList.First(), queryResult);
                        queryResult.SqlBuilder.Append(") ");
                    }
                    break;
                case SqlFunction.Sum:
                    {
                        queryResult.SqlBuilder.Append(" SELECT SUM(");
                        this.ExpressionParser.SelectParser(ExpressionSelectList.First(), queryResult);
                        queryResult.SqlBuilder.Append(") ");
                    }
                    break;
            }
        }

        private void ParseFrom(SqlQuery queryResult)
        {
            var tableName = ExpressionParser.GetTableName(ThisType);
            queryResult.SqlBuilder.Append($" \nFROM {tableName} ");
        }

        private void ParseJoin(SqlQuery queryResult)
        {
            if (ExpressionJoinList == null)
                return;

            foreach (var expr in ExpressionJoinList)
            {
                var joinTableName = ExpressionParser.GetTableName(expr.type);

                var join = " JOIN ";
                switch (expr.joinType)
                {
                    case JoinType.InnerJoin: join = " INNER JOIN "; break;
                    case JoinType.LeftJoin: join = " LEFT JOIN "; break;
                    case JoinType.RightJoin: join = " RIGHT JOIN "; break;
                }

                queryResult.SqlBuilder.Append($" \n{join} {joinTableName} ON ");

                this.ExpressionParser.QueryParser(expr.property, queryResult, false);
            }
        }

        private void ParseWhere(SqlQuery queryResult)
        {
            if (ExpressionWhereList == null && WhereStrList == null)
                return;

            var whereAdded = false;

            if (ExpressionWhereList != null)
            {
                foreach (var expression in ExpressionWhereList)
                {
                    queryResult.SqlBuilder.Append(whereAdded ? "\n\tAND " : "\nWHERE ");

                    this.ExpressionParser.QueryParser(expression, queryResult, false);

                    whereAdded = true;
                }
            }

            if (WhereStrList != null)
            {
                foreach (var (whereStr, param) in WhereStrList)
                {
                    queryResult.SqlBuilder.Append(whereAdded ? "\n\tAND " : "\nWHERE ");

                    queryResult.SqlBuilder.Append(whereStr);
                    queryResult.SetParam(param);

                    whereAdded = true;
                }
            }
        }

        private void ParseOrderBy(SqlQuery queryResult)
        {
            if (ExpressionOrderList == null)
                return;

            var orderByAdded = false;

            foreach (var (property, ascendant) in ExpressionOrderList)
            {
                queryResult.SqlBuilder.Append(orderByAdded ? "," : " \nORDER BY ");
                this.ExpressionParser.SelectParser(property, queryResult);
                queryResult.SqlBuilder.Append(ascendant ? " ASC " : " DESC ");

                orderByAdded = true;
            }
        }
    }
}
