using System;
using System.Reactive.Linq;

namespace Vostok.Configuration.Sources.Helpers
{
    public static class ObservableExtensions
    {
        public static IObservable<(TResult value, Exception error)> SelectValueOrError<TSource, TResult>(this IObservable<TSource> source, Func<TSource, TResult> selector)
        {
            return source.Select(
                sourceValue =>
                {
                    try
                    {
                        return (selector(sourceValue), null as Exception);
                    }
                    catch (Exception e)
                    {
                        return (default, e);
                    }
                });
        }
        
        internal static SubscriptionsCounterAdapter<T> WithSubscriptionsCounter<T>(this IObservable<T> source)
        {
            return new SubscriptionsCounterAdapter<T>(source);
        }
    }
}