using System.Collections;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.SettingsTree;

namespace Vostok.Configuration.Sources.Environment
{
    internal static class EnvironmentVariablesConverter
    {
        public static ISettingsNode Convert(IDictionary configuration)
        {
            var result = null as ISettingsNode;

            foreach (DictionaryEntry entry in configuration)
            {
                var node = TreeFactory.CreateTreeByMultiLevelKey(
                    null,
                    entry.Key.ToString().Replace(" ", "").Split('.'),
                    entry.Value.ToString());

                result = result == null ? node : result.Merge(node);
            }

            return result;
        }
    }
}