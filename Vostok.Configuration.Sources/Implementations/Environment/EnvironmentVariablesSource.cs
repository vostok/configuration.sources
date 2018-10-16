using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources.Implementations.Environment
{
    /// <inheritdoc />
    /// <summary>
    /// Environment variables converter to <see cref="ISettingsNode"/> tree
    /// </summary>
    public class EnvironmentVariablesSource : ConfigurationSourceAdapter
    {
        /// <inheritdoc />
        /// <summary>
        /// <para>Creates a <see cref="EnvironmentVariablesRawSource"/> instance.</para>
        /// </summary>
        public EnvironmentVariablesSource()
            : base(new EnvironmentVariablesRawSource())
        {
        }
    }
}