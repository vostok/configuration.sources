using System;
using System.Reactive.Linq;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources.Scoped
{
    /// <summary>
    /// Searches subtree in <see cref="ISettingsNode"/> tree.
    /// </summary>
    public class ScopedSource : IConfigurationSource
    {
        private readonly IConfigurationSource source;
        private readonly string[] scope;

        /// <summary>
        /// Creates a <see cref="ScopedSource"/> instance for <see cref="source"/> to search in by <see cref="scope"/>
        /// <para>You can use "[n]" format in <see cref="scope"/> to get n-th index of list.</para>
        /// </summary>
        /// <param name="source">Source of <see cref="ISettingsNode"/> tree</param>
        /// <param name="scope">Search path</param>
        public ScopedSource(
            [NotNull] IConfigurationSource source,
            [NotNull] params string[] scope)
        {
            this.source = source;
            this.scope = scope;
        }

        public IObservable<(ISettingsNode settings, Exception error)> Observe() => 
            source.Observe().Select(pair => (pair.settings?.ScopeTo(scope), pair.error));
    }
}