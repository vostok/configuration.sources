using System;
using System.Threading;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources.Helpers
{
    public class TaskSource
    {
        private CurrentValueObserver<(ISettingsNode, Exception)> rawValueObserver;

        public TaskSource() => rawValueObserver = new CurrentValueObserver<(ISettingsNode, Exception)>();

        public (ISettingsNode settings, Exception error) Get(IObservable<(ISettingsNode, Exception)> observable)
        {
            try
            {
                return rawValueObserver.Get(observable);
            }
            catch
            {
                ReplaceObserver();
                return rawValueObserver.Get(observable);
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