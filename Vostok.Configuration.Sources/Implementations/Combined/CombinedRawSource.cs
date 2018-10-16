using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.Merging;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources.Implementations.Combined
{
    /// <summary>
    /// A source that combines settings from several other sources, resolving possible conflicts in favor of sources which one came earlier in the list
    /// </summary>
    internal class CombinedRawSource : IRawConfigurationSource
    {
        private readonly IReadOnlyCollection<IConfigurationSource> sources;
        private readonly SettingsMergeOptions options;
        private IObservable<(ISettingsNode settings, Exception error)> observer;

        public CombinedRawSource(
            [NotNull] IReadOnlyCollection<IConfigurationSource> sources,
            SettingsMergeOptions options)
        {
            if (sources == null || sources.Count == 0)
                throw new ArgumentException($"{nameof(CombinedRawSource)}: {nameof(sources)} collection should not be empty");

            this.sources = sources;
            this.options = options;
        }

        /// <inheritdoc />
        /// <summary>
        /// <para>Subscribtion to see <see cref="ISettingsNode"/> changes in any of sources.</para>
        /// <para>Returns current value immediately on subscribtion.</para>
        /// </summary>
        /// <returns>Event with new RawSettings tree</returns>
        public IObservable<(ISettingsNode settings, Exception error)> ObserveRaw() => 
            observer ?? (observer = sources.Select(s => s.Observe()).CombineLatest().Select(l => l.Aggregate((a, b) => (a.settings.Merge(b.settings, options), null as Exception)))); // TODO(krait):  merge exceptions
    }
}