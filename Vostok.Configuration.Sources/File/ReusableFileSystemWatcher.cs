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
        private volatile bool disposed;

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
            disposed = true;

            periodicalChecker?.Stop();
            currentWatcher?.Dispose();
            currentWatcher = null;
        }

        private void RecreateWatcherIfNeeded()
        {
            if (disposed)
                return;

            if (!folder.Exists)
            {
                currentWatcher?.Dispose();
                currentWatcher = null;
            }
            else if (currentWatcher == null && TryCreateWatcher(out var newWatcher))
                currentWatcher = newWatcher;
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

                watcher = fileWatcher;
                return true;
            }

            watcher = null;
            return false;
        }
    }
}