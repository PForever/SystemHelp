using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SystemHelp
{
    public static class ExpressionLogicOperations
    {
        class ParameterVisitor : ExpressionVisitor
        {
            private readonly ReadOnlyCollection<ParameterExpression> from, to;
            public ParameterVisitor(ReadOnlyCollection<ParameterExpression> from, ReadOnlyCollection<ParameterExpression> to)
            {
                if (from == null) throw new ArgumentNullException("from");
                if (to == null) throw new ArgumentNullException("to");
                if (from.Count != to.Count) throw new InvalidOperationException(
                    "Parameter lengths must match");
                this.from = from;
                this.to = to;
            }
            protected override Expression VisitParameter(ParameterExpression node)
            {
                for (int i = 0; i < from.Count; i++)
                {
                    if (node == from[i]) return to[i];
                }
                return node;
            }
        }
        public static Expression<Func<T, bool>> AndAlso<T>(
            Expression<Func<T, bool>> x, Expression<Func<T, bool>> y)
        {
            var newY = new ParameterVisitor(y.Parameters, x.Parameters)
                .VisitAndConvert(y.Body, "AndAlso");
            return Expression.Lambda<Func<T, bool>>(
                Expression.AndAlso(x.Body, newY),
                x.Parameters);
        }
        public static LambdaExpression AndAlso(
            LambdaExpression x, LambdaExpression y)
        {
            var newY = new ParameterVisitor(y.Parameters, x.Parameters)
                .VisitAndConvert(y.Body, "AndAlso");
            return Expression.Lambda(
                Expression.AndAlso(x.Body, newY),
                x.Parameters);
        }
        public static Expression<Func<T, bool>> OrElse<T>(
            Expression<Func<T, bool>> x, Expression<Func<T, bool>> y)
        {
            var newY = new ParameterVisitor(y.Parameters, x.Parameters)
                .VisitAndConvert(y.Body, "OrElse");
            return Expression.Lambda<Func<T, bool>>(
                Expression.OrElse(x.Body, newY),
                x.Parameters);
        }
        public static LambdaExpression OrElse(
            LambdaExpression x, LambdaExpression y)
        {
            var newY = new ParameterVisitor(y.Parameters, x.Parameters)
                .VisitAndConvert(y.Body, "OrElse");
            return Expression.Lambda(
                Expression.OrElse(x.Body, newY),
                x.Parameters);
        }
        public static Expression<Func<T, bool>> Or<T>(
            Expression<Func<T, bool>> x, Expression<Func<T, bool>> y)
        {
            var newY = new ParameterVisitor(y.Parameters, x.Parameters)
                .VisitAndConvert(y.Body, "Or");
            return Expression.Lambda<Func<T, bool>>(
                Expression.Or(x.Body, newY),
                x.Parameters);
        }
        public static LambdaExpression Or(
            LambdaExpression x, LambdaExpression y)
        {
            var newY = new ParameterVisitor(y.Parameters, x.Parameters)
                .VisitAndConvert(y.Body, "Or");
            return Expression.Lambda(
                Expression.Or(x.Body, newY),
                x.Parameters);
        }
    }
}
