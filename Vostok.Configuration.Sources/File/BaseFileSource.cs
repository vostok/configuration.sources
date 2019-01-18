using System;
using System.Reactive.Linq;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.Watchers;

namespace Vostok.Configuration.Sources.File
{
    /// <summary>
    /// <para>Note: this class is intended to be used only by those implementing their own configuration sources.</para>
    /// <para>A base class for all configuration sources that read settings from a file.</para>
    /// <para>Descendants should only specify how to parse file contents into a settings tree.</para>
    /// <para>The file is read upon receiving a file changed event and once in a period specified in settings.</para>
    /// </summary>
    [PublicAPI]
    public class BaseFileSource : IConfigurationSource
    {
        private readonly Func<string, ISettingsNode> parseSettings;
        private readonly Func<IObservable<(string, Exception)>> fileWatcherProvider;
        private CacheItem lastResult;

        private static readonly WatcherCache<FileSourceSettings, string> Watchers = 
            new WatcherCache<FileSourceSettings, string>(new FileWatcherFactory(new FileSystem()));
        
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