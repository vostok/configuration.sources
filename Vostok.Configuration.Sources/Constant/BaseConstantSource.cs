using System;
using System.Reactive.Linq;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources.Constant
{
    public class BaseConstantSource : IConfigurationSource
    {
        private readonly Func<ISettingsNode> settingsGetter;
        private (ISettingsNode, Exception)? currentSettings;

        protected BaseConstantSource(Func<ISettingsNode> settingsGetter)
        {
            this.settingsGetter = settingsGetter;
        }

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