using System;
using System.Reactive.Linq;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.Helpers;
using Vostok.Configuration.Sources.Watchers;

namespace Vostok.Configuration.Sources.File
{
    public class BaseFileSource : IConfigurationSource
    {
        private readonly Func<string, ISettingsNode> parseSettings;
        private readonly Func<IObservable<(string, Exception)>> fileWatcherProvider;
        private CacheItem lastResult;

        private static readonly WatcherCache<FileSourceSettings, string> Watchers = 
            new WatcherCache<FileSourceSettings, string>(new FileWatcherFactory(new FileSystem()));
        
        /// <summary>
        ///     <para>Creates a <see cref="BaseFileSource" /> instance.</para>
        ///     <para>Wayits for file to be parsed.</para>
        /// </summary>
        /// <param name="filePath">File name with settings</param>
        /// <param name="settings">File parsing settings</param>
        /// <param name="parseSettings">"Get" method invocation for string source</param>
        public BaseFileSource(FileSourceSettings settings, Func<string, ISettingsNode> parseSettings)
            : this(() => Watchers.Watch(settings), parseSettings)
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

                    var lastResult = this.lastResult;
                    if (lastResult != null && content == lastResult.Content)
                        return lastResult.Result;
                    
                    (ISettingsNode, Exception) result;
                    try
                    {
                        result = (parseSettings(content), null as Exception);
                    }
                    catch (Exception error)
                    {
                        result = (null, error);
                    }
                    
                    this.lastResult = new CacheItem(content, result);

                    return result;
                });
        }
        
        private class CacheItem
        {
            public string Content { get; }
            public (ISettingsNode settings, Exception error) Result { get; }

            public CacheItem(string content, (ISettingsNode settings, Exception error) result)
            {
                Content = content;
                Result = result;
            }
        }
    }
}