using JetBrains.Annotations;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.Transforming;

namespace Vostok.Configuration.Sources.Templating
{
    /// <summary>
    /// Replaces placeholders (like <c>#{this}</c>) in <see cref="ValueNode"/>s with values according to a set of <see cref="Substitution"/>s.
    /// </summary>
    [PublicAPI]
    public class TemplatingSource : TransformingSource
    {
        public TemplatingSource([NotNull] IConfigurationSource baseSource, [NotNull] TemplatingSourceOptions options)
            : base(baseSource, new ValueNodeTransformer(new SubstitutingTransformer(options.Substitutions).Transform))
        {
        }
    }
}
