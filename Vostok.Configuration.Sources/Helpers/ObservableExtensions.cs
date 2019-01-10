using System;
using System.Reactive.Linq;
using JetBrains.Annotations;
using Vostok.Configuration.Sources.File;

namespace Vostok.Configuration.Sources.Helpers
{
    /// <summary>
    /// <para>Note: this class is intended to be used only by those implementing their own configuration sources.</para>
    /// <para>See <see cref="BaseFileSource"/> implementation for a usage example.</para>
    /// </summary>
    [PublicAPI]
    public static class ObservableExtensions
    {
        /// <summary>
        /// Returns (<paramref name="selector"/>(value), null) normally, and (null, error) in case the <paramref name="selector"/>(value) call fails.
        /// </summary>
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
        
        internal static SubscriptionsCounterAdapter<T> WithSubscriptionsCounter<T>(this IObservable<T> source) => new SubscriptionsCounterAdapter<T>(source);
    }
}