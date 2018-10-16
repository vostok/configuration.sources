using JetBrains.Annotations;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources.Implementations.Scoped
{
    /// <summary>
    /// Searches subtree in <see cref="ISettingsNode"/> tree.
    /// </summary>
    public class ScopedSource : ConfigurationSourceAdapter
    {
        /// <summary>
        /// Creates a <see cref="ScopedSource"/> instance for <see cref="source"/> to search in by <see cref="scope"/>
        /// <para>You can use "[n]" format in <see cref="scope"/> to get n-th index of list.</para>
        /// </summary>
        /// <param name="source">Source of <see cref="ISettingsNode"/> tree</param>
        /// <param name="scope">Search path</param>
        public ScopedSource(
            [NotNull] IConfigurationSource source,
            [NotNull] params string[] scope)
            : base(new ScopedRawSource(source, scope))
        {
        }

        /// <summary>
        /// <para>Creates a <see cref="ScopedSource"/> instance for <see cref="settings"/> to search in by <see cref="scope"/></para> 
        /// <para>You can use "[n]" format in <see cref="scope"/> to get n-th index of list.</para>
        /// </summary>
        /// <param name="settings">Tree to search in</param>
        /// <param name="scope">Search path</param>
        public ScopedSource(
            [NotNull] ISettingsNode settings,
            [NotNull] params string[] scope)
            : base(new ScopedRawSource(settings, scope))
        {
        }
    }
}