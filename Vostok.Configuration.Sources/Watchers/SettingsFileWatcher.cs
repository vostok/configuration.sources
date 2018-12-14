using System;
using System.Reactive.Linq;
using JetBrains.Annotations;
using Vostok.Configuration.Sources.File;
using Vostok.Configuration.Sources.Helpers;

namespace Vostok.Configuration.Sources.Watchers
{
    /// <summary>
    ///     Watches for changes in files
    /// </summary>
    internal static class SettingsFileWatcher
    {
        private static readonly GarbageCollectedCache<(string fileName, FileSourceSettings settings), SubscriptionsCounterAdapter<(string content, Exception error)>> Watchers =
            new GarbageCollectedCache<(string fileName, FileSourceSettings settings), SubscriptionsCounterAdapter<(string content, Exception error)>>(
                kv => kv.Value.SubscriptionsCount == 0);

        /// <summary>
        ///     Subscribtion to <paramref name="file" />
        /// </summary>
        /// <param name="file">Full file path</param>
        /// <param name="settings"></param>
        /// <returns>Subscriber receiving file text. Receive null if file not exists.</returns>
        public static IObservable<(string content, Exception error)> WatchFile([NotNull] string file, FileSourceSettings settings = null)
        {
            return WatchFile(file, settings, () => new SingleFileWatcher(file, settings));
        }
        
        internal static IObservable<(string content, Exception error)> WatchFile(string file, FileSourceSettings settings, Func<IObservable<(string, Exception)>> singleFileWatcherFactory)
        {
            return Watchers.GetOrAdd((file, settings), _ => singleFileWatcherFactory().Replay(1).RefCount().WithSubscriptionsCounter());
        }
    }
}