using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.Environment;
using Vostok.Configuration.Sources.Helpers;

namespace Vostok.Configuration.Sources.SettingsTree
{
    /// <summary>
    /// <para>Note: this class is intended to be used only by those implementing their own configuration sources.</para>
    /// <para>Contains helper methods allowing to create a settings tree from a sequence of keys.</para>
    /// <para>See <see cref="EnvironmentVariablesSource"/> implementation for a usage example.</para>
    /// </summary>
    [PublicAPI]
    public static class TreeFactory
    {
        /// <inheritdoc cref="CreateTreeByMultiLevelKey(string,IEnumerable{string},ISettingsNode)"/>
        public static ISettingsNode CreateTreeByMultiLevelKey(string rootName, string[] keys, string value) => 
            CreateTreeByMultiLevelKey(rootName, keys, new ValueNode(new []{rootName}.Concat(keys).Last(), value));

        /// <inheritdoc cref="CreateTreeByMultiLevelKey(string,IEnumerable{string},ISettingsNode)"/>
        public static ISettingsNode CreateTreeByMultiLevelKey(string rootName, string[] keys, string[] values) =>
            CreateTreeByMultiLevelKey(rootName, keys, new ArrayNode(new []{rootName}.Concat(keys).Last(), values.Select((v, i) => new ValueNode(i.ToString(), v)).ToArray()));

        /// <summary>
        /// Creates a settings tree with a path specified by <paramref name="keys"/> and ending with given <paramref name="value"/> node.
        /// </summary>
        public static ISettingsNode CreateTreeByMultiLevelKey(string rootName, IEnumerable<string> keys, ISettingsNode value)
        {
            var root = value;
            foreach (var key in rootName.ToEnumerable().Concat(keys).Reverse().Skip(1))
                root = new ObjectNode(key, new[] {root});
            return root;
        }
    }
}