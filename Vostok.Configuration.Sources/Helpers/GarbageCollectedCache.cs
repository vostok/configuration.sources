using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Commons.Threading;
using Vostok.Configuration.Sources.File;

namespace Vostok.Configuration.Sources.Helpers
{
    internal class GarbageCollectedCache<TKey, TValue>
    {
        private readonly Func<KeyValuePair<TKey, TValue>, bool> garbageSelector;
        private readonly ConcurrentDictionary<TKey, TValue> cache = new ConcurrentDictionary<TKey, TValue>();

        public GarbageCollectedCache(Func<KeyValuePair<TKey, TValue>, bool> garbageSelector)
        {
            this.garbageSelector = garbageSelector;
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            foreach (var kv in cache.ToArray())
            {
                if (garbageSelector(kv))
                    cache.TryRemove(kv.Key, out _);
            }
            return cache.GetOrAdd(key, valueFactory);
        }
    }
}