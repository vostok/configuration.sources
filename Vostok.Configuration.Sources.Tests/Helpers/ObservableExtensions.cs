using System;
using System.Linq;
using System.Reactive.Linq;

namespace Vostok.Configuration.Sources.Tests.Helpers
{
    internal static class ObservableExtensions
    {
        public static T WaitFirstValue<T>(this IObservable<T> observable, TimeSpan timeout)
        {
            return observable.Buffer(timeout, 1)
                .ToEnumerable()
                .First()
                .First();
        }
    }
}