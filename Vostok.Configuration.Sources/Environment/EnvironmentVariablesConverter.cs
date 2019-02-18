using System;
using System.Collections;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.SettingsTree;

namespace Vostok.Configuration.Sources.Environment
{
    internal static class EnvironmentVariablesConverter
    {
        private static readonly string[] Separators = {".", ":", "__"};

        public static ISettingsNode Convert(IDictionary configuration)
        {
            var result = null as ISettingsNode;

            foreach (DictionaryEntry entry in configuration)
            {
                var node = TreeFactory.CreateTreeByMultiLevelKey(
                    null,
                    entry.Key.ToString().Replace(" ", "").Split(Separators, StringSplitOptions.RemoveEmptyEntries),
                    entry.Value.ToString());

                result = SettingsNodeMerger.Merge(result, node, null);
            }

            return result;
        }
    }
}