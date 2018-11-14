using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;

namespace Vostok.Configuration.Sources.Tests.Helpers
{
    internal class TestObserver<T> : IObserver<T>
    {
        public IList<Notification<T>> Messages
        {
            get
            {
                lock (lockObject)
                    return messages.ToList();
            }
        }

        public IList<T> Values => Messages.Select(message => message.Value).ToList();
        
        private readonly object lockObject = new object();
        private readonly List<Notification<T>> messages = new List<Notification<T>>();

        public void OnNext(T value)
        {
            Add(Notification.CreateOnNext(value));
        }

        public void OnError(Exception exception)
        {
            Add(Notification.CreateOnError<T>(exception));
        }

        public void OnCompleted()
        {
            Add(Notification.CreateOnCompleted<T>());
        }

        private void Add(Notification<T> message)
        {
            lock (lockObject)
                messages.Add(message);
        }
    }
}