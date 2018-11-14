using System.Collections.Generic;
using System.Linq;
using Microsoft.Reactive.Testing;

namespace Vostok.Configuration.Sources.Tests.Helpers
{
    internal static class TestableObserverExtensions
    {
        public static IEnumerable<T> GetValues<T>(this ITestableObserver<T> observer)
        {
            return observer.Messages.Select(received => received.Value.Value);
        }
    }
}