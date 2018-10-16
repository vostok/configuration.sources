using System;
using System.Collections.Generic;
using System.Linq;

namespace Vostok.Configuration.Sources.SettingsTree.Mutable
{
    internal static class EnumerableExtensions
    {
        public static SortedDictionary<TKey, TElement> ToSortedDictionary<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            IComparer<TKey> comparer = null) =>
            new SortedDictionary<TKey, TElement>(source.ToDictionary(keySelector, elementSelector), comparer);
    }
}