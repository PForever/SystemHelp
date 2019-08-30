using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SystemHelp
{
    public static class ExpressionHelper
    {
        internal static Expression<Action<object, T>> CreateSetProperty<T>(object defaultObject, string propertyName)
        {
            var varriable = Expression.Variable(typeof(object));
            var varriableConverted = Expression.Convert(varriable, defaultObject.GetType());
            var value = Expression.Variable(typeof(T));
            var property = Expression.PropertyOrField(varriableConverted, propertyName);
            var act = Expression.Assign(property, value);
            return Expression.Lambda<Action<object, T>>(act, varriable, value);
        }
        internal static Expression<Func<object, T>> CreateGetProperty<T>(object defaultObject, string propertyName)
        {
            var varriable = Expression.Variable(typeof(object));
            var varriableConverted = Expression.Convert(varriable, defaultObject.GetType());
            Expression property = Expression.PropertyOrField(varriableConverted, propertyName);
            if (property.Type != typeof(T)) property = Expression.Convert(property, typeof(T));
            return Expression.Lambda<Func<object, T>>(property, varriable);
        }
    }
}
