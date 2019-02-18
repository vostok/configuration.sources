using System.Collections.Generic;
using JetBrains.Annotations;

namespace Vostok.Configuration.Sources.CommandLine
{
    internal static class CommandLineArgumentsParser
    {
        private const char Separator = '=';

        private static readonly string[] KeyPrefixes = {"--", "/"};

        [NotNull]
        public static IEnumerable<(string key, string value)> Parse([NotNull] IEnumerable<string> args)
        {
            using (var enumerator = args.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var currentArgument = enumerator.Current?.Trim();

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
                            continue;

                        key = currentArgument.Substring(keyStartIndex);

                        if (!enumerator.MoveNext())
                            continue;

                        value = enumerator.Current;
                    }

                    yield return (key, value);
                }
            }
        }
    }
}
