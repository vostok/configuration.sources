using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.SettingsTree;

namespace Vostok.Configuration.Sources.Environment
{
    internal static class EnvironmentVariablesConverter
    {
        public static ISettingsNode Convert(IDictionary configuration)
        {
            var trees = new List<ISettingsNode>();
            foreach (DictionaryEntry ev in configuration)
            {
                trees.Add(
                    TreeFactory.CreateTreeByMultiLevelKey(
                        null,
                        ev.Key.ToString().Replace(" ", "").Split('.'),
                        ev.Value.ToString()));
            }

            return trees.Any() ? trees.Aggregate((a, b) => a.Merge(b)) : null;
        }
    }
}