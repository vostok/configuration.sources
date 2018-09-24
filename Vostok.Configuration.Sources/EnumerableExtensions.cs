using System.Collections.Generic;

namespace Vostok.Configuration.Sources
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> ToEnumerable<T>(this T item)
        {
            yield return item;
        }
    }
}