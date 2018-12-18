using System;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Commons.Helpers.Extensions;
using Vostok.Commons.Threading;
using Vostok.Configuration.Sources.Helpers;

namespace Vostok.Configuration.Sources.File
{
    /// <inheritdoc />
    /// <summary>
    ///     Watching changes in as single text file
    /// </summary>
    internal class SingleFileWatcher : IObservable<(string content, Exception error)>
    {
        private readonly string filePath;
        private readonly FileSourceSettings settings;
        private readonly AtomicBoolean taskIsRun;
        private readonly IFileSystem fileSystem;

        private readonly Subject<(string content, Exception error)> observers;

        private CancellationTokenSource tokenDelaySource;
        private CancellationToken tokenDelay;

        /// <summary>
        ///     Creates a <see cref="SingleFileWatcher" /> instance with given parameter <paramref name="filePath" />
        /// </summary>
        /// <param name="filePath">Full file path</param>
        /// <param name="fileSourceSettings"></param>
        public SingleFileWatcher([NotNull] string filePath, FileSourceSettings fileSourceSettings)
            : this(filePath, fileSourceSettings, new FileSystem())
        {
        }

        internal SingleFileWatcher([NotNull] string filePath, FileSourceSettings fileSourceSettings, IFileSystem fileSystem)
        {
            this.filePath = filePath;
            settings = fileSourceSettings ?? new FileSourceSettings();
            this.fileSystem = fileSystem;
            observers = new Subject<(string content, Exception error)>();
            taskIsRun = new AtomicBoolean(false);
        }

        public IDisposable Subscribe(IObserver<(string content, Exception error)> observer)
        {
            var subscription = observers.Subscribe(observer);

            var watcher = StartWatcher();
            
            var tokenStopSource = new CancellationTokenSource();
            Task.Run(() => WatchFile(tokenStopSource), tokenStopSource.Token);
            
            return new CompositeDisposable(watcher, new CancellationDisposable(tokenStopSource), subscription);
        }

        private IDisposable StartWatcher()
        {
            var path = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(path))
                path = AppDomain.CurrentDomain.BaseDirectory; // TODO(krait): Use working directory instead.
            return fileSystem.WatchFileSystem(path, Path.GetFileName(filePath), OnFileWatcherEvent);
        }

        private void OnFileWatcherEvent(object sender, FileSystemEventArgs args)
        {
            if (tokenDelaySource != null && !tokenDelaySource.IsCancellationRequested)
                tokenDelaySource.Cancel();
        }

        private async Task WatchFile(CancellationTokenSource tokenStopSource)
        {
            while (!tokenStopSource.IsCancellationRequested)
            {
                try
                {
                    observers.OnNext((ReadFile(), null));
                }
                catch (Exception e)
                {
                    observers.OnNext((null, e));
                }

                if (tokenDelaySource == null || tokenDelay.IsCancellationRequested)
                {
                    tokenDelaySource = new CancellationTokenSource();
                    tokenDelay = tokenDelaySource.Token;
                }

                var cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(tokenDelay, tokenStopSource.Token);
                await Task.Delay(settings.FileWatcherPeriod, cancellationToken.Token).SilentlyContinue().ConfigureAwait(false);
            }

            taskIsRun.TrySetFalse();
        }

        private string ReadFile()
        {
            if (!fileSystem.Exists(filePath))
                return null;

            using (var reader = fileSystem.OpenFile(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, settings.Encoding))
                return reader.ReadToEnd();
        }
    }
}