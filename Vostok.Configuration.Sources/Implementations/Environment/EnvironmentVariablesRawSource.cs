using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.SettingsTree;

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

        private static ISettingsNode GetSettings()
        {
            var trees = new List<ISettingsNode>();
            foreach (DictionaryEntry ev in System.Environment.GetEnvironmentVariables())
                trees.Add(TreeFactory.CreateTreeByMultiLevelKey(
                    "root", ev.Key.ToString().Replace(" ", "").Split('.'), ev.Value.ToString()));

            return trees.Any() ? trees.Aggregate((a, b) => a.Merge(b)) : null;
        }

        public IObservable<(ISettingsNode settings, Exception error)> ObserveRaw()
        {
            if (neverParsed)
            {
                currentValue = (GetSettings(), null as Exception);
                neverParsed = false;
            }

            return Observable.Return(currentValue);
        }
    }
}