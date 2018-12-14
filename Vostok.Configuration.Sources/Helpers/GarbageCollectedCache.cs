using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Commons.Threading;
using Vostok.Configuration.Sources.File;
using Vostok.Configuration.Sources.Watchers;

namespace Vostok.Configuration.Sources.Helpers
{
    internal class GarbageCollectedCache<TKey, TValue>
    {
        private readonly Func<KeyValuePair<TKey, TValue>, bool> garbageSelector;
        private readonly TimeSpan garbageCollectionPeriod;
        private readonly ConcurrentDictionary<TKey, TValue> cache = new ConcurrentDictionary<TKey, TValue>();
        private readonly AtomicBoolean taskIsRun;

        public GarbageCollectedCache(Func<KeyValuePair<TKey, TValue>, bool> garbageSelector, TimeSpan garbageCollectionPeriod)
        {
            this.garbageSelector = garbageSelector;
            this.garbageCollectionPeriod = garbageCollectionPeriod;
            taskIsRun = new AtomicBoolean(false);
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            if (taskIsRun.TrySetTrue())
                Task.Run(CollectGarbage);
                    
            return cache.GetOrAdd(key, valueFactory);
        }

        private async Task CollectGarbage()
        {
            while (true)
            {
                await Task.Delay(garbageCollectionPeriod).ConfigureAwait(false);
                foreach (var kv in cache.ToArray())
                {
                    if (garbageSelector(kv))
                        cache.TryRemove(kv.Key, out _);
                }
            }
        }
    }
}