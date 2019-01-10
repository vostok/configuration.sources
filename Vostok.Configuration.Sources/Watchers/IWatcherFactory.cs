using System;
using JetBrains.Annotations;
using Vostok.Configuration.Sources.File;

namespace Vostok.Configuration.Sources.Watchers
{
    /// <summary>
    /// <para>Note: this class is intended to be used only by those implementing their own configuration sources.</para>
    /// <para>A generic settings watcher factory to be used in <see cref="WatcherCache{TSettings,TValue}"/>.</para>
    /// <para>See <see cref="BaseFileSource"/> implementation for a usage example.</para>
    /// </summary>
    [PublicAPI]
    public interface IWatcherFactory<in TSettings, TValue>
    {
        /// <summary>
        /// Creates and starts a new generic settings watcher.
        /// </summary>
        IObservable<(TValue value, Exception error)> CreateWatcher(TSettings settings);
    }
}