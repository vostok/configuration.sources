using System;
using System.Collections.Generic;
using System.Linq;

namespace Vostok.Configuration.Sources.Helpers
{
    internal class ExceptionsComparer : IEqualityComparer<Exception> // CR(krait): Where are the tests?
    {
        public static bool Equals(Exception e1, Exception e2)
        {
            return e1 == e2 ||
                   e1 != null && e2 != null &&
                   ToTuple(e1).Equals(ToTuple(e2)) &&
                   GetInnerExceptions(e1).SequenceEqual(GetInnerExceptions(e2), new ExceptionsComparer()); // CR(krait): Use a singleton instance instead of new ExceptionsComparer().
        }

        public int GetHashCode(Exception obj) => ToTuple(obj).GetHashCode();

        private static (Type, string) ToTuple(Exception ex)
        {
            return (ex.GetType(), ex.Message);
        }

        private static IEnumerable<Exception> GetInnerExceptions(Exception exception)
        {
            if (exception is AggregateException aggregateException)
                return aggregateException.InnerExceptions;
            return new[] {exception.InnerException}; // CR(krait): Use .ToEnumerable()
        }

        bool IEqualityComparer<Exception>.Equals(Exception x, Exception y) => Equals(x, y);
    }
}