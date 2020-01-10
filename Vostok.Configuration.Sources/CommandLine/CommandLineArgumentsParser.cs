using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Vostok.Configuration.Sources.CommandLine
{
    internal static class CommandLineArgumentsParser
    {
        private const char Separator = '=';

        private static readonly string[] KeyPrefixes = {"--", "-", "/"};

        [NotNull]
        public static IEnumerable<(string key, string value)> Parse([NotNull] IReadOnlyList<string> args)
        {
            for (var i = 0; i < args.Count; i++)
            {
                var currentArgument = args[i]?.Trim();

                if (string.IsNullOrEmpty(currentArgument))
                    continue;

                var keyStartIndex = 0;

                foreach (var prefix in KeyPrefixes)
                {
                    if (currentArgument.StartsWith(prefix))
                    {
                        keyStartIndex = prefix.Length;
                        break;
                    }
                }

                string key, value;

                var separatorIndex = currentArgument.IndexOf(Separator);
                if (separatorIndex >= 0)
                {
                    key = currentArgument.Substring(keyStartIndex, separatorIndex - keyStartIndex);
                    value = currentArgument.Substring(separatorIndex + 1);
                }
                else
                {
                    if (keyStartIndex == 0)
                    {
                        yield return (null, currentArgument);
                        continue;
                    }

                    key = currentArgument.Substring(keyStartIndex);

                    if (i + 1 >= args.Count || !IsValidValue(args[i + 1]))
                    {
                        yield return (key, null);
                        continue;
                    }

                    value = args[++i];
                }

                yield return (key, value);
            }
        }

        private static bool IsValidValue(string value)
            => !KeyPrefixes.Any(value.StartsWith) && value.IndexOf(Separator) < 0;
    }
}
