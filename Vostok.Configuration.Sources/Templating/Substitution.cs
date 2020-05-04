using System;
using JetBrains.Annotations;

namespace Vostok.Configuration.Sources.Templating
{
    /// <summary>
    /// An instance of <see cref="Substitution"/> instructs on how to replace a single named placeholder in settings values.
    /// </summary>
    [PublicAPI]
    public class Substitution
    {
        private readonly Func<string> valueProvider;

        /// <param name="name">See <see cref="Name"/>.</param>
        /// <param name="value">See <see cref="Value"/>.</param>
        public Substitution([NotNull] string name, string value)
            : this(name, () => value)
        {
        }

        /// <param name="name">See <see cref="Name"/>.</param>
        /// <param name="valueProvider">See <see cref="Value"/>. Re-evaluated only on settings updates.</param>
        public Substitution([NotNull] string name, [NotNull] Func<string> valueProvider)
        {
            Name = name;
            this.valueProvider = valueProvider;
        }

        /// <summary>
        /// Name of the placeholder in settings to look for. Name 'Key' corresponds to the '#{Key}' placeholder.
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// Value to use instead of the placeholder.
        /// </summary>
        public string Value => valueProvider();
    }
}
