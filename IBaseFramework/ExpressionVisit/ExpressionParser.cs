using IBaseFramework.Infrastructure;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace IBaseFramework.ExpressionVisit
{
    public partial class ExpressionParser : ExpressionVisitor 
    {
        private List<ExpressionType> operators;
        private bool _isSelectParser = false;

        private StringBuilder sql = new StringBuilder();
        private Dictionary<string, object> parameters = new Dictionary<string, object>();

        private readonly SqlProvider _sqlProvider = SqlProvider.MySQL;

        // ReSharper disable once StaticMemberInGenericType
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> ReflectionPropertyCache = new ConcurrentDictionary<Type, PropertyInfo[]>();
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> KeysPropertyCache = new ConcurrentDictionary<Type, PropertyInfo[]>();
        private static readonly ConcurrentDictionary<Type, PropertyInfo> IdentityKeyPropertyCache = new ConcurrentDictionary<Type, PropertyInfo>();
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ConcurrentDictionary<Type, string> TableNameCache = new ConcurrentDictionary<Type, string>();

        public string StartQuotationMark
        {
            get;
        }

        public string EndQuotationMark
        {
            get;
        }

        public ExpressionParser(SqlProvider sqlProvider = SqlProvider.MySQL)
        {
            operators = new List<ExpressionType>();
            _sqlProvider = sqlProvider;

            switch (sqlProvider)
            {
                case SqlProvider.SQLServer:
                    StartQuotationMark = "[";
                    EndQuotationMark = "]";

                    break;

                case SqlProvider.MySQL:
                    StartQuotationMark = "`";
                    EndQuotationMark = "`";

                    break;

                case SqlProvider.PostgreSQL:
                    StartQuotationMark = "\"";
                    EndQuotationMark = "\"";

                    break;

                default:
                    StartQuotationMark = "\"";
                    EndQuotationMark = "\"";
                    break;
            }
        }

        public ExpressionParser QueryParser(Expression expression, SqlQuery sqlQuery = null, bool appendWhere = true)
        {
            sql.Clear();
            parameters.Clear();

            _isSelectParser = false;

            Visit(expression);

            if (sqlQuery != null)
            {
                if (sql.Length > 0)
                {
                    if (appendWhere)
                        sqlQuery.SqlBuilder.Append(" WHERE ");

                    sqlQuery.SqlBuilder.Append(sql);

                    sqlQuery.SetParam(this.GetParameters());
                }
            }

            return this;
        }

        public ExpressionParser SelectParser(Expression expression, SqlQuery sqlQuery = null)
        {
            sql.Clear();
            parameters.Clear();

            _isSelectParser = true;

            Visit(expression);

            if (sqlQuery != null)
            {
                if (sql.Length > 0)
                {
                    sqlQuery.SqlBuilder.Append(sql);
                }
            }

            return this;
        }

        public IDictionary<string, object> GetParameters()
        {
            return parameters.ToDictionary(e => e.Key,
                e =>
                {
                    if (e.Value is Expression node)
                    {
                        return QueryHelper.GetValueFromExpression(node);
                    }
                    else
                    {
                        return e.Value;
                    }
                });
        }

        public string GetSql()
        {
            return sql.ToString();
        }


    }
}
