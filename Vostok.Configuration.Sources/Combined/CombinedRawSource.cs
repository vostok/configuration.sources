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
    internal class CombinedRawSource : IRawConfigurationSource
    {
        private readonly IReadOnlyCollection<IConfigurationSource> sources;
        private readonly SettingsMergeOptions options;
        private IObservable<(ISettingsNode settings, Exception error)> observer; // CR(krait): But it's an observable, not observer..

        public CombinedRawSource(params IConfigurationSource[] sources)
            : this(sources, new SettingsMergeOptions())
        {
        }

        public CombinedRawSource(
            [NotNull] IReadOnlyCollection<IConfigurationSource> sources, // CR(krait): And [ItemNotNull], probably?
            SettingsMergeOptions options)
        {
            if (sources == null || sources.Count == 0)
                throw new ArgumentException($"{nameof(CombinedRawSource)}: {nameof(sources)} collection should not be empty");

            this.sources = sources;
            this.options = options;
        }

        /// <inheritdoc />
        /// <summary>
        ///     <para>Subscribtion to see <see cref="ISettingsNode" /> changes in any of sources.</para>
        ///     <para>Returns current value immediately on subscribtion.</para>
        /// </summary>
        /// <returns>Event with new RawSettings tree</returns>
        public IObservable<(ISettingsNode settings, Exception error)> ObserveRaw()
        {
            if (observer != null)
                return observer;

            observer = sources
                .Select(s => s.Observe())
                .CombineLatest()
                .Select(
                    l =>
                    {
                        var error = MergeErrors(l);
                        var settings = error == null ? MergeSettings(l) : null;
                        return (settings, error);
                    });
            return observer;
        }

        private static Exception MergeErrors(IEnumerable<(ISettingsNode settings, Exception error)> values)
        {
            // CR(krait): And if there's only one error?
            var errors = values.Select(pair => pair.error).Where(error => error != null).ToArray();
            return errors.Length > 0 ? new AggregateException(errors) : null;
        }

        private ISettingsNode MergeSettings(IEnumerable<(ISettingsNode settings, Exception error)> values)
        {
            // CR(krait): What will happen if a pair (null, null) is pushed?
            return values.Select(pair => pair.settings).Aggregate((a, b) => a.Merge(b, options));
        }
    }
}