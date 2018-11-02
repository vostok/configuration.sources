using System;
using System.Collections.Concurrent;
using JetBrains.Annotations;
using Vostok.Configuration.Sources.File;

namespace Vostok.Configuration.Sources.Watchers
{
    /// <summary>
    /// Watches for changes in files
    /// </summary>
    internal static class SettingsFileWatcher
    {
        private static readonly ConcurrentDictionary<string, IObservable<(string content, Exception error)>> Watchers =
            new ConcurrentDictionary<string, IObservable<(string content, Exception error)>>();

        /// <summary>
        /// Subscribtion to <paramref name="file" />
        /// </summary>
        /// <param name="file">Full file path</param>
        /// <param name="settings"></param>
        /// <returns>Subscriber receiving file text. Receive null if file not exists.</returns>
        public static IObservable<(string content, Exception error)> WatchFile([NotNull] string file, FileSourceSettings settings = null) =>
            WatchFile(file, settings, (f, s) => new SingleFileWatcher(f, s));

        internal static IObservable<(string content, Exception error)> WatchFile([NotNull] string file, FileSourceSettings settings, Func<string, FileSourceSettings, IObservable<(string content, Exception error)>> watcherCreator)
        {
            if (Watchers.TryGetValue(file, out var watcher))
                return watcher;

            watcher = watcherCreator?.Invoke(file, settings) ?? new SingleFileWatcher(file, settings);
            Watchers.TryAdd(file, watcher);
            return watcher;
        }

        internal static void ClearCache() => Watchers.Clear();
    }
}