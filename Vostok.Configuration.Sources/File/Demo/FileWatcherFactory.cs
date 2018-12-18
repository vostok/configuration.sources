using System;
using System.IO;
using System.Reactive.Linq;
using Vostok.Configuration.Sources.Helpers;

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
            return new GenericWatcher<string>(() => ReadFile(settings), GenerateSignals(settings));
        }

        private IObservable<object> GenerateSignals(FileSourceSettings settings) // TODO(krait): Wrap into a self-disposing observable with CompositeDisposable, like in SingleFileWatcher 
        {
            var watcher = StartWatcher(settings);
            return Observable.Interval(settings.FileWatcherPeriod).Select(_ => null as object)
                .Merge(Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(h => watcher.Changed += h, h => watcher.Changed -= h));
        }

        private string ReadFile(FileSourceSettings settings)
        {
            if (!fileSystem.Exists(settings.FilePath))
                return null;

            using (var reader = fileSystem.OpenFile(settings.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, settings.Encoding))
                return reader.ReadToEnd();
        }

        private FileSystemWatcher StartWatcher(FileSourceSettings settings)
        {
            var path = Path.GetDirectoryName(settings.FilePath);
            if (string.IsNullOrEmpty(path))
                path = AppDomain.CurrentDomain.BaseDirectory; // TODO(krait): Use working directory instead.
            var fileWatcher = new FileSystemWatcher(path, Path.GetFileName(settings.FilePath));
            fileWatcher.EnableRaisingEvents = true;
            return fileWatcher;
        }
    }
}