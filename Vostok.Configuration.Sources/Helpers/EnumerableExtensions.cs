using System.Collections.Generic;

namespace Vostok.Configuration.Sources.Helpers
{
    // CR(krait): Why public? Maybe move to commons.helpers or whatever?
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> ToEnumerable<T>(this T item)
        {
            yield return item;
        }
    }
}