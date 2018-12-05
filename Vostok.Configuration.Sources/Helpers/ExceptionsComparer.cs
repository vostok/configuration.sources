using System;
using System.Collections.Generic;
using System.Linq;

namespace Vostok.Configuration.Sources.Helpers
{
    internal class ExceptionsComparer : IEqualityComparer<Exception>
    {
        private static readonly IEqualityComparer<Exception> ComparerInstance = new ExceptionsComparer();
        
        public static bool Equals(Exception e1, Exception e2) => ComparerInstance.Equals(e1, e2);
        
        bool IEqualityComparer<Exception>.Equals(Exception e1, Exception e2)
        { 
            return e1 == e2 ||
                e1 != null && e2 != null &&
                ToTuple(e1).Equals(ToTuple(e2)) &&
                GetInnerExceptions(e1).SequenceEqual(GetInnerExceptions(e2), this);
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
            return exception.InnerException.ToEnumerable();
        }

    }
}