using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using IBaseFramework.DapperExtension.ExpressionTree;

namespace IBaseFramework.DapperExtension
{
    public partial class SqlGenerator<TEntity>
    {
        private readonly List<string> _conditions = new List<string>();
        private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();
        private int _paramIndex = 0;
        private readonly Dictionary<ExpressionType, string> _operationDictionary = new Dictionary<ExpressionType, string>()
        {
            { ExpressionType.Equal, "="},
            { ExpressionType.NotEqual, "!="},
            { ExpressionType.GreaterThan, ">"},
            { ExpressionType.LessThan, "<"},
            { ExpressionType.GreaterThanOrEqual, ">="},
            { ExpressionType.LessThanOrEqual, "<="},
            { ExpressionType.And, " AND "},
            { ExpressionType.AndAlso,  " AND "},
            { ExpressionType.Or, " OR "},
            { ExpressionType.OrElse, " OR "},
        };

        public string ResolveQuery<T>(Expression<Func<T, bool>> expression, ref IDictionary<string, object> parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            _conditions.Clear();
            _parameters.Clear();
            _paramIndex = 0;

            var expressionTree = ResolveQuery((dynamic)expression.Body);
            BuildSql(expressionTree);

            parameters = _parameters;

            return string.Join("", _conditions);
        }

        #region ResolveQuery
        private static Node ResolveQuery(ConstantExpression constantExpression)
        {
            return new ValueNode() { Value = constantExpression.Value };
        }

        private Node ResolveQuery(UnaryExpression unaryExpression)
        {
            return new SingleOperationNode()
            {
                Operator = unaryExpression.NodeType,
                Child = ResolveQuery((dynamic)unaryExpression.Operand)
            };
        }

        private Node ResolveQuery(BinaryExpression binaryExpression)
        {
            return new OperationNode
            {
                Left = ResolveQuery((dynamic)binaryExpression.Left),
                Operator = binaryExpression.NodeType,
                Right = ResolveQuery((dynamic)binaryExpression.Right)
            };
        }

        private Node ResolveQuery(MethodCallExpression callExpression)
        {
            if (Enum.TryParse(callExpression.Method.Name, true, out LikeMethod callFunction))
            {
                object valueObj;
                var member = callExpression.Object is MemberExpression expression
                    ? expression
                    : (MemberExpression)callExpression.Arguments[1];

                if (member.Type.IsGenericType && callExpression.Arguments[0] is MemberExpression argExp)//判断为List集合
                {
                    valueObj = GetExpressionValue(member);
                    member = argExp;
                    callFunction = LikeMethod.In;
                }
                else if (callExpression.Arguments[0].Type.IsArray)//判断为Array集合
                {
                    valueObj = GetExpressionValue(callExpression.Arguments[0]);
                    callFunction = LikeMethod.In;
                }
                else
                {
                    valueObj = GetExpressionValue(callExpression.Arguments.First());
                }

                return new LikeNode()
                {
                    MemberNode = new MemberNode()
                    {
                        TableName = TableName,
                        FieldName = GetColumnName(member)
                    },
                    Method = callFunction,
                    Value = valueObj
                };
            }
            else
            {
                var value = ResolveMethodCall(callExpression);
                return new ValueNode() { Value = value };
            }
        }

        private Node ResolveQuery(MemberExpression memberExpression, MemberExpression rootExpression = null)
        {
            if (memberExpression.Expression != null)
            {
                rootExpression = rootExpression ?? memberExpression;
                switch (memberExpression.Expression.NodeType)
                {
                    case ExpressionType.Parameter:
                        return new MemberNode()
                        { TableName = TableName, FieldName = GetColumnName(rootExpression) };
                    case ExpressionType.MemberAccess:
                        return ResolveQuery(memberExpression.Expression as MemberExpression, rootExpression);
                    case ExpressionType.Call:
                    case ExpressionType.Constant:
                        return new ValueNode() { Value = GetExpressionValue(rootExpression) };
                    default:
                        throw new ArgumentException("Expected member expression");
                }
            }
            else
            {
                var compiled = Expression.Lambda(memberExpression).Compile();
                var eval = compiled.DynamicInvoke();
                return new ValueNode() { Value = eval };
            }
        }

        private object GetExpressionValue(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Constant:
                    return (expression as ConstantExpression)?.Value;
                case ExpressionType.Call:
                    return ResolveMethodCall(expression as MethodCallExpression);
                case ExpressionType.MemberAccess:
                    var memberExpr = (expression as MemberExpression);
                    if (memberExpr != null && memberExpr.Expression == null)
                    {
                        var compiled = Expression.Lambda(memberExpr).Compile();
                        var eval = compiled.DynamicInvoke();
                        return eval;
                    }
                    var obj = GetExpressionValue(memberExpr?.Expression);
                    return ResolveValue((dynamic)memberExpr?.Member, obj);
                case ExpressionType.Convert:
                    var express = expression as UnaryExpression;
                    return GetExpressionValue(express?.Operand);
                default:
                    throw new ArgumentException("Expected constant expression");
            }
        }

        private object ResolveMethodCall(MethodCallExpression callExpression)
        {
            var arguments = callExpression.Arguments.Select(GetExpressionValue).ToArray();
            var obj = callExpression.Object != null ? GetExpressionValue(callExpression.Object) : arguments.First();

            return callExpression.Method.Invoke(obj, arguments);
        }

        private static object ResolveValue(PropertyInfo property, object obj)
        {
            return property.GetValue(obj, null);
        }

        private static object ResolveValue(FieldInfo field, object obj)
        {
            return field.GetValue(obj);
        }

        private static void ResolveQuery(Expression expression)
        {
            throw new ArgumentException($"The provided expression '{expression.NodeType}' is currently not supported");
        }
        #endregion

        #region BuildSql

        private void BuildSql(Node node)
        {
            BuildSql((dynamic)node);
        }

        private void BuildSql(ValueNode node)
        {
            if (node.Value is bool boolValue)
            {
                if (!boolValue)
                {
                    _conditions.Add(" FALSE ");
                }
            }
        }

        private void BuildSql(LikeNode node)
        {
            if (node.Method == LikeMethod.Equals)
            {
                this.QueryByField(node.MemberNode.TableName, node.MemberNode.FieldName,
                    _operationDictionary[ExpressionType.Equal], node.Value);
            }
            else
            {
                var value = node.Value;
                switch (node.Method)
                {
                    case LikeMethod.StartsWith:
                        value = node.Value + "%";
                        break;
                    case LikeMethod.EndsWith:
                        value = "%" + node.Value;
                        break;
                    case LikeMethod.Contains:
                        value = "%" + node.Value + "%";
                        break;
                    case LikeMethod.In:
                        value = node.Value;
                        break;
                }
                this.QueryByFieldLike(node.MemberNode.TableName, node.MemberNode.FieldName, value);
            }
        }

        private void BuildSql(OperationNode node)
        {
            BuildSql((dynamic)node.Left, (dynamic)node.Right, node.Operator);
        }

        private void BuildSql(MemberNode memberNode)
        {
            this.QueryByField(memberNode.TableName, memberNode.FieldName, _operationDictionary[ExpressionType.Equal], true);
        }

        private void BuildSql(SingleOperationNode node)
        {
            if (node.Operator == ExpressionType.Not)
                _conditions.Add(" NOT ");
            BuildSql(node.Child);
        }

        private void BuildSql(MemberNode memberNode, ValueNode valueNode, ExpressionType op)
        {
            if (valueNode.Value == null)
            {
                switch (op)
                {
                    case ExpressionType.Equal:
                        _conditions.Add($"{Field(memberNode.TableName, memberNode.FieldName)} IS NULL");
                        break;
                    case ExpressionType.NotEqual:
                        _conditions.Add($"{Field(memberNode.TableName, memberNode.FieldName)} IS NOT NULL");
                        break;
                }
            }
            else
            {
                this.QueryByField(memberNode.TableName, memberNode.FieldName, _operationDictionary[op], valueNode.Value);
            }
        }

        private void BuildSql(ValueNode valueNode, MemberNode memberNode, ExpressionType op)
        {
            BuildSql(memberNode, valueNode, op);
        }

        private void BuildSql(MemberNode leftMember, MemberNode rightMember, ExpressionType op)
        {
            var newCondition = $"{Field(leftMember.TableName, leftMember.FieldName)} {_operationDictionary[op]} {Field(rightMember.TableName, rightMember.FieldName)}";

            _conditions.Add(newCondition);
        }

        private void BuildSql(SingleOperationNode leftMember, SingleOperationNode rightMember, ExpressionType op)
        {
            BuildSql((dynamic)leftMember.Child, (dynamic)rightMember.Child, op);
        }

        private void BuildSql(SingleOperationNode leftMember, Node rightMember, ExpressionType op)
        {
            if (leftMember.Operator == ExpressionType.Not)
                BuildSql(leftMember as Node, rightMember, op);
            else
                BuildSql((dynamic)leftMember.Child, (dynamic)rightMember, op);
        }

        private void BuildSql(Node leftMember, SingleOperationNode rightMember, ExpressionType op)
        {
            BuildSql(rightMember, leftMember, op);
        }

        private void BuildSql(Node leftNode, Node rightNode, ExpressionType op)
        {
            _conditions.Add("(");

            BuildSql((dynamic)leftNode);

            if (_conditions.Count > 0)
            {
                if (leftNode is ValueNode leftValueNode)
                {
                    if (leftValueNode.Value is bool leftValue)
                    {
                        if (!leftValue)
                        {
                            _conditions.Add(_operationDictionary[op]);
                        }
                    }
                }
                else if (rightNode is ValueNode rightValueNode)
                {
                    if (rightValueNode.Value is bool rightValue)
                    {
                        if (!rightValue)
                        {
                            _conditions.Add(_operationDictionary[op]);
                        }
                    }
                }
                else
                {
                    _conditions.Add(_operationDictionary[op]);
                }
            }

            BuildSql((dynamic)rightNode);

            _conditions.Add(")");
        }

        private void QueryByField(string tableName, string fieldName, string op, object fieldValue)
        {
            var paramId = NextParamId();
            var newCondition = $"{Field(tableName, fieldName)} {op} {Parameter(paramId)}";

            _conditions.Add(newCondition);
            AddParameter(paramId, fieldValue);
        }

        private void QueryByFieldLike(string tableName, string fieldName, object fieldValue)
        {
            if (fieldValue is string)
            {
                var paramId = NextParamId();
                var newCondition = $"{Field(tableName, fieldName)} LIKE {Parameter(paramId)}";

                _conditions.Add(newCondition);
                AddParameter(paramId, fieldValue);
            }
            else
            {
                var valueList = fieldValue as IList;

                var parameters = new List<string>();
                if (valueList != null)
                {
                    foreach (var item in valueList)
                    {
                        var paramId = NextParamId();
                        AddParameter(paramId, item);
                        parameters.Add(Parameter(paramId));
                    }
                }

                var newCondition = $"{Field(tableName, fieldName)} IN ({ string.Join(",", parameters)})";
                _conditions.Add(newCondition);
            }
        }

        #endregion

        private string Field(string tableName, string fieldName)
        {
            return $"{tableName}.{Config.StartQuotationMark}{fieldName}{Config.EndQuotationMark}";
        }

        private string Parameter(string parameterId)
        {
            return "@" + parameterId;
        }

        private string NextParamId()
        {
            ++_paramIndex;
            return "p" + _paramIndex.ToString(CultureInfo.InvariantCulture);
        }

        private void AddParameter(string key, object value)
        {
            if (!_parameters.ContainsKey(key))
                _parameters.Add(key, value);
        }

        private string GetColumnName(Expression expression)
        {
            var member = GetMemberExpression(expression);
            return member.Member.Name;
        }

        private MemberExpression GetMemberExpression(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.MemberAccess:
                    return expression as MemberExpression;
                case ExpressionType.Convert:
                    return GetMemberExpression((expression as UnaryExpression)?.Operand);
            }

            throw new ArgumentException("Member expression expected");
        }
    }

    public enum LikeMethod
    {
        StartsWith,
        EndsWith,
        Contains,
        Equals,
        In
    }
}
