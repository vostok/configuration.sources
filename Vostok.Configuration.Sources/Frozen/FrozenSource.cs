using System;
using System.Reactive.Linq;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources.Frozen
{
    /// <summary>
    /// <para>A source wrapper that effectively "freezes" the underlying source after receiving first settings.</para>
    /// <para>Passes any errors coming before first settings are observed (doesn't cache errors forever).</para>
    /// <para>Note that first settings mentioned above are such in the sense of coming first since subscription, not since creation of the source itself.</para>
    /// </summary>
    [PublicAPI]
    public class FrozenSource : IConfigurationSource
    {
        private readonly IConfigurationSource source;

        public FrozenSource([NotNull] IConfigurationSource source)
            => this.source = source ?? throw new ArgumentNullException(nameof(source));

        public IObservable<(ISettingsNode settings, Exception error)> Observe()
            => source.Observe().TakeUntil(pair => pair.settings != null);
    }
}
