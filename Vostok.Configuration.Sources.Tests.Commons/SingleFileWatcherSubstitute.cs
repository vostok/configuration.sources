using System;
using System.Reactive.Subjects;
using Vostok.Configuration.Sources.Implementations.File;

namespace Vostok.Configuration.Sources.Tests.Commons
{
    public class SingleFileWatcherSubstitute : IObservable<(string, Exception)>
    {
        private readonly Subject<(string, Exception)> observers;
        private (string content, Exception error)? currentValue;
        private readonly object locker;

        public SingleFileWatcherSubstitute(string filePath, FileSourceSettings fileSourceSettings)
        {
            observers = new Subject<(string, Exception)>();
            currentValue = null;
            locker = new object();
        }

        public IDisposable Subscribe(IObserver<(string, Exception)> observer)
        {
            var subscription = observers.Subscribe(observer);
            lock (locker)
                if (currentValue != null)
                    observer.OnNext(currentValue.Value);

            return subscription;
        }

        /// <summary>
        /// Imitates file creating/updating. Do not send OnNext if new and old values are equal.
        /// </summary>
        /// <param name="newValue">File content</param>
        /// <param name="ignoreIfEquals">Ignore if old and new values are equal. Always send OnNext for observers</param>
        public void GetUpdate(string newValue, bool ignoreIfEquals = false)
        {
            var isNew = currentValue == null || newValue != currentValue.Value.content || currentValue.Value.error != null;
            currentValue = (newValue, null as Exception);
            if (isNew || ignoreIfEquals)
                observers.OnNext(currentValue.Value);
        }

        /// <summary>
        /// Imitates throwing exeptions on reading file
        /// </summary>
        /// <param name="e">Some exception</param>
        public void ThrowException(Exception e)
        {
            if (currentValue == null)
                observers.OnError(e);
            else
            {
                currentValue = (currentValue.Value.content, e);
                observers.OnNext(currentValue.Value);
            }
        }
    }
}