using System;
using System.Reactive.Linq;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources.Constant
{
    internal class BaseConstantRawSource : IRawConfigurationSource
    {
        private readonly Func<ISettingsNode> settingsGetter;
        private (ISettingsNode, Exception)? currentSettings;

        public BaseConstantRawSource(Func<ISettingsNode> settingsGetter)
        {
            this.settingsGetter = settingsGetter;
        }

        public IObservable<(ISettingsNode settings, Exception error)> ObserveRaw()
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