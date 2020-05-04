using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Vostok.Configuration.Sources.Templating
{
    /// <summary>
    /// Configuration of <see cref="TemplatingSource"/>.
    /// </summary>
    [PublicAPI]
    public class TemplatingSourceOptions
    {
        public TemplatingSourceOptions([NotNull] IReadOnlyList<Substitution> substitutions)
            => Substitutions = substitutions ?? throw new ArgumentNullException(nameof(substitutions));

        [NotNull]
        public IReadOnlyList<Substitution> Substitutions { get; }
    }
}
