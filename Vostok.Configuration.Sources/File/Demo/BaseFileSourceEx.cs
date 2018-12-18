using System;
using System.Reactive.Linq;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.Helpers;

namespace Vostok.Configuration.Sources.File
{
    public class BaseFileSourceEx : IConfigurationSource
    {
        private readonly Func<FileSourceSettings> settingsProvider;
        private readonly Func<string, ISettingsNode> parseSettings;
        private string lastContent;
        private (ISettingsNode settings, Exception error) currentValue;

        private static readonly GenericWatcherCache<FileSourceSettings, string> Watchers = 
            new GenericWatcherCache<FileSourceSettings, string>(new FileWatcherFactory(new FileSystem()));
        
        public BaseFileSourceEx(Func<FileSourceSettings> settingsProvider, Func<string, ISettingsNode> parseSettings)
        {
            this.settingsProvider = settingsProvider;
            this.parseSettings = parseSettings;
        }

        public IObservable<(ISettingsNode settings, Exception error)> Observe()
        {
            var fileWatcher = Watchers.Watch(settingsProvider());
            return fileWatcher.Select(
                pair =>
                {
                    var (content, readingError) = pair;
                    if (readingError != null)
                        return (null, readingError);

                    if (content == lastContent)
                        return currentValue;
                    lastContent = content;
                    try
                    {
                        currentValue = (parseSettings(content), null as Exception);
                    }
                    catch (Exception error)
                    {
                        currentValue = (null, error);
                    }

                    return currentValue;
                });
        }
    }
}