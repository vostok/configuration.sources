using System;
using System.Reactive.Linq;
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
        private (ISettingsNode, Exception)? currentSettings;

        public LazyConstantSource(Func<ISettingsNode> settingsGetter)
        {
            this.settingsGetter = settingsGetter;
        }

        /// <inheritdoc />
        public IObservable<(ISettingsNode settings, Exception error)> Observe()
        {
            if (currentSettings == null)
            {
                try
                {
                    currentSettings = (settingsGetter(), null as Exception);
                }
                catch (Exception e)
                {
                    currentSettings = (null, e);
                }
            }

            return Observable.Return(currentSettings.Value);
        }
    }
}