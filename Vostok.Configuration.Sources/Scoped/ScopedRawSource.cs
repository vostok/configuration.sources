using System;
using System.Reactive.Linq;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources.Scoped
{
    internal class ScopedRawSource : IRawConfigurationSource
    {
        private readonly IConfigurationSource source;
        private readonly string[] scope;

        public ScopedRawSource(
            [NotNull] IConfigurationSource source,
            [NotNull] params string[] scope)
        {
            this.source = source;
            this.scope = scope;
        }

        public IObservable<(ISettingsNode settings, Exception error)> ObserveRaw()
        {
            return source.Observe().Select(pair => (pair.settings?.ScopeTo(scope), pair.error));
        }
    }
}