using System.Collections.Generic;

namespace Vostok.Configuration.Sources.Helpers
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> ToEnumerable<T>(this T item)
        {
            yield return item;
        }
    }
}