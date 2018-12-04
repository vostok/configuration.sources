using System;
using System.Threading;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources.Helpers
{
    public class TaskSource // CR(krait): Where are the tests?
    {
        private CurrentValueObserver<(ISettingsNode, Exception)> rawValueObserver;

        public TaskSource() => rawValueObserver = new CurrentValueObserver<(ISettingsNode, Exception)>();

        public (ISettingsNode settings, Exception error) Get(Func<IObservable<(ISettingsNode, Exception)>> observableProvider)
        {
            try
            {
                return rawValueObserver.Get(observableProvider());
            }
            catch
            {
                ReplaceObserver();
                return rawValueObserver.Get(observableProvider());
            }
        }

        private void ReplaceObserver()
        {
            var currentObserver = rawValueObserver;
            var newObserver = new CurrentValueObserver<(ISettingsNode, Exception)>();
            if (Interlocked.CompareExchange(ref rawValueObserver, newObserver, currentObserver) != currentObserver)
                newObserver.Dispose();
            currentObserver.Dispose();
        }
    }
}