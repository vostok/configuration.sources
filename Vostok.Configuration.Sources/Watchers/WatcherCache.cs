using System;
using System.Reactive.Linq;
using JetBrains.Annotations;
using Vostok.Configuration.Sources.File;
using Vostok.Configuration.Sources.Helpers;

namespace Vostok.Configuration.Sources.Watchers
{
    /// <summary>
    /// <para>Note: this class is intended to be used only by those implementing their own configuration sources.</para>
    /// <para>A helper class that implements a cache of generic settings watchers with in-built garbage collection.</para>
    /// <para>See <see cref="BaseFileSource"/> implementation for a usage example.</para>
    /// </summary>
    [PublicAPI]
    public class WatcherCache<TSettings, TValue>
    {
        private readonly IWatcherFactory<TSettings, TValue> watcherFactory;
        private readonly GarbageCollectedCache<TSettings, SubscriptionsCounterAdapter<(TValue value, Exception error)>> watchers =
            new GarbageCollectedCache<TSettings, SubscriptionsCounterAdapter<(TValue value, Exception error)>>(
                kv => kv.Value.SubscriptionsCount == 0);

        public WatcherCache(IWatcherFactory<TSettings, TValue> watcherFactory)
        {
            this.watcherFactory = watcherFactory;
        }

        public IObservable<(TValue value, Exception error)> Watch(TSettings settings)
        {
            return watchers.GetOrAdd(settings, s => watcherFactory.CreateWatcher(s).Replay(1).RefCount().WithSubscriptionsCounter());
        }
    }
}