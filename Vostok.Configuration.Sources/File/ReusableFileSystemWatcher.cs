using System;
using System.IO;
using Vostok.Commons.Time;

namespace Vostok.Configuration.Sources.File
{
    internal class ReusableFileSystemWatcher : IDisposable
    {
        private static readonly TimeSpan CheckPeriod = TimeSpan.FromSeconds(10);

        private readonly DirectoryInfo folder;
        private readonly string filter;
        private readonly FileSystemEventHandler eventHandler;
        private readonly PeriodicalAction periodicalChecker;
        private volatile IDisposable currentWatcher;
        private DateTime lastSeenCreationTime;

        public ReusableFileSystemWatcher(string path, string filter, FileSystemEventHandler eventHandler)
        {
            folder = new DirectoryInfo(path);
            this.filter = filter;
            this.eventHandler = eventHandler;

            if (TryCreateWatcher(out var watcher))
                currentWatcher = watcher;

            periodicalChecker = new PeriodicalAction(RecreateWatcherIfNeeded, exception => {}, () => CheckPeriod);
            periodicalChecker.Start();
        }

        public void Dispose()
        {
            periodicalChecker?.Stop();
            currentWatcher?.Dispose();
            currentWatcher = null;
        }

        private void RecreateWatcherIfNeeded()
        {
            if (!folder.Exists)
            {
                currentWatcher?.Dispose();
                currentWatcher = null;
            }
            else if ((currentWatcher == null || folder.CreationTime != lastSeenCreationTime) && TryCreateWatcher(out var newWatcher))
            {
                lastSeenCreationTime = folder.CreationTime;
                currentWatcher = newWatcher;
            }
        }

        private bool TryCreateWatcher(out IDisposable watcher)
        {
            if (folder.Exists)
            {
                FileSystemWatcher fileWatcher;

                try
                {
                    fileWatcher = new FileSystemWatcher(folder.FullName, filter)
                    {
                        InternalBufferSize = 8192,
                    };
                }
                catch (Exception)
                {
                    watcher = null;
                    return false;
                }

                fileWatcher.Created += eventHandler;
                fileWatcher.Deleted += eventHandler;
                fileWatcher.Changed += eventHandler;
                fileWatcher.Renamed += (sender, args) => eventHandler(sender, args);

                fileWatcher.EnableRaisingEvents = true;

                // NOTE (tsup): We have to handle situations where the folder was deleted and recreated with a file so that we could see this event as a file change.
                foreach (var file in Directory.EnumerateFiles(folder.FullName, filter))
                    eventHandler(this, new FileSystemEventArgs(WatcherChangeTypes.Changed, folder.Name, file));

                watcher = fileWatcher;
                return true;
            }

            watcher = null;
            return false;
        }
    }
}