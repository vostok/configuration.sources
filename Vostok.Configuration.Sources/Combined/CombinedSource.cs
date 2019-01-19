using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.Merging;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources.Combined
{
    /// <summary>
    /// <para>Returns settings combined from all settings returned by the provided sources.</para>
    /// <para>Order of the sources is important: settings from sources that come later in the array have greater priority.</para>
    /// <para>New settings are pushed to subscribers each time one of the sources generates new settings.</para>
    /// </summary>
    [PublicAPI]
    public class CombinedSource : IConfigurationSource
    {
        private readonly IEnumerable<IConfigurationSource> sources;
        private readonly SettingsMergeOptions options;

        public CombinedSource(params IConfigurationSource[] sources)
            : this(sources, null)
        {
        }

        public CombinedSource(
            [NotNull] [ItemNotNull] IEnumerable<IConfigurationSource> sources,
            [CanBeNull] SettingsMergeOptions options)
        {
            if (sources == null || !sources.Any())
                throw new ArgumentException($"{nameof(CombinedSource)}: {nameof(sources)} collection should not be empty");

            this.sources = sources;
            this.options = options;
        }

        /// <inheritdoc />
        public IObservable<(ISettingsNode settings, Exception error)> Observe()
        {
            return sources
                .Select(s => s.Observe())
                .CombineLatest()
                .Select(list => (MergeSettings(list), MergeErrors(list)));
        }

        private static Exception MergeErrors(IEnumerable<(ISettingsNode settings, Exception error)> values)
        {
            var errors = values.Select(pair => pair.error).Where(error => error != null).ToArray();
            return errors.Length > 1 ? new AggregateException(errors) : errors.FirstOrDefault();
        }

        private ISettingsNode MergeSettings(IEnumerable<(ISettingsNode settings, Exception error)> values) => 
            values.Select(pair => pair.settings).Aggregate((a, b) => SettingsNodeMerger.Merge(a, b, options));
    }
}