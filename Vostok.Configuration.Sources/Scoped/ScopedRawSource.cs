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
            // CR(krait): Wow, should it really work this way? Why does it ignore settings if error is set?
            return source.Observe()
                .Select(pair => pair.error == null ? (pair.settings?.ScopeTo(scope), null) : (null as ISettingsNode, pair.error)); // CR(krait): And also it can push (null, null). IIRC we allow this, but is all of our code ready for this?
        }
    }
}