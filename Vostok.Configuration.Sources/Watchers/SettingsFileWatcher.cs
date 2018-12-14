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
                kv => kv.Value.SubscriptionsCount == 0, TimeSpan.FromSeconds(5));

        /// <summary>
        ///     Subscribtion to <paramref name="file" />
        /// </summary>
        /// <param name="file">Full file path</param>
        /// <param name="settings"></param>
        /// <returns>Subscriber receiving file text. Receive null if file not exists.</returns>
        public static IObservable<(string content, Exception error)> WatchFile([NotNull] string file, FileSourceSettings settings = null)
        {
            return Watchers.GetOrAdd((file, settings), _ => new SingleFileWatcher(file, settings).Replay().RefCount().WithSubscriptionsCounter());
        }
    }
}