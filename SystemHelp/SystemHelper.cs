using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;

namespace SystemHelp
{
    public static class SystemHelper
    {
        private static readonly ConcurrentDictionary<Type, Delegate> _dictionary = new ConcurrentDictionary<Type, Delegate>();
        public static void MemberCopy<T>(T src, T dst) => ((Action<T, T>)_dictionary.GetOrAdd(typeof(T), t => CreateCopyMethod<T>()))(src, dst);

        private static Action<T, T> CreateCopyMethod<T>()
        {
            var type = typeof(T);
            var srcExpression = Expression.Parameter(type);
            var dstExpression = Expression.Variable(type);
            var block = new List<Expression>();
            var props = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).Where(p => p.CanRead && p.CanWrite);
            foreach (var prop in props) block.Add(Expression.Assign(Expression.Property(dstExpression, prop), Expression.Property(srcExpression, prop)));
            var body = Expression.Block(block);
            return Expression.Lambda<Action<T, T>>(body, srcExpression, dstExpression).Compile();
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> first, T second)
        {
            foreach (T item in first) yield return item;
            yield return second;
        }

        public static Exception GetInnerException(Exception e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            e = e.InnerException;
            while (e.InnerException != null) e = e.InnerException;
            return e;
        }

        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> src, params (TKey Key, TValue Value)[] values)
        {
            foreach (var (key, value) in values) src.Add(key, value);
        }

        public static IEnumerable<T> Convert<T>(this IEnumerable src)
        {
            foreach (var o in src) yield return (T)o;
        }

        private static readonly Lazy<StringBuilder> StringBuilderFactory = new Lazy<StringBuilder>(() => new StringBuilder(), true);

        public static bool IsNullOrEmpty(this string s) => string.IsNullOrEmpty(s);
        public static bool IsFilled(this string s)
        {
            return !String.IsNullOrEmpty(s);
        }

        public static bool AllFilled(params string[] arr)
        {
            foreach (string s in arr)
            {
                if (!s.IsFilled()) return false;
            }
            return true;
        }

        public static bool AnyFilled(params string[] arr)
        {
            foreach (string s in arr)
            {
                if (s.IsFilled()) return true;
            }
            return false;
        }

        public static bool In<T>(this T node, params T[] values) => values.Contains(node);
        public static bool NotIn<T>(this T node, params T[] values) => !values.Contains(node);
        public static bool In<T>(this T node, IEnumerable<T> values) => values.Contains(node);
        public static bool NotIn<T>(this T node, IEnumerable<T> values) => !values.Contains(node);
        public static bool In<T>(this T node, IQueryable<T> values) => values.Contains(node);
        public static bool NotIn<T>(this T node, IQueryable<T> values) => !values.Contains(node);

        public static IEnumerable<T> SafeWhere<T>(this IEnumerable<T> src, Func<T, bool> predicate) => src.SafeWhere(predicate != null, predicate);
        public static IEnumerable<T> SafeWhere<T>(this IEnumerable<T> src, bool condition, Func<T, bool> predicate) => condition ? src.Where(predicate) : src;
        public static IQueryable<T> SafeWhere<T>(this IQueryable<T> src, Expression<Func<T, bool>> predicate) => src.SafeWhere(predicate != null, predicate);
        public static IQueryable<T> SafeWhere<T>(this IQueryable<T> src, bool condition, Expression<Func<T, bool>> predicate) => condition ? src.Where(predicate) : src;

        [Conditional("DEBUG")]
        public static void ThrowIfDebug<T>(this T e) where T : Exception => throw e;
        [Conditional("DEBUG")]
        public static void InfoIfDebug(string message, [CallerFilePath] string path = "", [CallerMemberName] string method = "", [CallerLineNumber] int line = -1)
        {
            using (var writer = new StringWriter())
            {
                writer.WriteLine($"Path: {path}");
                writer.WriteLine($"Method: {method}");
                writer.WriteLine($"Line: {line}");
                writer.WriteLine($"Message: \"{message}\"");
                Debug.WriteLine(writer.ToString());
            }
        }
        public static string ToInv(this DateTime date) => date.ToString(CultureInfo.InvariantCulture);
        public static string ToInvShort(this DateTime date) => date.ToString("d", CultureInfo.InvariantCulture);
        public static string ToCur(this DateTime date) => date.ToString(CultureInfo.CurrentCulture);
        private static readonly bool _isInvariant = Equals(CultureInfo.CurrentCulture, CultureInfo.InvariantCulture);
        public static string InvToCur(this string value) => _isInvariant ? value : value.InvToDate()?.ToCur();

        public static DateTime? InvToDate(this string value)
        {
            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime datetime))
                return datetime;
            return null;
        }

        public static IList<T> With<T>(this IList<T> list, T item)
        {
            list.Add(item);
            return list;
        }
        public static IList<T> With<T>(this IList<T> list, T item, int position)
        {
            list.Insert(position, item);
            return list;
        }
        public static IList<T> With<T>(this IList<T> list, IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                list.Add(item);
            }
            return list;
        }
        public static IList<T> With<T>(this IList<T> list, params T[] items)
        {
            foreach (T item in items)
            {
                list.Add(item);
            }
            return list;
        }
        public static IList<T> With<T>(this IList<T> list, params (T Item, bool IsNeed)[] items)
        {
            foreach (var (item, isNeed) in items)
            {
                if (!isNeed) continue;
                list.Add(item);
            }
            return list;
        }
        public static T ParsTo<T>(this object obj)
        {
            if (obj is T res) return res;
            throw new UnexpectedTypeException(obj, typeof(T));
        }
        public static bool TryParsTo<T>(this object obj, out T result)
        {
            if (obj is T res)
            {
                result = res;
                return true;
            }
            result = default(T);
            return false;
        }

        private static readonly ConcurrentDictionary<Type, Delegate> _cashedConverters = new ConcurrentDictionary<Type, Delegate>();
        public static IEnumerable<TResult> To<TSource, TResult>(this IEnumerable<TSource> src)
        {
            var typeFunc = typeof(Func<TSource, TResult>);
            Func<TSource, TResult> lambda;
            IEnumerable<TResult> Worker()
            {
                foreach (var item in src)
                {
                    yield return lambda.Invoke(item);
                }
            }
            if (_cashedConverters.ContainsKey(typeFunc))
            {
                lambda = (Func<TSource, TResult>)_cashedConverters[typeFunc];
                return Worker();
            }
            var typeDest = typeof(TResult);
            var typeSrc = typeof(TSource);
            var ctrInfo = typeDest.GetConstructor(new[] { typeof(TSource) });
            if (ctrInfo == null) throw new ArgumentException($"Type {typeDest.Name} has no constructor within type {typeSrc.Name}");
            var srcParam = Expression.Parameter(typeSrc);
            var ctr = Expression.New(ctrInfo, srcParam);
            lambda = Expression.Lambda<Func<TSource, TResult>>(ctr, srcParam).Compile();
            _cashedConverters.TryAdd(typeFunc, lambda);

            return Worker();
        }

        public static IEnumerable<T> LazyForEach<T>(this IEnumerable<T> src, Action<T> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            IEnumerable<T> ForEach()
            {
                foreach (T item in src)
                {
                    action(item);
                    yield return item;
                }
            }
            return ForEach();
        }
        public static void ForEach<T>(this IEnumerable<T> src, Action<T> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            foreach (T item in src)
            {
                action(item);
            }
        }
    }
}
