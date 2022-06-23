using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace IBaseFramework.ExpressionVisit
{
    public class QueryHelper
    {
        public static object GetValueFromExpression(Expression expression)
        {
            if (expression.NodeType == ExpressionType.NewArrayInit)
            {
                if (expression is NewArrayExpression newArrayExpression)
                {
                    var arrayValueLength = newArrayExpression.Expressions.Count;
                    var array = new object[arrayValueLength];
                    for (var i = 0; i < arrayValueLength; i++)
                    {
                        array[i] = GetValueFromExpression(newArrayExpression.Expressions[i]);
                    }

                    return array;
                }
            }
            else if (expression.NodeType == ExpressionType.ListInit)
            {
                if (expression is ListInitExpression listInitExpression)
                {
                    var listLength = listInitExpression.Initializers.Count;
                    var list = new List<object>(listLength);

                    for (var i = 0; i < listLength; i++)
                    {
                        list.Add(GetValueFromExpression(listInitExpression.Initializers[i].Arguments[0]));
                    }

                    return list;
                }
            }
            else if (expression.NodeType == ExpressionType.Constant)
            {
                if (expression is ConstantExpression constantExpression)
                    return constantExpression.Value;
            }
            else if (expression.NodeType == ExpressionType.Call)
            {
                if (expression is MethodCallExpression callExpression)
                {
                    var arguments = callExpression.Arguments
                        .Select(GetValueFromExpression)
                        .ToArray();

                    var obj = callExpression.Object != null
                        ? GetValueFromExpression(callExpression.Object)
                        : arguments[0];

                    return callExpression.Method.Invoke(obj, arguments);
                }
            }
            else if (expression.NodeType == ExpressionType.MemberAccess)
            {
                if (expression is MemberExpression memberExpr)
                {
                    if (memberExpr.Expression == null)
                    {
                        return ResolveValue(memberExpr.Member, null);
                    }

                    var obj = GetValueFromExpression(memberExpr.Expression);
                    return ResolveValue(memberExpr.Member, obj);
                }

                object ResolveValue(MemberInfo member, object obj)
                {
                    switch (member)
                    {
                        case PropertyInfo propertyInfo:
                            return propertyInfo.GetValue(obj, null);
                        case FieldInfo fieldInfo:
                            return fieldInfo.GetValue(obj);
                        default:
                            throw new ArgumentException($"Member '{member.Name}' is not supported");
                    }
                }
            }
            else if (expression.NodeType == ExpressionType.Convert)
            {
                if (expression is UnaryExpression convertExpression)
                {
                    return GetValueFromExpression(convertExpression.Operand);
                }
            }
            else if (expression.NodeType == ExpressionType.Lambda)
            {
                if (expression is LambdaExpression lambda)
                {
                    if (lambda.Body is ConstantExpression constant)
                    {
                        return constant.Value;
                    }

                    return lambda.Compile().DynamicInvoke();
                }
            }

            return Expression.Lambda(expression).Compile().DynamicInvoke();
        }

        internal static bool IsVariable(Expression expr)
        {
            return expr is MemberExpression && ((MemberExpression)expr).Expression is ConstantExpression;
        }


        internal static bool IsHasValue(MemberExpression expr)
        {
            return expr.Member.Name == "HasValue";
        }

        internal static bool IsBoolean(MemberExpression member)
        {
            var type = (member.Member as PropertyInfo)?.PropertyType;
            return type == typeof(bool) || type == typeof(bool?);
        }

        internal static bool IsString(MemberExpression member)
        {
            var type = (member.Member as PropertyInfo)?.PropertyType;
            return type == typeof(string);
        }

        internal static bool IsBoolean(Type type)
        {
            return type == typeof(bool) || type == typeof(bool?);
        }

        internal static Expression GetFirstMemberExpression(Expression expression)
        {
            if (expression is BinaryExpression binaryExpression)
            {
                return GetFirstMemberExpression(binaryExpression.Left)
                    ?? GetFirstMemberExpression(binaryExpression.Right);
            }
            else if (expression is UnaryExpression unaryExpression)
            {
                return GetFirstMemberExpression(unaryExpression.Operand);
            }
            else if (expression is LambdaExpression lambdaExpression)
            {
                return GetFirstMemberExpression(lambdaExpression.Body);
            }
            else if (expression is NewArrayExpression newArrayExpression)
            {
                return newArrayExpression.Expressions
                    .Select(a => GetFirstMemberExpression(a))
                    .FirstOrDefault(e => e != null);
            }
            else if (expression is MethodCallExpression callExpression)
            {
                var res = GetFirstMemberExpression(callExpression.Object);
                res ??= callExpression.Arguments
                      .Select(a => GetFirstMemberExpression(a))
                      .FirstOrDefault(e => e != null);
                return res;
            }
            else if (expression is MemberExpression memberExpression)
            {
                if (memberExpression == null || memberExpression.Expression == null)
                    return null;

                if (memberExpression.Expression.NodeType == ExpressionType.Parameter)
                {
                    return expression;
                }
                else if (memberExpression.Expression.NodeType == ExpressionType.MemberAccess)
                {
                    return GetFirstMemberExpression(memberExpression.Expression);
                }
            }

            return null;
        }


        internal static string GetOperator(BinaryExpression b)
        {
            switch (b.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return IsBoolean(b.Left.Type) ? "AND" : "&";
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return IsBoolean(b.Left.Type) ? "OR" : "|";
                default:
                    return GetOperator(b.NodeType);
            }
        }

        internal static bool IsLogicalOperation(ExpressionType nodeType)
        {
            switch (nodeType)
            {
                case ExpressionType.Not:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return true;
                default:
                    return false;
            }
        }

        internal static string GetOperator(ExpressionType exprType)
        {
            switch (exprType)
            {
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.NotEqual:
                    return "<>";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return "+";
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return "-";
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return "*";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.Modulo:
                    return "%";
                case ExpressionType.ExclusiveOr:
                    return "^";
                case ExpressionType.LeftShift:
                    return "<<";
                case ExpressionType.RightShift:
                    return ">>";
                default:
                    return "";
            }
        }

    }
}
