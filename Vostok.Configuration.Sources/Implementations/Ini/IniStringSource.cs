using System;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources.Implementations.Ini
{
    /// <inheritdoc />
    /// <summary>
    /// Ini converter to <see cref="ISettingsNode"/> tree from string
    /// </summary>
    public class IniStringSource : ConfigurationSourceAdapter
    {
        /// <summary>
        /// <para>Creates a <see cref="IniStringSource"/> instance using given string in <paramref name="ini"/> parameter</para>
        /// <para>Parsing is here.</para>
        /// </summary>
        /// <param name="ini">ini data in string</param>
        /// <param name="allowMultiLevelValues">Allow interpret point divided values as fields of inner objects</param>
        /// <exception cref="Exception">Ini has wrong format</exception>
        public IniStringSource(string ini, bool allowMultiLevelValues = true)
            : base(new IniStringRawSource(ini, allowMultiLevelValues))
        {
        }
    }
}