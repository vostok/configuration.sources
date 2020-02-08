using System;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources.Transforming
{
    [PublicAPI]
    public class ValueNodeTransformer : NodeTransformer
    {
        private readonly Func<ValueNode, ISettingsNode> transform;

        public ValueNodeTransformer([NotNull] Func<ValueNode, ISettingsNode> transform)
            => this.transform = transform ?? throw new ArgumentNullException(nameof(transform));

        public override bool TryTransform(ValueNode valueNode, out ISettingsNode transformedNode)
            => !valueNode.Equals(transformedNode = transform(valueNode));
    }
}
