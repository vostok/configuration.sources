using System.Linq;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.Merging;
using Vostok.Configuration.Sources.Combined;
using Vostok.Configuration.Sources.Helpers;
using Vostok.Configuration.Sources.Scoped;

namespace Vostok.Configuration.Sources
{
    /// <summary>
    /// Extension methods for <see cref="IConfigurationSource"/>.
    /// </summary>
    [PublicAPI]
    public static class ConfigurationSourceExtensions
    {
        /// <summary>
        /// Wraps the provided <paramref name="source"/> into a <see cref="ScopedSource"/> with given <paramref name="scope"/>.
        /// </summary>
        public static IConfigurationSource ScopeTo(this IConfigurationSource source, params string[] scope) =>
            new ScopedSource(source, scope);

        /// <summary>
        /// Returns a new <see cref="CombinedSource"/> constructed using given <see cref="IConfigurationSource"/>s.
        /// </summary>
        public static IConfigurationSource Combine(this IConfigurationSource source, IConfigurationSource other, SettingsMergeOptions options = null) =>
            new CombinedSource(new[] {source, other}, options);

        /// <inheritdoc cref="Combine(IConfigurationSource,IConfigurationSource,SettingsMergeOptions)"/>
        public static IConfigurationSource Combine(this IConfigurationSource source, params IConfigurationSource[] others) =>
            new CombinedSource(source.ToEnumerable().Concat(others).ToArray());
    }
}