using System;
using System.Linq.Expressions;
using System.Reflection;

namespace EasyDynamo.Extensions
{
    public static class ExpressionExtensions
    {
        public static MemberExpression TryGetMemberExpression<TSource, TDestination>(
            this Expression<Func<TSource, TDestination>> sourceExpression)
        {
            var memberExpr = sourceExpression.Body as MemberExpression
                ?? (sourceExpression.Body as UnaryExpression)?.Operand as MemberExpression;

            return memberExpr;
        }

        public static string TryGetMemberName<TSource, TDestination>(
            this Expression<Func<TSource, TDestination>> sourceExpression)
        {
            return TryGetMemberExpression(sourceExpression)?.Member?.Name;
        }

        public static Type TryGetMemberType<TSource, TDestination>(
            this Expression<Func<TSource, TDestination>> sourceExpression)
        {
            return (TryGetMemberExpression(sourceExpression)?.Member as PropertyInfo)?.PropertyType;
        }
    }
}
