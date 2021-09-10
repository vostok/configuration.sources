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
        private IDisposable currentWatcher;
        private DateTime lastSeenCreationTime;

        public ReusableFileSystemWatcher(string path, string filter, FileSystemEventHandler eventHandler)
        {
            folder = new DirectoryInfo(path);
            lastSeenCreationTime = folder.CreationTime;
            this.filter = filter;
            this.eventHandler = eventHandler;

            currentWatcher = TryCreateWatcher();

            periodicalChecker = new PeriodicalAction(RecreateWatcherIfNeeded, exception => {}, () => CheckPeriod, true);
            periodicalChecker.Start();
        }

        public void Dispose()
        {
            periodicalChecker.Stop();
            currentWatcher?.Dispose();
            currentWatcher = null;
        }

        private void RecreateWatcherIfNeeded()
        {
            folder.Refresh();

            if (!folder.Exists || folder.CreationTime != lastSeenCreationTime)
            {
                currentWatcher?.Dispose();
                currentWatcher = null;
            }

            currentWatcher ??= TryCreateWatcher();
        }

        private IDisposable TryCreateWatcher()
        {
            FileSystemWatcher fileWatcher;

            try
            {
                fileWatcher = CreateFileWatcher();
            }
            catch (Exception)
            {
                // NOTE (tsup): Let's trigger file absence as an event in order to have actual state information
                eventHandler(this, new FileSystemEventArgs(WatcherChangeTypes.Deleted, folder.FullName, string.Empty));
                throw;
            }

            // NOTE (tsup): We have to handle situations where the folder was deleted and recreated with a file so that we could see this event as a file change.
            if (lastSeenCreationTime != folder.CreationTime)
                foreach (var file in folder.EnumerateFiles(filter))
                    eventHandler(this, new FileSystemEventArgs(WatcherChangeTypes.Changed, folder.FullName, file.Name));
            lastSeenCreationTime = folder.CreationTime;

            return fileWatcher;
        }

        private FileSystemWatcher CreateFileWatcher()
        {
            var fileWatcher = new FileSystemWatcher(folder.FullName, filter)
            {
                InternalBufferSize = 8192
            };

            fileWatcher.Created += eventHandler;
            fileWatcher.Deleted += eventHandler;
            fileWatcher.Changed += eventHandler;
            fileWatcher.Renamed += (sender, args) => eventHandler(sender, args);

            fileWatcher.EnableRaisingEvents = true;

            return fileWatcher;
        }
    }
}