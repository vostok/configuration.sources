using System;
using System.Reactive.Linq;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources.Scoped
{
    /// <summary>
    /// A source which returns settings from the underlying source scoped to the given scope.
    /// </summary>
    [PublicAPI]
    public class ScopedSource : IConfigurationSource
    {
        private readonly IConfigurationSource source;
        private readonly string[] scope;

        public ScopedSource(
            [NotNull] IConfigurationSource source,
            [NotNull] params string[] scope)
        {
            this.source = source ?? throw new ArgumentNullException(nameof(source));
            this.scope = scope ?? throw new ArgumentNullException(nameof(scope));
        }

        /// <inheritdoc />
        public IObservable<(ISettingsNode settings, Exception error)> Observe() =>
            source.Observe().Select(pair => (pair.settings?.ScopeTo(scope), pair.error));
    }
}
