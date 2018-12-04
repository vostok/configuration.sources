using System;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources.File
{
    public class BaseFileSource : ConfigurationSourceAdapter
    {
        /// <summary>
        ///     <para>Creates a <see cref="BaseFileRawSource" /> instance.</para>
        ///     <para>Wayits for file to be parsed.</para>
        /// </summary>
        /// <param name="filePath">File name with settings</param>
        /// <param name="settings">File parsing settings</param>
        /// <param name="parseSettings">"Get" method invocation for string source</param>
        public BaseFileSource(string filePath, FileSourceSettings settings, Func<string, ISettingsNode> parseSettings)
            : base(new BaseFileRawSource(filePath, settings, parseSettings))
        {
        }
    }
}