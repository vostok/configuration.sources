using System.Linq;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.Constant;
using Vostok.Configuration.Sources.SettingsTree;

namespace Vostok.Configuration.Sources.CommandLine
{
    /// <summary>
    /// <para>A source that provides settings parsed from given command line arguments.</para>
    /// <para>Supports 5 flavors of key-value syntax:</para>
    /// <list type="bullet">
    ///     <item><description><c>--key=value</c></description></item>
    ///     <item><description><c>--key value</c></description></item>
    ///     <item><description><c>/key=value</c></description></item>
    ///     <item><description><c>/key value</c></description></item>
    ///     <item><description><c>key=value</c></description></item>
    /// </list>
    /// <para>Keys with dots (such as <c>"a.b.c"</c>) are treated as hierarchical and get split into segments.</para>
    /// </summary>
    [PublicAPI]
    public class CommandLineSource : LazyConstantSource
    {
        public CommandLineSource(string[] args)
            : base(() => ParseSettings(args))
        {
        }

        private static ISettingsNode ParseSettings(string[] args)
        {
            var result = null as ISettingsNode;

            foreach (var (key, value) in CommandLineArgumentsParser.Parse(args ?? Enumerable.Empty<string>()))
            {
                var node = TreeFactory.CreateTreeByMultiLevelKey(null, key.Split('.'), value);

                result = SettingsNodeMerger.Merge(result, node, null);
            }

            return result;
        }
    }
}
