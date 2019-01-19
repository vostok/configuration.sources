using System;
using System.IO;
using System.Reactive.Linq;
using Vostok.Configuration.Sources.Helpers;
using Vostok.Configuration.Sources.Watchers;

namespace Vostok.Configuration.Sources.File
{
    internal class FileWatcherFactory : IWatcherFactory<FileSourceSettings, string>
    {
        private readonly IFileSystem fileSystem;

        public FileWatcherFactory(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public IObservable<(string value, Exception error)> CreateWatcher(FileSourceSettings settings)
        {
            return GenerateSignals(settings).SelectValueOrError(_ => ReadFile(settings));
        }

        private IObservable<object> GenerateSignals(FileSourceSettings settings)
        {
            return ObservableHelpers.PeriodicalSignalsFromNow(settings.FileWatcherPeriod).Merge(FileSystemEvents(settings.FilePath));
        }

        private string ReadFile(FileSourceSettings settings)
        {
            if (!fileSystem.Exists(settings.FilePath))
                return null;

            try
            {
                using (var reader = fileSystem.OpenFile(settings.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, settings.Encoding))
                    return reader.ReadToEnd();
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        private IObservable<object> FileSystemEvents(string filePath)
        {
            IDisposable watcher = null;
            return Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                h => { watcher = StartWatcher(filePath, h); },
                h => { watcher.Dispose(); });
        }

        private IDisposable StartWatcher(string filePath, FileSystemEventHandler handler)
        {
            var path = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(path))
                path = Directory.GetCurrentDirectory();
            return fileSystem.WatchFileSystem(path, Path.GetFileName(filePath), handler);
        }
    }
}