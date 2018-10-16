using System;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources.Implementations.File
{
    public class BaseFileSource : ConfigurationSourceAdapter
    {
        protected BaseFileSource(string filePath, FileSourceSettings settings, Func<string, ISettingsNode> parseSettings)
            : base(new BaseFileRawSource(filePath, settings, parseSettings))
        {
        }

        protected BaseFileSource(
            string filePath,
            FileSourceSettings settings,
            Func<string, ISettingsNode> parseSettings,
            Func<string, FileSourceSettings, IObservable<(string, Exception)>> fileWatcherCreator) 
            : base(new BaseFileRawSource(filePath, settings, parseSettings, fileWatcherCreator))
        {
        }
    }
}