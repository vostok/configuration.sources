using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

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
            foreach (var kv in cache.ToArray()) // CR(krait): Why .ToArray()? It's safe to enumerate a ConcurrentDictionary while modifying it.
            {
                if (garbageSelector(kv))
                    cache.TryRemove(kv.Key, out _);
            }
            return cache.GetOrAdd(key, valueFactory);
        }
    }
}