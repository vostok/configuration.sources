using System;
using System.Threading;

namespace Vostok.Configuration.Sources.Helpers
{
    internal class TaskSource<T>
    {
        private readonly Func<ICurrentValueObserver<T>> currentValueObserverFactory;
        private ICurrentValueObserver<T> rawValueObserver;

        public TaskSource()
            : this(() => new CurrentValueObserver<T>())
        {
        }

        public TaskSource(Func<ICurrentValueObserver<T>> currentValueObserverFactory)
        {
            this.currentValueObserverFactory = currentValueObserverFactory;
            rawValueObserver = currentValueObserverFactory();
        }

        public T Get(Func<IObservable<T>> observableProvider)
        {
            try
            {
                return rawValueObserver.Get(observableProvider);
            }
            catch
            {
                ReplaceObserver();
                return rawValueObserver.Get(observableProvider);
            }
        }

        private void ReplaceObserver()
        {
            var currentObserver = rawValueObserver;
            var newObserver = currentValueObserverFactory();
            if (Interlocked.CompareExchange(ref rawValueObserver, newObserver, currentObserver) != currentObserver)
                newObserver.Dispose();
            currentObserver.Dispose();
        }
    }
}