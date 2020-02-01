using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.SettingsTree;
using Vostok.Configuration.Sources.Transforming;

namespace Vostok.Configuration.Sources.Nesting
{
    /// <summary>
    /// A wrapper source that nests the output of its base source into a sequence of <see cref="ObjectNode"/>s with given names.
    /// </summary>
    [PublicAPI]
    public class NestingSource : TransformingSource
    {
        public NestingSource([NotNull] IConfigurationSource baseSource, [NotNull] params string[] scopes)
            : base(baseSource, node => Nest(node, scopes))
        {
        }

        [CanBeNull]
        private static ISettingsNode Nest([CanBeNull] ISettingsNode node, [NotNull] string[] scopes)
        {
            if (node == null || scopes.Length == 0)
                return node;

            return TreeFactory.CreateTreeByMultiLevelKey(null, scopes, PatchName(node, scopes.Last()));
        }

        [NotNull]
        private static ISettingsNode PatchName([NotNull] ISettingsNode node, [NotNull] string newName)
        {
            switch (node)
            {
                case ValueNode _:
                    return new ValueNode(newName, node.Value);

                case ArrayNode _:
                    return new ArrayNode(newName, node.Children.ToArray());

                case ObjectNode _:
                    return new ObjectNode(newName, node.Children);
            }

            return node;
        }
    }
}
