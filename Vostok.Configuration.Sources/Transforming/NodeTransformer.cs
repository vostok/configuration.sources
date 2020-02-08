using System.Linq;
using JetBrains.Annotations;
using Vostok.Commons.Collections;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources.Transforming
{
    [PublicAPI]
    public abstract class NodeTransformer
    {
        [CanBeNull]
        public ISettingsNode Transform([CanBeNull] ISettingsNode node)
        {
            ISettingsNode transformedNode;

            switch (node)
            {
                case ObjectNode objectNode:
                    if (TryTransform(objectNode, out transformedNode))
                        return transformedNode;

                    var builder = new ObjectNodeBuilder(objectNode.Name);
                    var transformedAnything = false;

                    foreach (var child in objectNode.Children)
                    {
                        var transformedChild = Transform(child);
                        if (transformedChild != null)
                            builder.SetChild(transformedChild);

                        if (!ReferenceEquals(child, transformedChild))
                            transformedAnything = true;
                    }

                    return transformedAnything ? builder.Build() : objectNode;

                case ArrayNode arrayNode:
                    if (TryTransform(arrayNode, out transformedNode))
                        return transformedNode;
                    
                    var transformedChildren = arrayNode.Children.Select(Transform).Where(t => t != null).ToArray();
                    if (transformedChildren.SequenceEqual(arrayNode.Children, ByReferenceEqualityComparer<ISettingsNode>.Instance))
                        return arrayNode;

                    return new ArrayNode(arrayNode.Name, transformedChildren);

                case ValueNode valueNode:
                    return TryTransform(valueNode, out transformedNode) ? transformedNode : valueNode;

                default:
                    return node;
            }
        }

        public virtual bool TryTransform([NotNull] ObjectNode objectNode, out ISettingsNode transformedNode)
        {
            transformedNode = null;
            return false;
        }

        public virtual bool TryTransform([NotNull] ArrayNode arrayNode, out ISettingsNode transformedNode)
        {
            transformedNode = null;
            return false;
        }

        public virtual bool TryTransform([NotNull] ValueNode valueNode, out ISettingsNode transformedNode)
        {
            transformedNode = null;
            return false;
        }
    }
}
