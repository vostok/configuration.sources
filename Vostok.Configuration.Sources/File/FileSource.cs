using System;
using System.Reactive.Linq;
using JetBrains.Annotations;
using Vostok.Commons.Helpers.Rx;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.Helpers;
using Vostok.Configuration.Sources.Watchers;

namespace Vostok.Configuration.Sources.File
{
    /// <summary>
    /// <para>Note: this class is intended to be used only by those implementing their own configuration sources.</para>
    /// <para>A base class for all configuration sources that read settings from a file.</para>
    /// <para>Descendants should only specify how to parse file contents into a settings tree.</para>
    /// <para>The file is read upon receiving a file changed event and once in a period specified in settings.</para>
    /// </summary>
    [PublicAPI]
    public class FileSource : IConfigurationSource
    {
        private static readonly WatcherCache<FileSourceSettings, string> Watchers =
            new WatcherCache<FileSourceSettings, string>(new FileWatcherFactory(new FileSystem()));

        private readonly Func<string, ISettingsNode> parseSettings;
        private readonly Func<IObservable<(string, Exception)>> fileWatcherProvider;
        static FileSource() => RxHacker.Hack();

        public FileSource([NotNull] FileSourceSettings settings, [NotNull] Func<string, ISettingsNode> parseSettings)
            : this(() => Watchers.Watch(settings), parseSettings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            if (parseSettings == null)
                throw new ArgumentNullException(nameof(parseSettings));
        }

        internal FileSource(Func<IObservable<(string, Exception)>> fileWatcherProvider, Func<string, ISettingsNode> parseSettings)
        {
            this.fileWatcherProvider = fileWatcherProvider;
            this.parseSettings = parseSettings;
        }

        /// <inheritdoc />
        public IObservable<(ISettingsNode settings, Exception error)> Observe()
        {
            return fileWatcherProvider()
               .DistinctUntilChanged()
               .SelectValueOrError(parseSettings);
        }
    }
}