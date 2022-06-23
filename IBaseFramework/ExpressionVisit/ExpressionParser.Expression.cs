using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;

namespace IBaseFramework.ExpressionVisit
{
    public partial class ExpressionParser
    {
        protected ExpressionType? LastOperator()
        {
            if (operators.Any())
                return operators.Last();
            return null;
        }

        private static ExpressionType[] noOpenBraceTypes = new[] {
            ExpressionType.Not,
            ExpressionType.Convert
        };

        private static ExpressionType[] notOperators = new[] {
            ExpressionType.Not,
            ExpressionType.NotEqual
        };

        protected bool IsNotLastOperator()
        {
            if (operators.Any())
            {
                return notOperators.Contains(LastOperator().Value);
            }
            return false;
        }

        protected Expression WithOperator(ExpressionType nodeType, Func<Expression> action)
        {
            operators.Add(nodeType);

            var openBrace = operators.Count() > 1 && !noOpenBraceTypes.Contains(nodeType);

            if (openBrace) OpenBrace();

            var res = action.Invoke();

            operators.RemoveAt(operators.Count() - 1);
            if (openBrace) CloseBrace();

            return res;
        }

        public override Expression Visit(Expression node)
        {
            if (_isSelectParser)
            {
                return base.Visit(node);
            }

            var member = QueryHelper.GetFirstMemberExpression(node);
            if (member == null && node.NodeType != ExpressionType.Parameter)
            {
                AddExpression(node);
                return node;
            }
            else
            {
                return base.Visit(node);
            }
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            return WithOperator(node.NodeType, () =>
            {
                var not = IsNotLastOperator();
                if (node.Left is ConstantExpression left && left.Value == null)
                {
                    Visit(node.Right);
                    AddIsNull(not);
                }
                else if (node.Right is ConstantExpression right && right.Value == null)
                {
                    Visit(node.Left);
                    AddIsNull(not);
                }
                else
                {
                    Visit(node.Left);
                    AddOperator(QueryHelper.GetOperator(node));
                    Visit(node.Right);
                }
                return node;
            });
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (_isSelectParser)
            {
                AddColumnSelector(node.Value);
                return node;
            }

            AddParameter(node.Value);
            return base.VisitConstant(node);
        }

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            return base.VisitConditional(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (_isSelectParser)
            {
                if (node.Type.IsClass && node.Type != typeof(string))
                {
                    var columns = GetSelectColumnList(node.Type);
                    if (columns.Any())
                    {
                        foreach (var item in columns)
                        {
                            AddColumnSelector(node.Type, item.Name);
                        }
                    }

                    return node;
                }
                else
                {
                    if (AddColumnSelector(node.Expression.Type, node.Member.Name))
                    {
                        return node;
                    }
                }

                return base.VisitMember(node);
            }

            if ((node.Expression.NodeType == ExpressionType.Parameter || node.Expression.NodeType == ExpressionType.MemberAccess) && AddTableField(node.Expression.Type, node.Member.Name))
            {
                var lastOp = LastOperator();
                var not = IsNotLastOperator();

                if (lastOp == null || QueryHelper.IsLogicalOperation(lastOp.Value))
                {
                    if (QueryHelper.IsBoolean(node))
                    {
                        AddBoolean(not);
                    }
                    else
                    {
                        AddIsNull(not);
                    }
                }

                return node;
            }

            // External Variable Value
            if (QueryHelper.IsVariable(node))
            {
                AddExpression(node);
                return node;
            }

            return base.VisitMember(node);
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            if (!_isSelectParser)
                return base.VisitMemberInit(node);

            for (int i = 0; i < node.Bindings.Count; i++)
            {
                var memberName = string.Empty;
                var columnName = string.Empty;

                if (node.Bindings[i].Member != null)
                    memberName = node.Bindings[i].Member.Name;

                if (node.Bindings[i] is MemberAssignment memberAssignment)
                {
                    if (memberAssignment.Expression is MemberExpression memberExpression)
                    {
                        columnName = memberExpression.Member.Name;
                    }

                    Visit(memberAssignment.Expression);

                    if (!string.IsNullOrEmpty(memberName) && !columnName.Equals(memberName, StringComparison.InvariantCultureIgnoreCase))
                        sql.Append($" AS {memberName}");
                }
            }

            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var not = IsNotLastOperator();

            switch (node.Method.Name)
            {
                case MethodCall.Contains:
                    if (node.Method.DeclaringType == typeof(string))
                    {
                        return WithOperator(node.NodeType, () => StringLike(node, not, true, true));
                    }
                    else
                    {
                        return WithOperator(node.NodeType, () => ListIn(node, not));
                    }
                case MethodCall.EndsWith:
                    return WithOperator(node.NodeType, () => StringLike(node, not, true, false));
                case MethodCall.StartsWith:
                    return WithOperator(node.NodeType, () => StringLike(node, not, false, true));
                case MethodCall.IsNullOrEmpty:
                    if (node.Arguments[0] is MemberExpression member && QueryHelper.IsString(member))
                    {
                        StringNullOrEmpty(member, not);
                        return node;
                    }
                    break;
            }
            return base.VisitMethodCall(node);
        }

        protected override Expression VisitNew(NewExpression node)
        {
            if (!_isSelectParser)
                return base.VisitNew(node);

            for (int i = 0; i < node.Arguments.Count; i++)
            {
                if (node.Arguments[i].Type.IsClass && node.Arguments[i].Type != typeof(string))
                {
                    var columns = GetSelectColumnList(node.Arguments[i].Type);
                    if (columns.Any())
                    {
                        foreach (var item in columns)
                        {
                            AddColumnSelector(node.Arguments[i].Type, item.Name);
                        }
                    }

                    return node;
                }
                else
                {
                    var memberName = string.Empty;
                    var columnName = string.Empty;

                    if (node.Members != null)
                        memberName = node.Members[i].Name;

                    if (node.Arguments[i] is MemberExpression memberExpression)
                    {
                        columnName = memberExpression.Member.Name;

                        if (!AddColumnSelector(memberExpression.Expression.Type, columnName))
                        {
                            Visit(node.Arguments[i]);
                        }
                    }
                    else if (node.Arguments[i] is ConstantExpression constantExpression)
                    {
                        columnName = constantExpression.Value.ToString();
                        AddColumnSelector(columnName);
                    }
                    else
                    {
                        Visit(node.Arguments[i]);
                    }

                    if (!string.IsNullOrEmpty(memberName) && !columnName.Equals(memberName, StringComparison.InvariantCultureIgnoreCase))
                        sql.Append($" AS {memberName}");
                }

            }

            return node;
        }

        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            if (_isSelectParser)
            {
                return base.VisitNewArray(node);
            }

            return WithOperator(node.NodeType, () => base.VisitNewArray(node));
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            return WithOperator(node.NodeType, () => base.VisitUnary(node));
        }


        private Expression ListIn(MethodCallExpression node, bool not = false)
        {
            var values = QueryHelper.GetValueFromExpression(node.Object);

            Visit(node.Arguments[0]);

            //var elements = new List<string>();
            //foreach (var e in values)
            //{
            //    if (e is string es)
            //        elements.Add($"'{es}'");
            //    else
            //        elements.Add($"{e}");
            //}

            AddIn(not, values as IList);

            return node;
        }

        private Expression StringLike(MethodCallExpression node, bool not = false, bool prefix = false, bool sufix = false)
        {
            Visit(node.Object);

            AddLike(not);

            var objValue = QueryHelper.GetValueFromExpression(node.Arguments[0]);
            if (prefix)
                objValue = "%" + objValue;
            if (sufix)
                objValue += "%";

            AddParameter(objValue);

            return node;
        }

        private void StringNullOrEmpty(MemberExpression member, bool not = false)
        {
            AddIsNullOrEmpty(member.Expression.Type, member.Member.Name, not);
        }
    }

    public class MethodCall
    {
        public const string EndsWith = "EndsWith";
        public const string StartsWith = "StartsWith";
        public const string Contains = "Contains";
        public const string IsNullOrEmpty = "IsNullOrEmpty";
    }
}
