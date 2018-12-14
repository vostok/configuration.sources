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
    ///     A source that combines settings from several other sources, resolving possible conflicts in favor of sources which
    ///     one came earlier in the list
    /// </summary>
    public class CombinedSource : IConfigurationSource
    {
        private readonly IReadOnlyCollection<IConfigurationSource> sources;
        private readonly SettingsMergeOptions options;

        /// <summary>
        ///     <para>Creates a <see cref="CombinedSource" /> instance new source using combining options.</para>
        ///     <para>Combines sources here.</para>
        /// </summary>
        /// <param name="sources">Configuration sources to combine</param>
        /// <param name="options"></param>
        public CombinedSource(params IConfigurationSource[] sources)
            : this(sources, new SettingsMergeOptions())
        {
        }

        /// <inheritdoc />
        /// <summary>
        ///     <para>Creates a <see cref="CombinedSource" /> instance new source using default combining options</para>
        ///     <para>Combines sources here.</para>
        /// </summary>
        /// <param name="sources">Configurations</param>
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
        /// <summary>
        ///     <para>Subscribtion to see <see cref="ISettingsNode" /> changes in any of sources.</para>
        ///     <para>Returns current value immediately on subscribtion.</para>
        /// </summary>
        /// <returns>Event with new RawSettings tree</returns>
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