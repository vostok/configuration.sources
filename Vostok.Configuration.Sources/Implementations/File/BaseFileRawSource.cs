using System;
using System.Reactive.Linq;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.Helpers;
using Vostok.Configuration.Sources.Watchers;

namespace Vostok.Configuration.Sources.Implementations.File
{
    /// <inheritdoc />
    /// <summary>
    /// Base class for converters to <see cref="ISettingsNode"/> tree from file
    /// </summary>
    internal class BaseFileRawSource : IRawConfigurationSource
    {
        private readonly string filePath;
        private readonly FileSourceSettings settings;
        private readonly Func<string, ISettingsNode> parseSettings;
        private readonly Func<string, FileSourceSettings, IObservable<(string, Exception)>> fileWatcherCreator;
        private IObservable<(ISettingsNode settings, Exception error)> fileObserver;
        private (ISettingsNode settings, Exception error)? currentValue;

        /// <summary>
        /// <para>Creates a <see cref="BaseFileRawSource"/> instance.</para>
        /// <para>Wayits for file to be parsed.</para>
        /// </summary>
        /// <param name="filePath">File name with settings</param>
        /// <param name="settings">File parsing settings</param>
        /// <param name="parseSettings">"Get" method invocation for string source</param>
        public BaseFileRawSource(string filePath, FileSourceSettings settings, Func<string, ISettingsNode> parseSettings)
            : this(filePath, settings, parseSettings, SettingsFileWatcher.WatchFile)
        {
        }

        public BaseFileRawSource(string filePath,
                                 FileSourceSettings settings,
                                 Func<string, ISettingsNode> parseSettings,
                                 Func<string, FileSourceSettings, IObservable<(string, Exception)>> fileWatcherCreator)
        {
            this.filePath = filePath;
            this.settings = settings ?? new FileSourceSettings();
            this.parseSettings = parseSettings;
            this.fileWatcherCreator = fileWatcherCreator;
        }

        /// <inheritdoc />
        public IObservable<(ISettingsNode settings, Exception error)> ObserveRaw()
        {
            if (fileObserver != null)
                return fileObserver;

            var fileWatcher = SettingsFileWatcher.WatchFile(filePath, settings, fileWatcherCreator);
            fileObserver = fileWatcher.Select(
                    pair =>
                    {
                        var (content, readingError) = pair;
                        if (readingError != null)
                            return (null, readingError);
                        try
                        {
                            return (parseSettings(content), null as Exception);
                        }
                        catch (Exception error)
                        {
                            return (null, error);
                        }
                    });

            return fileObserver;
        }
    }
}