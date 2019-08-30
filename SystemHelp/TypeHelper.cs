using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SystemHelp
{
    public static class TypeHelper
    {
        private static readonly Type[] _additionalPrimitiveTypes = new[] { typeof(DateTime), typeof(DateTime?), typeof(string), typeof(bool?), typeof(byte?), typeof(short?), typeof(int?), typeof(long?), typeof(decimal?), typeof(float?), typeof(double?) };
        private static readonly Type StringType = typeof(string);
        private static readonly Type[] _intNumbers = new[] { typeof(byte), typeof(short), typeof(int), typeof(long), typeof(byte?), typeof(short?), typeof(int?), typeof(long?), };
        private static readonly Type[] _lineSpaceType = new[] { typeof(DateTime), typeof(DateTime?), typeof(byte?), typeof(short?), typeof(int?), typeof(long?), typeof(decimal?), typeof(float?), typeof(double?), typeof(byte), typeof(short), typeof(int), typeof(long), typeof(decimal), typeof(float), typeof(double) };

        private static readonly Type[] _decNumbers = new[] { typeof(decimal), typeof(double), typeof(float), typeof(decimal?), typeof(double?), typeof(float?) };
        private static readonly Type[] _dateTypes = new[] { typeof(DateTime), typeof(DateTime?) };
        private static readonly Type[] _boolTypes = new[] { typeof(bool), typeof(bool?) };
        private static readonly Type[] _guidTypes = new[] { typeof(Guid), typeof(Guid?) };


        public static bool IsCollection(this Type type) => type.GetInterfaces().Any(i => i.Name.Contains("IEnumerable") && i.IsGenericType);
        internal static Type GetCollectionGenericArg(Type type) => type.GetInterfaces().FirstOrDefault(i => i.Name.Contains("IEnumerable") && i.IsGenericType).GetGenericArguments().Single();
        public static bool IsNulluble(this Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        public static bool IsLineSpace(this Type type) => _lineSpaceType.Contains(type);
        public static bool IsComparable(this Type type) => typeof(IComparable).IsAssignableFrom(type)
                ? true
                : type.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IComparable<>) && i.GenericTypeArguments[0].IsAssignableFrom(type));

        public static bool IsString(this Type t) => t == StringType;
        public static bool IsIntNumber(this Type t) => _intNumbers.Contains(t);
        public static bool IsDecNumber(this Type t) => _decNumbers.Contains(t);
        public static bool IsDate(this Type t) => _dateTypes.Contains(t);
        public static bool IsBool(this Type t) => _boolTypes.Contains(t);
        public static bool IsGuid(this Type t) => _guidTypes.Contains(t);
        public static object CreateNew(this Type type)
        {
            if (type == StringType) return "";
            return Activator.CreateInstance(type);
        }

        internal static string PrintValue(object value)
        {
            switch (value)
            {
                case DateTime d: return d.ToString("d");
                case null: return null;
                default: return value.ToString();
            }
        }

        public static bool IsPrimitive(this Type type) => type.IsPrimitive || _additionalPrimitiveTypes.Contains(type);

        public static void SetProperty<T>(object defaultObject, string propertyName, T propertyValue)
        {
            var type = defaultObject.GetType();
            var setter = DictionatyHost<T>.SetPropertyList.GetOrAdd((type, propertyName), ExpressionHelper.CreateSetProperty<T>(defaultObject, propertyName).Compile());
            setter(defaultObject, propertyValue);
        }
        public static T GetProperty<T>(object defaultObject, string propertyName)
        {
            if (defaultObject == null) return default(T);
            var type = defaultObject.GetType();
            var getter = DictionatyHost<T>.GetPropertyList.GetOrAdd((type, propertyName), ExpressionHelper.CreateGetProperty<T>(defaultObject, propertyName).Compile());
            return getter(defaultObject);
        }


        public static object GetPropertyObject(object v, string member) => string.IsNullOrEmpty(member) ? v : GetProperty<object>(v, member);

        public static string GetPropertyString(object v, string member) => string.IsNullOrEmpty(member) ? v.ToString() : GetProperty<string>(v, member);

        static class DictionatyHost<T>
        {
            public static ConcurrentDictionary<(Type Type, string Property), Action<object, T>> SetPropertyList { get; } = new ConcurrentDictionary<(Type Type, string Property), Action<object, T>>();
            public static ConcurrentDictionary<(Type Type, string Property), Func<object, T>> GetPropertyList { get; } = new ConcurrentDictionary<(Type Type, string Property), Func<object, T>>();
        }


        public static bool TryGetParentName(Expression expression, out string data)
        {
            data = null;
            var expression2 = RemoveConvert(expression);
            var methodCallExpression = expression2 as MethodCallExpression;
            if (expression2 is MemberExpression memberExpression)
            {
                var member = memberExpression.Member;

                if (!TryGetParentName(memberExpression.Expression, out var parent))
                {
                    return false;
                }
                data = member.Name;
            }

            else if (methodCallExpression != null)
            {
                if (methodCallExpression.Method.Name == "Select" && methodCallExpression.Arguments.Count == 2)
                {
                    if (!TryGetParentName(methodCallExpression.Arguments[0], out var parent2)) return false;
                    if (methodCallExpression.Arguments[1] is LambdaExpression lambdaExpression)
                    {
                        if (!TryGetParentName(lambdaExpression.Body, out var data2))
                        {
                            return false;
                        }
                        data = data2;
                        return true;
                    }
                }
                return false;
            }
            else data = "";//(expression as LambdaExpression).Parameters[0].Name;

            return true;
        }

        public static bool TryGetParentType(Expression expression, out Type data)
        {
            data = null;
            var expression2 = RemoveConvert(expression);
            var methodCallExpression = expression2 as MethodCallExpression;
            if (expression2 is MemberExpression memberExpression)
            {
                var member = memberExpression.Member;

                if (!TryGetParentType(memberExpression.Expression, out var parent))
                {
                    return false;
                }
                data = memberExpression.Type;
            }

            else if (methodCallExpression != null)
            {
                if (methodCallExpression.Method.Name == "Select" && methodCallExpression.Arguments.Count == 2)
                {
                    if (!TryGetParentType(methodCallExpression.Arguments[0], out var parent2))
                    {
                        return false;
                    }
                    if (methodCallExpression.Arguments[1] is LambdaExpression lambdaExpression)
                    {
                        if (!TryGetParentType(lambdaExpression.Body, out var data2))
                        {
                            return false;
                        }
                        data = data2;
                        return true;
                    }
                }
                return false;
            }

            else data = expression.Type;

            return true;
        }

        public static Expression RemoveConvert(Expression expression)
        {
            while (expression.NodeType == ExpressionType.Convert || expression.NodeType == ExpressionType.ConvertChecked)
            {
                expression = ((UnaryExpression)expression).Operand;
            }
            return expression;
        }
    }
    public static class TypeHelper<T>
    {

        public static string NameOf<TProperty>(Expression<Func<T, TProperty>> path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (!TypeHelper.TryGetParentName(path.Body, out var data) || data == null)
            {
                throw new ArgumentException(nameof(path));
            }
            return data;
        }

        public static Type TypeOf<TProperty>(Expression<Func<T, TProperty>> path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (!TypeHelper.TryGetParentType(path.Body, out var data) || data == null)
            {
                throw new ArgumentException(nameof(path));
            }
            return data;
        }
    }
}
