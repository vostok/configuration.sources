using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.SettingsTree;
using Vostok.Configuration.Sources.SettingsTree.Mutable;

namespace Vostok.Configuration.Sources.Implementations.Ini
{
    internal class IniStringRawSource : IRawConfigurationSource
    {
        private readonly string ini;
        private readonly bool allowMultiLevelValues;
        private volatile bool neverParsed;
        private (ISettingsNode settings, Exception error) currentSettings;

        public IniStringRawSource(string ini, bool allowMultiLevelValues = true)
        {
            this.ini = ini;
            this.allowMultiLevelValues = allowMultiLevelValues;
            neverParsed = true;
        }
        
        private ISettingsNode ParseIni(string text, string name)
        {
            var res = new List<ISettingsNode>();
            string section = null;
            var currentLine = -1;

            var lines = text
                .Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrWhiteSpace(l));
            foreach (var line in lines)
            {
                currentLine++;
                if (line.StartsWith("#") || line.StartsWith(";"))
                    continue;
                if (line.StartsWith("[") && line.EndsWith("]") && line.Length > 2 && !line.Contains(" "))
                    section = line.Substring(1, line.Length - 2).Replace(" ", "");
                else
                {
                    var pair = line.Split(new[] {'='}, 2).Select(s => s.Trim()).ToArray();
                    if (pair.Length == 2 && pair[0].Length > 0 && !pair[0].Contains(" "))
                        res.Add(ParsePair(name, section, pair[0], pair[1]));
                    else
                        throw new FormatException($"{nameof(IniStringRawSource)}: wrong ini file ({currentLine}): line \"{line}\"");
                }
            }

            return res.Any() ? UnfoldSingleValues(res.Aggregate((a, b) => a.Merge(b))) : null;
        }

        private ISettingsNode ParsePair(string rootName, string section, string key, string value)
        {
            key = key.Replace(" ", "");
            var sectionKey = section != null ? new[] {section} : new string[0];
            var keys = sectionKey.Concat(allowMultiLevelValues ? key.Split('.') : new[] {key}).ToArray();
            var lastKey = keys[keys.Length - 1];
            return TreeFactory.CreateTreeByMultiLevelKey(rootName, keys, new ArrayNode(lastKey, new[] {new ValueNode(value)}));
        }

        private static ISettingsNode UnfoldSingleValues(ISettingsNode tree)
        {
            switch (tree)
            {
                case ArrayNode arrayNode:
                    return arrayNode.Children.Count() > 1
                        ? (ISettingsNode)arrayNode
                        : new ValueNode(arrayNode.Name, arrayNode.Children.First().Value);
                case ObjectNode objectNode:
                    return new ObjectNode(objectNode.Name, objectNode.Children.ToSortedDictionary(node => node.Name, UnfoldSingleValues, StringComparer.OrdinalIgnoreCase));
                default:
                    throw new ArgumentException();
            }
        }

        public IObservable<(ISettingsNode settings, Exception error)> ObserveRaw()
        {
            if (neverParsed)
            {
                neverParsed = false;
                try
                {
                    currentSettings = string.IsNullOrWhiteSpace(ini) ? (null, null) : (ParseIni(ini, "root"), null as Exception);
                }
                catch (Exception e)
                {
                    currentSettings = (null, e);
                }
            }

            return Observable.Return(currentSettings);
        }
    }
}