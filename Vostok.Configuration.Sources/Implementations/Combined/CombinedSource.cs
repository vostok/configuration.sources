using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.Merging;

namespace Vostok.Configuration.Sources.Implementations.Combined
{
    public class CombinedSource : ConfigurationSourceAdapter
    {
        /// <summary>
        /// <para>Creates a <see cref="CombinedRawSource"/> instance new source using combining options.</para>
        /// <para>Combines sources here.</para>
        /// </summary>
        /// <param name="sources">Configuration sources to combine</param>
        /// <param name="options"></param>
        public CombinedSource(
            [NotNull] IReadOnlyCollection<IConfigurationSource> sources,
            SettingsMergeOptions options)
            : base(new CombinedRawSource(sources, options))
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// <para>Creates a <see cref="CombinedRawSource" /> instance new source using default combining options</para>
        /// <para>Combines sources here.</para>
        /// </summary>
        /// <param name="sources">Configurations</param>
        public CombinedSource(params IConfigurationSource[] sources)
            : this(sources, new SettingsMergeOptions())
        {
        }
    }
}