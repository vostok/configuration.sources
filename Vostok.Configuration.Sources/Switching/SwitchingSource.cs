using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources.Switching
{
    /// <summary>
    /// A source that proxies from an underlying source that can be dynamically swapped to another one without resubscribing.
    /// </summary>
    [PublicAPI]
    public class SwitchingSource : IConfigurationSource
    {
        private readonly ReplaySubject<IConfigurationSource> sources = new ReplaySubject<IConfigurationSource>(1);

        private volatile IConfigurationSource currentSource;

        public SwitchingSource([NotNull] IConfigurationSource initialSource)
            => SwitchTo(initialSource);

        public IConfigurationSource CurrentSource => currentSource;

        public void SwitchTo(IConfigurationSource newSource)
            => sources.OnNext(currentSource = newSource);

        public void SwitchTo(Func<IConfigurationSource, IConfigurationSource> transform)
            => SwitchTo(transform(currentSource));

        public IObservable<(ISettingsNode settings, Exception error)> Observe()
            => sources.Select(source => source.Observe()).Switch();
    }
}
