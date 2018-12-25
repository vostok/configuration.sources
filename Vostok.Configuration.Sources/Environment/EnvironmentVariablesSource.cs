using Vostok.Configuration.Sources.Constant;

namespace Vostok.Configuration.Sources.Environment
{
    public class EnvironmentVariablesSource : BaseConstantSource
    {
        /// <inheritdoc />
        /// <summary>
        /// <para>Creates an <see cref="EnvironmentVariablesSource" /> instance.</para>
        /// </summary>
        public EnvironmentVariablesSource()
            : base(() => EnvironmentVariablesConverter.Convert(System.Environment.GetEnvironmentVariables()))
        {
        }
    }
}