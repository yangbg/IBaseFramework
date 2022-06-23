using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace IBaseFramework.ExpressionVisit
{
    public partial class ExpressionParser
    {
        private int paraIndex = 1;
        private string GetNewParameterId() => $"arg_{paraIndex++}";
        public void AddOperator(string op)
        {
            sql.Append($" {op} ");
        }

        public void AddSql(string sqlExpression)
        {
            sql.Append(sqlExpression);
        }

        public void OpenBrace()
        {
            sql.Append("(");
        }

        public void CloseBrace()
        {
            sql.Append(")");
        }

        //public void AddLikePrefix()
        //{
        //    sql.Append("'%' + ");
        //}

        //internal void AddLikeSuffix()
        //{
        //    sql.Append("+ '%'");
        //}

        public void AddLike(bool not = false)
        {
            AddOperator(not ? "NOT LIKE" : "LIKE");
        }

        public void AddIn(bool not = false, IList elements = null)
        {
            AddOperator(not ? "NOT IN" : "IN");

            if (elements != null)
            {
                var elementParas = new List<string>();
                foreach (var item in elements)
                {
                    var argId = GetNewParameterId();
                    parameters.Add(argId, item);

                    elementParas.Add($"@{argId}");
                }

                sql.Append("(" + string.Join(", ", elementParas) + ")");
            }
        }

        public void AddBoolean(bool not)
        {
            sql.Append(not ? " = 0" : " = 1");
        }

        public void AddIsNull(bool not)
        {
            sql.Append(not ? " IS NOT NULL" : " IS NULL");
        }

        public void AddParameter(object val, string argId = null)
        {
            if (val == null)
            {
                sql.Append("NULL");
                return;
            }

            argId ??= GetNewParameterId();
            parameters.Add(argId, val);

            sql.Append($"@{argId}");
        }

        public void AddExpression(Expression val, string argId = null)
        {
            argId ??= GetNewParameterId();
            parameters.Add(argId, val);

            sql.Append($"@{argId}");
        }

        public void AddIsNullOrEmpty(Type tableType, string columnName, bool not)
        {
            var property = Encapsulation(columnName, GetTableName(tableType));
            sql.Append(not ? $"ISNULL({property}, '') <> ''" : $"ISNULL({property}, '') = ''");
        }

        public bool AddTableField(Type tableType, string fieldName)
        {
            if (!string.IsNullOrEmpty(fieldName))
            {
                sql.Append(Encapsulation(fieldName, GetTableName(tableType)));

                return true;
            }

            return false;
        }

        public void AddColumnSelector(object val)
        {
            var dbColumn = val.ToString();
            sql.Append(sql.Length > 1 ? $", {dbColumn}" : $"{dbColumn}");
            return;
        }

        public bool AddColumnSelector(Type tableType, string columnName)
        {
            if (!string.IsNullOrEmpty(columnName))
            {
                var dbColumn = Encapsulation(columnName, GetTableName(tableType));
                sql.Append(sql.Length > 1 ? $", {dbColumn}" : dbColumn);

                return true;
            }

            return false;
        }

        public string Encapsulation(string dbWord, string tableAlias = null)
        {
            if (string.IsNullOrEmpty(tableAlias))
                return $"{StartQuotationMark}{dbWord}{EndQuotationMark}";

            return $"{StartQuotationMark}{tableAlias}{EndQuotationMark}.{StartQuotationMark}{dbWord}{EndQuotationMark}";
        }
    }
}

