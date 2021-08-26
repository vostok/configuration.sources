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
            var fixedSettings = MakeFilePathAbsolute(settings);
            return GenerateSignals(fixedSettings).SelectValueOrError(_ => ReadFile(fixedSettings));
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
                path = AppDomain.CurrentDomain.BaseDirectory;
            return fileSystem.WatchFileSystem(path, Path.GetFileName(filePath), handler);
        }

        private static FileSourceSettings MakeFilePathAbsolute(FileSourceSettings settings)
        {
            if (IsPathFullyQualified(settings.FilePath))
                return settings;
            var absoluteFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, settings.FilePath);
            return settings.WithFilePath(absoluteFilePath);
        }

        // NOTE (tsup): An alternative for https://docs.microsoft.com/ru-ru/dotnet/api/system.io.path.ispathrooted
        // since above method doesn't exist on .net standard and 'true' value of Path.IsPathRooted does not mean that path is relative.
        private static bool IsPathFullyQualified(string path)
        {
            var root = Path.GetPathRoot(path);
            return root.StartsWith(@"\\") || root.EndsWith(@"\");
        }
    }
}