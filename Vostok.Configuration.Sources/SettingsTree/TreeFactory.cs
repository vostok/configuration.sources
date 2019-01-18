using System.Linq;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.Environment;

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
        /// <inheritdoc cref="CreateTreeByMultiLevelKey(string,string[],ISettingsNode)"/>
        public static ISettingsNode CreateTreeByMultiLevelKey(string rootName, string[] keys, string value) => 
            CreateTreeByMultiLevelKey(rootName, keys, new ValueNode(keys[keys.Length - 1], value));

        /// <summary>
        /// Creates a settings tree with a path specified by <paramref name="keys"/> and ending with node <paramref name="value"/>.
        /// </summary>
        public static ISettingsNode CreateTreeByMultiLevelKey(string rootName, string[] keys, ISettingsNode value)
        {
            return keys.Length > 0
                ? new ObjectNode(rootName, new []{CreateTreeByMultiLevelKey(keys[0], keys.Skip(1).ToArray(), value)})
                : value;
        }
    }
}