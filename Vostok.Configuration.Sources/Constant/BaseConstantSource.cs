using System;
using System.Reactive.Linq;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources.Constant
{
    /// <summary>
    /// <para>Note: this class is intended to be used only by those implementing their own configuration sources.</para>
    /// <para>Obtains settings once from the given provider and then propagates them to new subscribers.</para>
    /// </summary>
    [PublicAPI]
    public class BaseConstantSource : IConfigurationSource
    {
        private readonly Func<ISettingsNode> settingsGetter;
        private (ISettingsNode, Exception)? currentSettings;

        protected BaseConstantSource(Func<ISettingsNode> settingsGetter)
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