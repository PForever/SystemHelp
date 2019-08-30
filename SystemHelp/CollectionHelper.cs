using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemHelp
{
    public static class CollectionHelper
    {
        public static IEnumerable<T> Replace<T>(this IEnumerable<T> src, T oldValue, T newValue)
        {
            foreach (var item in src) yield return Equals(item, oldValue) ? newValue : item;
        }

        public static IDictionary<TKey, TValue> AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key, TValue value)
        {
            if (source.ContainsKey(key)) source[key] = value;
            else source.Add(key, value);
            return source;
        }
        public static T GetOrAdd<T>(this IList<T> source, int index, Func<T> factory)
        {
            if (source.Count <= index) for (int i = index - source.Count; i >= 0; i--) source.Add(factory());
            return source[index];
        }
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key, Func<TValue> factory)
        {
            if (!source.ContainsKey(key))
            {
                var value = factory();
                source.Add(key, value);
                return value;
            }
            return source[key];
        }
    }
}
