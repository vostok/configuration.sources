using System;
using System.Reactive.Subjects;

namespace Vostok.Configuration.Sources.Helpers
{
    internal class RestartingSubject<TSubject, TValue> : ISubject<TValue>, IDisposable
        where TSubject : ISubject<TValue>, new()
    {
        private TSubject subject;
        
        public RestartingSubject()
        {
            subject = new TSubject(); 
        }
        
        public void OnCompleted()
        {
            subject.OnCompleted();
            Completed = true;
        }

        public void OnError(Exception error)
        {
            subject.OnError(error);
            Completed = true;
        }

        public void OnNext(TValue value)
        {
            subject.OnNext(value);
        }

        public IDisposable Subscribe(IObserver<TValue> observer) => subject.Subscribe(observer);

        public void RestartSequence()
        {
            DisposeSubject();
            subject = new TSubject();
            Completed = false;
        }
        
        public bool Completed { get; private set; }

        public void Dispose()
        {
            DisposeSubject();
        }

        private void DisposeSubject()
        {
            if (subject is IDisposable disposable)
                disposable.Dispose();
        }
    }
}