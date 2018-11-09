using System;
using System.Reactive.Linq;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.Watchers;

namespace Vostok.Configuration.Sources.File
{
    internal class BaseFileRawSource : IRawConfigurationSource
    {
        private readonly Func<string, ISettingsNode> parseSettings;
        private readonly Func<IObservable<(string, Exception)>> fileWatcherProvider;
        private IObservable<(ISettingsNode settings, Exception error)> fileObserver;
        private string lastContent;
        private (ISettingsNode settings, Exception error) currentValue;

        public BaseFileRawSource(string filePath, FileSourceSettings settings, Func<string, ISettingsNode> parseSettings)
            : this(() => SettingsFileWatcher.WatchFile(filePath, settings), parseSettings)
        {
        }

        public BaseFileRawSource(Func<IObservable<(string, Exception)>> fileWatcherProvider, Func<string, ISettingsNode> parseSettings)
        {
            this.fileWatcherProvider = fileWatcherProvider;
            this.parseSettings = parseSettings;
        }

        /// <inheritdoc />
        public IObservable<(ISettingsNode settings, Exception error)> ObserveRaw()
        {
            if (fileObserver != null)
                return fileObserver;

            var fileWatcher = fileWatcherProvider();
            fileObserver = fileWatcher.Select(
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

            return fileObserver;
        }
    }
}