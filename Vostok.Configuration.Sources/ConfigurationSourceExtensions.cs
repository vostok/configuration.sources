using System.Linq;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.Merging;
using Vostok.Configuration.Sources.Combined;
using Vostok.Configuration.Sources.Helpers;
using Vostok.Configuration.Sources.Scoped;

namespace Vostok.Configuration.Sources
{
    public static class ConfigurationSourceExtensions
    {
        public static IConfigurationSource ScopeTo(this IConfigurationSource source, params string[] scope) =>
            new ScopedSource(source, scope);

        public static IConfigurationSource Combine(this IConfigurationSource source, IConfigurationSource other, SettingsMergeOptions options = null) =>
            new CombinedSource(new[] {source, other}, options);

        public static IConfigurationSource Combine(this IConfigurationSource source, params IConfigurationSource[] others) =>
            new CombinedSource(source.ToEnumerable().Concat(others).ToArray());
    }
}