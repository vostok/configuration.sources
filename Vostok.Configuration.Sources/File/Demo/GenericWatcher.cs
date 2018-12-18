using System;
using System.Reactive.Linq;

namespace Vostok.Configuration.Sources.File
{
    internal class GenericWatcher<TValue> : IObservable<(TValue value, Exception error)>
    {
        private readonly Func<TValue> updateFunc;
        private readonly IObservable<object> signals;

        public GenericWatcher(Func<TValue> updateFunc, IObservable<object> signals)
        {
            this.updateFunc = updateFunc;
            this.signals = signals;
        }

        public IDisposable Subscribe(IObserver<(TValue value, Exception error)> observer)
        {
            return signals.Select(_ => Update()).Subscribe(observer);
        }

        private (TValue value, Exception error) Update()
        {
            try
            {
                return  (updateFunc(), null);
            }
            catch (Exception e)
            {
                return (default, e);
            }
        }
    }
}