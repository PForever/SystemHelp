using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SystemHelp
{
    public static class NonTypedCollectionHelper
    {
        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> src, LambdaExpression lambda, ListSortDirection direction = ListSortDirection.Ascending)
        {
            var typeResult = lambda.ReturnType;
            var orderby = GetOrder<T>(typeResult, direction);
            return (IOrderedQueryable<T>)orderby.Invoke(null, new object[] { src, lambda });
        }
        public static IOrderedQueryable OrderBy(this IQueryable src, Type type, LambdaExpression lambda, ListSortDirection direction = ListSortDirection.Ascending)
        {
            var typeResult = lambda.ReturnType;
            var orderby = GetOrder(type, typeResult, direction);
            return (IOrderedQueryable)orderby.Invoke(null, new object[] { src, lambda });
        }

        private static MethodInfo GetOrder<T>(Type typeResult, ListSortDirection direction) => GetOrder(typeof(T), typeResult, direction);

        private static MethodInfo GetOrder(Type typeSrc, Type typeResult, ListSortDirection direction)
        {
            string name;
            switch (direction)
            {
                case ListSortDirection.Ascending:
                    name = nameof(Queryable.OrderBy);
                    break;
                case ListSortDirection.Descending:
                    name = nameof(Queryable.OrderByDescending);
                    break;
                default: throw new NotSupportedException(direction.ToString());
            }
            return _methodDictionary2.GetOrAdd((name, typeSrc, typeResult, 2), MethodFactory);
        }
        private static MethodInfo GetThen<T>(Type typeResult, ListSortDirection direction) => GetThen(typeof(T), typeResult, direction);

        private static MethodInfo GetThen(Type typeSrc, Type typeResult, ListSortDirection direction)
        {
            string name;
            switch (direction)
            {
                case ListSortDirection.Ascending:
                    name = nameof(Queryable.ThenBy);
                    break;
                case ListSortDirection.Descending:
                    name = nameof(Queryable.ThenByDescending);
                    break;
                default: throw new NotSupportedException(direction.ToString());
            }
            return _methodDictionary2.GetOrAdd((name, typeSrc, typeResult, 2), MethodFactory);
        }

        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> src, params (LambdaExpression Selector, ListSortDirection Direction)[] lambda)
        {
            var l = lambda[0];
            var s = src.OrderBy(l.Selector, l.Direction);
            for (int i = 1; i < lambda.Length; i++)
            {
                l = lambda[i];
                s = s.ThenBy(l.Selector, l.Direction);
            }
            return s;
        }

        public static IOrderedQueryable OrderBy(this IQueryable src, Type type, params (LambdaExpression Selector, ListSortDirection Direction)[] lambda)
        {
            var l = lambda[0];
            var s = src.OrderBy(type, l.Selector, l.Direction);
            for (int i = 1; i < lambda.Length; i++)
            {
                l = lambda[i];
                s = s.ThenBy(type, l.Selector, l.Direction);
            }
            return s;
        }
        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> src, IEnumerable<(LambdaExpression Selector, ListSortDirection Direction)> lambda)
        {
            return OrderBy(src, lambda.ToArray());
        }
        public static IOrderedQueryable OrderBy(this IQueryable src, Type type, IEnumerable<(LambdaExpression Selector, ListSortDirection Direction)> lambda)
        {
            return OrderBy(src, type, lambda.ToArray());
        }

        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> src, LambdaExpression lambda, ListSortDirection direction)
        {
            var typeResult = lambda.ReturnType;
            var thenBy = GetThen<T>(typeResult, direction);
            return (IOrderedQueryable<T>)thenBy.Invoke(null, new object[] { src, lambda });
        }
        public static IOrderedQueryable ThenBy(this IOrderedQueryable src, Type type, LambdaExpression lambda, ListSortDirection direction)
        {
            var typeResult = lambda.ReturnType;
            var thenBy = GetThen(type, typeResult, direction);
            return (IOrderedQueryable)thenBy.Invoke(null, new object[] { src, lambda });
        }

        public static IQueryable Take(this IQueryable src, Type type, int count) => (IQueryable)Invoke(nameof(Queryable.Take), type, src, count);
        public static IQueryable Skip(this IQueryable src, Type type, int count) => (IQueryable)Invoke(nameof(Queryable.Skip), type, src, count);
        public static IQueryable Where(this IQueryable src, Type type, LambdaExpression predicate) => (IQueryable)Invoke(nameof(Queryable.Where), type, src, predicate);
        public static int Count(this IQueryable src, Type type) => (int)Invoke(nameof(Queryable.Count), type, src);

        private static object Invoke(string name, Type type, params object[] args)
        {
            var method = _methodDictionary.GetOrAdd((name, type, args.Length), MethodFactory);
            return method.Invoke(null, args);
        }
        private static object InvokeThis(string name, Type type, params object[] args)
        {
            var method = _methodDictionary.GetOrAdd((name, type, args.Length), MethodFactoryThis);
            return method.Invoke(null, args);
        }
        private static MethodInfo MethodFactory((string name, Type src, Type dst, int len) key)
        {
            return typeof(Queryable).GetMethods(BindingFlags.Static | BindingFlags.Public).First(m => m.Name == key.name && m.GetParameters().Length == key.len).MakeGenericMethod(new[] { key.src, key.dst });
        }
        private static MethodInfo MethodFactory((string name, Type generic, int len) key)
        {
            return typeof(Queryable).GetMethods(BindingFlags.Static | BindingFlags.Public).First(m => m.Name == key.name && m.GetParameters().Length == key.len).MakeGenericMethod(new[] { key.generic });
        }

        private static MethodInfo MethodFactoryThis((string name, Type generic, int len) key)
        {
            return typeof(ExpressionLogicOperations).GetMethods(BindingFlags.Static | BindingFlags.Public).First(m => m.Name == key.name && m.GetParameters().Length == key.len).MakeGenericMethod(new[] { key.generic });
        }

        private static ConcurrentDictionary<(string Name, Type Generic, int Len), MethodInfo> _methodDictionary = new ConcurrentDictionary<(string Name, Type Generic, int Len), MethodInfo>();
        private static ConcurrentDictionary<(string Name, Type Src, Type Dst, int Len), MethodInfo> _methodDictionary2 = new ConcurrentDictionary<(string Name, Type Generic, Type Dst, int Len), MethodInfo>();

    }
}
