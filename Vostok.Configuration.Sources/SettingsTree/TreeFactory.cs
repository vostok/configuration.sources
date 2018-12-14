using System;
using System.Collections.Generic;
using System.Linq;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources.SettingsTree
{
    public static class TreeFactory
    {
        public static ISettingsNode CreateTreeByMultiLevelKey(string rootName, string[] keys, string value)
        {
            return CreateTreeByMultiLevelKey(rootName, keys, new ValueNode(keys[keys.Length - 1], value));
        }
        
        public static ISettingsNode CreateTreeByMultiLevelKey(string rootName, string[] keys, ISettingsNode value)
        {
            return keys.Length > 0
                ? new ObjectNode(rootName, new []{CreateTreeByMultiLevelKey(keys[0], keys.Skip(1).ToArray(), value)})
                : value;
        }
    }
}