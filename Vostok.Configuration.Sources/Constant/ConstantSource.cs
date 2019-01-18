using System;
using System.Reactive.Linq;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources.Constant
{
    /// <summary>
    /// <para>Propagates the settings specified in constructor to new subscribers.</para>
    /// </summary>
    [PublicAPI]
    public class ConstantSource : IConfigurationSource
    {
        private readonly ISettingsNode settings;

        public ConstantSource(ISettingsNode settings)
        {
            this.settings = settings;
        }

        /// <inheritdoc />
        public IObservable<(ISettingsNode settings, Exception error)> Observe()
        {
            return Observable.Return((settings, null as Exception));
        }
    }
}