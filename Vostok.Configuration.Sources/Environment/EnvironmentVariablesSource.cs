using Vostok.Configuration.Sources.Constant;

namespace Vostok.Configuration.Sources.Environment
{
    /// <summary>
    /// <para>Parses settings from application's environment variables.</para>
    /// <para>Multi-level keys are supported, like A.B.C = D</para>
    /// </summary>
    public class EnvironmentVariablesSource : BaseConstantSource
    {
        /// <summary>
        /// <para>Creates an <see cref="EnvironmentVariablesSource" /> instance.</para>
        /// </summary>
        public EnvironmentVariablesSource()
            : base(() => EnvironmentVariablesConverter.Convert(System.Environment.GetEnvironmentVariables()))
        {
        }
    }
}