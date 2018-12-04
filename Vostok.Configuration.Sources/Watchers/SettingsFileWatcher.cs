using System;
using System.Collections.Concurrent;
using JetBrains.Annotations;
using Vostok.Configuration.Sources.File;

namespace Vostok.Configuration.Sources.Watchers
{
    /// <summary>
    ///     Watches for changes in files
    /// </summary>
    internal static class SettingsFileWatcher
    {
        private static readonly ConcurrentDictionary<(string fileName, FileSourceSettings settings), IObservable<(string content, Exception error)>> Watchers =
            new ConcurrentDictionary<(string fileName, FileSourceSettings settings), IObservable<(string content, Exception error)>>();

        /// <summary>
        ///     Subscribtion to <paramref name="file" />
        /// </summary>
        /// <param name="file">Full file path</param>
        /// <param name="settings"></param>
        /// <returns>Subscriber receiving file text. Receive null if file not exists.</returns>
        public static IObservable<(string content, Exception error)> WatchFile([NotNull] string file, FileSourceSettings settings = null)
        {
            return Watchers.GetOrAdd((file, settings), _ => new SingleFileWatcher(file, settings));
        }

        internal static void ClearCache() => Watchers.Clear();
    }
}