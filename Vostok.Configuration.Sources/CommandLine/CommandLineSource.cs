using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.Constant;
using Vostok.Configuration.Sources.SettingsTree;

namespace Vostok.Configuration.Sources.CommandLine
{
    /// <summary>
    /// <para>A source that provides settings parsed from given command line arguments.</para>
    /// <para>Supports 7 flavors of key-value syntax:</para>
    /// <list type="bullet">
    ///     <item><description><c>--key=value</c></description></item>
    ///     <item><description><c>--key value</c></description></item>
    ///     <item><description><c>-key=value</c></description></item>
    ///     <item><description><c>-key value</c></description></item>
    ///     <item><description><c>/key=value</c></description></item>
    ///     <item><description><c>/key value</c></description></item>
    ///     <item><description><c>key=value</c></description></item>
    /// </list>
    /// <para>Keys with dots (such as <c>"a.b.c"</c>) are treated as hierarchical and get split into segments.</para>
    /// <para>Multiple occurences of the same key are merged into arrays.</para>
    /// <para>Standalone keys may be optionally supplied with a default value.</para>
    /// <para>Standalone values may be optionally grouped under default key.</para>
    /// </summary>
    [PublicAPI]
    public class CommandLineSource : LazyConstantSource
    {
        public CommandLineSource([CanBeNull] string[] args)
            : this(args, null, null)
        {
        }

        public CommandLineSource([CanBeNull] string[] args, [CanBeNull] string defaultKey, [CanBeNull] string defaultValue)
            : base(() => ParseSettings(args, defaultKey, defaultValue))
        {
        }

        private static ISettingsNode ParseSettings(string[] args, string defaultKey, string defaultValue)
        {
            var resultBuilder = new ObjectNodeBuilder();
            var valueNodeIndex = new Dictionary<string, List<ValueNode>>(StringComparer.OrdinalIgnoreCase);
            var objectNodeIndex = new Dictionary<string, ISettingsNode>(StringComparer.OrdinalIgnoreCase);

            foreach (var pair in CommandLineArgumentsParser.Parse(args ?? Array.Empty<string>()))
            {
                var key = pair.key ?? defaultKey;
                if (key == null)
                    continue;

                var value = pair.value ?? defaultValue;
                if (value == null)
                    continue;

                var node = TreeFactory.CreateTreeByMultiLevelKey(null, key.Split('.'), value).Children.Single();
                var name = node.Name ?? string.Empty;

                if (node is ObjectNode objectNode)
                {
                    var currentNode = objectNodeIndex.TryGetValue(name, out var current) ? current : null;

                    objectNodeIndex[name] = SettingsNodeMerger.Merge(currentNode, objectNode, null);
                }
                else if (node is ValueNode valueNode)
                {
                    if (!valueNodeIndex.TryGetValue(name, out var nodes))
                        valueNodeIndex[name] = nodes = new List<ValueNode>();

                    nodes.Add(valueNode);
                } 
            }

            foreach (var pair in valueNodeIndex)
            {
                if (pair.Value.Count == 1)
                {
                    resultBuilder.SetChild(pair.Value.Single());
                }
                else
                {
                    resultBuilder.SetChild(new ArrayNode(pair.Key, pair.Value.Select((node, index) => new ValueNode(index.ToString(), node.Value)).ToArray()));
                }
            }

            foreach (var pair in objectNodeIndex)
                resultBuilder.SetChild(pair.Value);

            var result = resultBuilder.Build();
            if (result.ChildrenCount > 0)
                return result;

            return null;
        }
    }
}
