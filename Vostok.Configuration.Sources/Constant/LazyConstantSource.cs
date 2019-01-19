using System;
using System.Reactive.Linq;
using System.Threading;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources.Constant
{
    /// <summary>
    /// <para>Obtains settings once from the given provider and then propagates them to new subscribers.</para>
    /// </summary>
    [PublicAPI]
    public class LazyConstantSource : IConfigurationSource
    {
        private readonly Func<ISettingsNode> settingsGetter;
        private readonly Lazy<(ISettingsNode, Exception)> currentSettings;

        public LazyConstantSource(Func<ISettingsNode> settingsGetter)
        {
            this.settingsGetter = settingsGetter;

            currentSettings = new Lazy<(ISettingsNode, Exception)>(ObtainSettings, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        /// <inheritdoc />
        public IObservable<(ISettingsNode settings, Exception error)> Observe()
        {
            return Observable.Return(currentSettings.Value);
        }

        private (ISettingsNode settings, Exception error) ObtainSettings()
        {
            try
            {
                return (settingsGetter(), null as Exception);
            }
            catch (Exception error)
            {
                return (null, error);
            }
        }
    }
}