using System;
using System.Collections;
using System.Reactive.Linq;
using System.Text;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.Implementations.Ini;

namespace Vostok.Configuration.Sources.Implementations.Environment
{
    internal class EnvironmentVariablesRawSource : IRawConfigurationSource
    {
        private volatile bool neverParsed;
        private (ISettingsNode, Exception) currentValue;

        public EnvironmentVariablesRawSource()
        {
            neverParsed = true;
        }

        private static ISettingsNode GetSettings(string vars) => new IniStringSource(vars, false).Get();

        private static string GetVariables()
        {
            var builder = new StringBuilder();
            foreach (DictionaryEntry ev in System.Environment.GetEnvironmentVariables())
                builder.AppendLine($"{ev.Key}={ev.Value}");
            return builder.ToString();
        }

        public IObservable<(ISettingsNode settings, Exception error)> ObserveRaw()
        {
            if (neverParsed)
            {
                currentValue = (GetSettings(GetVariables()), null as Exception);
                neverParsed = false;
            }

            return Observable.Return(currentValue);
        }
    }
}