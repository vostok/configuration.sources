using System;

namespace Vostok.Configuration.Sources.File
{
    public interface IWatcherFactory<in TSettings, TValue>
    {
        IObservable<(TValue value, Exception error)> CreateWatcher(TSettings settings);
    }
}