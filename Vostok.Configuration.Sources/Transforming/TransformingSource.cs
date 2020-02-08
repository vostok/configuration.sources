using System;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.Helpers;

namespace Vostok.Configuration.Sources.Transforming
{
    /// <summary>
    /// A wrapper source that may apply an arbitrary transform to settings returned by its base source. 
    /// </summary>
    [PublicAPI]
    public class TransformingSource : IConfigurationSource
    {
        private readonly IConfigurationSource baseSource;
        private readonly Func<ISettingsNode, ISettingsNode> treeTransform;

        public TransformingSource(
            [NotNull] IConfigurationSource baseSource, 
            [NotNull] Func<ISettingsNode, ISettingsNode> treeTransform)
        {
            this.baseSource = baseSource ?? throw new ArgumentNullException(nameof(baseSource));
            this.treeTransform = treeTransform ?? throw new ArgumentNullException(nameof(treeTransform));
        }

        public TransformingSource([NotNull] IConfigurationSource baseSource, [NotNull] NodeTransformer transformer)
            : this(baseSource, transformer.Transform)
        {
        }

        public IObservable<(ISettingsNode settings, Exception error)> Observe() =>
            baseSource.Observe().SelectValueOrError(TransformTree);

        [CanBeNull]
        private ISettingsNode TransformTree([CanBeNull] ISettingsNode tree)
            => tree == null ? null : treeTransform(tree);
    }
}
