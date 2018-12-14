using System;
using System.Collections.Concurrent;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources
{
    public class SingletonConfigurationSource : IConfigurationSource
    {
        private static ConcurrentDictionary<(Type, object), IConfigurationSource> implementations = new ConcurrentDictionary<(Type, object), IConfigurationSource>();

        private IConfigurationSource currentImplementation;

        protected SingletonConfigurationSource(object key, Func<IConfigurationSource> constructor)
        {
            var typedKey = (GetType(), key);
            currentImplementation = implementations.GetOrAdd(typedKey, _ => constructor());
        }

        public IObservable<(ISettingsNode settings, Exception error)> Observe() =>
            currentImplementation.Observe();

        internal static void ClearCache() =>
            implementations.Clear();
    }
}