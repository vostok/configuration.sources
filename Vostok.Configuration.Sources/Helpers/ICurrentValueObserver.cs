using System;

namespace Vostok.Configuration.Sources.Helpers
{
    internal interface ICurrentValueObserver<T> : IDisposable
    {
        T Get(Func<IObservable<T>> observableProvider);
    }
}