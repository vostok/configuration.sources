using System;

namespace Vostok.Configuration.Sources.File
{
    internal interface IWatcherFactory<in TSettings, TValue>
    {
        IObservable<(TValue value, Exception error)> CreateWatcher(TSettings settings);
    }
}