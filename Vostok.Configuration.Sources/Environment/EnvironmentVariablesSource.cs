using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.Constant;

namespace Vostok.Configuration.Sources.Environment
{
    /// <inheritdoc />
    /// <summary>
    /// Environment variables converter to <see cref="ISettingsNode"/> tree
    /// </summary>
    public class EnvironmentVariablesSource : BaseConstantSource
    {
        /// <inheritdoc />
        /// <summary>
        /// <para>Creates a <see cref="EnvironmentVariablesSource"/> instance.</para>
        /// </summary>
        public EnvironmentVariablesSource()
            : base(() => new EnvironmentVariablesConverter().Convert(System.Environment.GetEnvironmentVariables()))
        {
        }
    }
}