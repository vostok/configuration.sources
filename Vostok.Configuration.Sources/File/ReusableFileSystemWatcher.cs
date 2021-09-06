using System;
using System.IO;
using System.Threading.Tasks;

namespace Vostok.Configuration.Sources.File
{
    public class ReusableFileSystemWatcher : IDisposable
    {
        private static readonly TimeSpan CheckPeriod = TimeSpan.FromSeconds(10);

        private readonly DirectoryInfo folder;
        private readonly string filter;
        private readonly FileSystemEventHandler eventHandler;
        private IDisposable currentWatcher;
        private IDisposable periodicalChecker;

        public ReusableFileSystemWatcher(string path, string filter, FileSystemEventHandler eventHandler)
        {
            folder = new DirectoryInfo(path);
            this.filter = filter;
            this.eventHandler = eventHandler;
        }

        public void Dispose()
        {
            periodicalChecker?.Dispose();
            currentWatcher?.Dispose();
        }

        // TODO: Add periodical action
        private void RecreateWatcherIfNeeded()
        {
            if (!folder.Exists)
                currentWatcher?.Dispose();
            else if (currentWatcher == null)
                TryCreateWatcher(out currentWatcher);
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