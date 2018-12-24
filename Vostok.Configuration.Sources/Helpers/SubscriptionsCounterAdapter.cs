using System;
using System.Reactive.Disposables;
using System.Threading;

namespace Vostok.Configuration.Sources.Helpers
{
    internal class SubscriptionsCounterAdapter<T> : IObservable<T>
    {
        private readonly IObservable<T> source;
        private int subscriptionsCount;

        public SubscriptionsCounterAdapter(IObservable<T> source)
        {
            this.source = source;
        }

        public int SubscriptionsCount => subscriptionsCount;

        public IDisposable Subscribe(IObserver<T> observer)
        {
            Interlocked.Increment(ref subscriptionsCount);
            var subscription = source.Subscribe(observer);
            return new CompositeDisposable(subscription, Disposable.Create(() => Interlocked.Decrement(ref subscriptionsCount)));
        }
    }
}