using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace IBaseFramework.Extension
{
    public static class ExpressionEx
    {
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.AndAlso);
        }

        public static Expression<Func<T, bool>> AndIf<T>(this Expression<Func<T, bool>> first, bool condition, Expression<Func<T, bool>> second)
        {
            return condition ? first.Compose(second, Expression.AndAlso) : first;
        }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.OrElse);
        }

        public static Expression<Func<T, bool>> OrIf<T>(this Expression<Func<T, bool>> first, bool condition, Expression<Func<T, bool>> second)
        {
            return condition ? first.Compose(second, Expression.OrElse) : first;
        }

        public static Expression<Func<T, bool>> Not<T>(this Expression<Func<T, bool>> expression)
        {
            var negated = Expression.Not(expression.Body);
            return Expression.Lambda<Func<T, bool>>(negated, expression.Parameters);
        }

        static Expression<T> Compose<T>(this Expression<T> expression, Expression<T> extendExpression, Func<Expression, Expression, Expression> mergeWay)
        {
            var parameterExpressionSetter = expression.Parameters
            .Select((u, i) => new { u, Parameter = extendExpression.Parameters[i] })
            .ToDictionary(d => d.Parameter, d => d.u);

            var extendExpressionBody = ParameterReplaceExpressionVisitor.ReplaceParameters(parameterExpressionSetter, extendExpression.Body);
            return Expression.Lambda<T>(mergeWay(expression.Body, extendExpressionBody), expression.Parameters);
        }

        /// <summary>
        /// 处理 Lambda 参数不一致问题
        /// </summary>
        class ParameterReplaceExpressionVisitor : ExpressionVisitor
        {
            /// <summary>
            /// 参数表达式映射集合
            /// </summary>
            private readonly Dictionary<ParameterExpression, ParameterExpression> parameterExpressionSetter;

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="parameterExpressionSetter">参数表达式映射集合</param>
            public ParameterReplaceExpressionVisitor(Dictionary<ParameterExpression, ParameterExpression> parameterExpressionSetter)
            {
                this.parameterExpressionSetter = parameterExpressionSetter ?? new Dictionary<ParameterExpression, ParameterExpression>();
            }

            /// <summary>
            /// 替换表达式参数
            /// </summary>
            /// <param name="parameterExpressionSetter">参数表达式映射集合</param>
            /// <param name="expression">表达式</param>
            /// <returns>新的表达式</returns>
            public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> parameterExpressionSetter, Expression expression)
            {
                return new ParameterReplaceExpressionVisitor(parameterExpressionSetter).Visit(expression);
            }

            /// <summary>
            /// 重写基类参数访问器
            /// </summary>
            /// <param name="parameterExpression"></param>
            /// <returns></returns>
            protected override Expression VisitParameter(ParameterExpression parameterExpression)
            {
                if (parameterExpressionSetter.TryGetValue(parameterExpression, out var replacement))
                {
                    parameterExpression = replacement;
                }

                return base.VisitParameter(parameterExpression);
            }
        }
    }
}
