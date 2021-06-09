using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using JetBrains.Annotations;
using Vostok.Commons.Collections;
using Vostok.Commons.Helpers.Rx;
using Vostok.Configuration.Sources.File;

namespace Vostok.Configuration.Sources.Watchers
{
    /// <summary>
    /// <para>Note: this class is intended to be used only by those implementing their own configuration sources.</para>
    /// <para>A helper class that implements a cache of generic settings watchers with in-built garbage collection.</para>
    /// <para>See <see cref="FileSource"/> implementation for a usage example.</para>
    /// </summary>
    [PublicAPI]
    public class WatcherCache<TKey, TValue>
    {
        private readonly IWatcherFactory<TKey, TValue> factory;
        private readonly ConcurrentDictionary<TKey, CountingAdapter> cache;
        static WatcherCache() => RxHacker.Hack();

        public WatcherCache(
            [NotNull] IWatcherFactory<TKey, TValue> factory,
            [NotNull] IEqualityComparer<TKey> comparer)
        {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));

            cache = new ConcurrentDictionary<TKey, CountingAdapter>(comparer ?? throw new ArgumentNullException(nameof(comparer)));
        }

        public WatcherCache(IWatcherFactory<TKey, TValue> factory)
            : this(factory, EqualityComparer<TKey>.Default)
        {
        }

        /// <summary>
        /// Obtain a settings watcher for the given <paramref name="key"/>.
        /// </summary>
        public IObservable<(TValue value, Exception error)> Watch(TKey key)
        {
            return cache.GetOrAdd(key, CreateAdapter);
        }

        private CountingAdapter CreateAdapter(TKey key)
        {
            return new CountingAdapter(key, () => factory.CreateWatcher(key).Replay(1).RefCount(), cache);
        }

        private class CountingAdapter : IObservable<(TValue, Exception)>
        {
            private readonly TKey key;
            private readonly Lazy<IObservable<(TValue, Exception)>> source;
            private readonly ConcurrentDictionary<TKey, CountingAdapter> cache;
            private int subscriptions;

            public CountingAdapter(
                TKey key,
                Func<IObservable<(TValue, Exception)>> source,
                ConcurrentDictionary<TKey, CountingAdapter> cache)
            {
                this.key = key;
                this.cache = cache;
                this.source = new Lazy<IObservable<(TValue, Exception)>>(source, LazyThreadSafetyMode.ExecutionAndPublication);
            }

            public IDisposable Subscribe(IObserver<(TValue, Exception)> observer)
            {
                var sourceSubcription = source.Value.Subscribe(observer);

                Interlocked.Increment(ref subscriptions);

                return new CompositeDisposable(sourceSubcription, Disposable.Create(Unsubscribe));
            }

            private void Unsubscribe()
            {
                if (Interlocked.Decrement(ref subscriptions) == 0)
                    cache.Remove(key, this);
            }
        }
    }
}