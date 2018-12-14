using System;
using System.Reactive.Linq;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.Watchers;

namespace Vostok.Configuration.Sources.File
{
    public class BaseFileSource : IConfigurationSource
    {
        private readonly Func<string, ISettingsNode> parseSettings;
        private readonly Func<IObservable<(string, Exception)>> fileWatcherProvider;
        private string lastContent;
        private (ISettingsNode settings, Exception error) currentValue;

        /// <summary>
        ///     <para>Creates a <see cref="BaseFileSource" /> instance.</para>
        ///     <para>Wayits for file to be parsed.</para>
        /// </summary>
        /// <param name="filePath">File name with settings</param>
        /// <param name="settings">File parsing settings</param>
        /// <param name="parseSettings">"Get" method invocation for string source</param>
        protected BaseFileSource(string filePath, FileSourceSettings settings, Func<string, ISettingsNode> parseSettings)
            : this(() => SettingsFileWatcher.WatchFile(filePath, settings), parseSettings)
        {
        }

        internal BaseFileSource(Func<IObservable<(string, Exception)>> fileWatcherProvider, Func<string, ISettingsNode> parseSettings)
        {
            this.fileWatcherProvider = fileWatcherProvider;
            this.parseSettings = parseSettings;
        }

        /// <inheritdoc />
        public IObservable<(ISettingsNode settings, Exception error)> Observe()
        {
            var fileWatcher = fileWatcherProvider();
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