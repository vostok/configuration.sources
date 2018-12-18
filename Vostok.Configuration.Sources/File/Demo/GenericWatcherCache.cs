using System;
using System.Reactive.Linq;
using Vostok.Configuration.Sources.Helpers;

namespace Vostok.Configuration.Sources.File
{
    internal class GenericWatcherCache<TSettings, TValue>
    {
        private readonly IWatcherFactory<TSettings, TValue> watcherFactory;
        private readonly GarbageCollectedCache<TSettings, SubscriptionsCounterAdapter<(TValue value, Exception error)>> watchers =
            new GarbageCollectedCache<TSettings, SubscriptionsCounterAdapter<(TValue value, Exception error)>>(
                kv => kv.Value.SubscriptionsCount == 0);

        public GenericWatcherCache(IWatcherFactory<TSettings, TValue> watcherFactory)
        {
            this.watcherFactory = watcherFactory;
        }

        public IObservable<(TValue value, Exception error)> Watch(TSettings settings)
        {
            return watchers.GetOrAdd(settings, s => watcherFactory.CreateWatcher(s).Replay(1).RefCount().WithSubscriptionsCounter());
        }
    }
}