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
    /// <para>Order of the sources is important.</para>
    /// <para>New settings are pushed to subscribers each time one of the sources generates new settings.</para>
    /// </summary>
    [PublicAPI]
    public class CombinedSource : IConfigurationSource
    {
        private readonly IReadOnlyCollection<IConfigurationSource> sources;
        private readonly SettingsMergeOptions options;

        public CombinedSource(params IConfigurationSource[] sources)
            : this(sources, new SettingsMergeOptions())
        {
        }

        public CombinedSource(
            [NotNull] [ItemNotNull] IReadOnlyCollection<IConfigurationSource> sources,
            SettingsMergeOptions options)
        {
            if (sources == null || sources.Count == 0)
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
            values.Select(pair => pair.settings).Aggregate((a, b) => a?.Merge(b, options));
    }
}