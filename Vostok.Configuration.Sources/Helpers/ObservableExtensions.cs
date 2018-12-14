using System;

namespace Vostok.Configuration.Sources.Helpers
{
    internal static class ObservableExtensions
    {
        public static SubscriptionsCounterAdapter<T> WithSubscriptionsCounter<T>(this IObservable<T> source)
        {
            return new SubscriptionsCounterAdapter<T>(source);
        }
    }
}