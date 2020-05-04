using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources.Templating
{
    internal class SubstitutingTransformer
    {
        private static readonly Regex placeholderRegex = new Regex(@"#\{(?<placeholder>[\w\s\-\.:;]+)\}",
            RegexOptions.Compiled | RegexOptions.Multiline);

        private readonly Dictionary<string, Substitution> substitutions;

        public SubstitutingTransformer(IEnumerable<Substitution> substitutions)
        {

            this.substitutions = new Dictionary<string, Substitution>(StringComparer.OrdinalIgnoreCase);

            foreach (var substitution in substitutions)
                this.substitutions[substitution.Name] = substitution;
        }

        public ISettingsNode Transform(ValueNode node)
        {
            if (string.IsNullOrEmpty(node?.Value))
                return node;

            var matches = placeholderRegex.Matches(node.Value);
            if (matches.Count == 0)
                return node;

            var builder = new StringBuilder(node.Value);
            var offset = 0;

            foreach (Match match in matches)
            {
                var name = match.Groups["placeholder"].Value;

                if (!substitutions.TryGetValue(name, out var substitution))
                    continue;

                var substitutionValue = substitution.Value;

                builder
                    .Remove(match.Index + offset, match.Length)
                    .Insert(match.Index + offset, substitutionValue);

                offset += substitutionValue.Length - match.Length;
            }

            return new ValueNode(node.Name, builder.ToString());
        }
    }
}
