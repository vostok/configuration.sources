using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Vostok.Configuration.Sources.Extensions.Observable
{
    /// <summary>
    /// Exposes some useful extensions methods from Rx.
    /// </summary>
    [PublicAPI]
    public static class ObservableExtensions
    {
        /// <summary>
        /// Projects each element of an observable sequence into a new form.
        /// </summary>
        [NotNull]
        public static IObservable<TResult> Select<TSource, TResult>([NotNull] this IObservable<TSource> source, [NotNull] Func<TSource, TResult> selector)
            => System.Reactive.Linq.Observable.Select(source, selector);

        /// <summary>
        /// Filters the elements of an observable sequence based on a predicate.
        /// </summary>
        [NotNull]
        public static IObservable<TSource> Where<TSource>([NotNull] this IObservable<TSource> source, [NotNull] Func<TSource, bool> predicate)
            => System.Reactive.Linq.Observable.Where(source, predicate);

        /// <summary>
        /// Returns an observable sequence that contains only distinct contiguous elements.
        /// </summary>
        public static IObservable<TSource> DistinctUntilChanged<TSource>(this IObservable<TSource> source)
            => System.Reactive.Linq.Observable.DistinctUntilChanged(source);

        /// <summary>
        /// Returns an observable sequence that contains only distinct contiguous elements.
        /// </summary>
        public static IObservable<TSource> DistinctUntilChanged<TSource, TKey>(this IObservable<TSource> source, Func<TSource, TKey> keySelector)
            => System.Reactive.Linq.Observable.DistinctUntilChanged(source, keySelector);
    }
}
