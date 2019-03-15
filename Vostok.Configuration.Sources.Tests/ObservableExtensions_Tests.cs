using System;
using System.Reactive.Subjects;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Testing.Observable;
using Vostok.Configuration.Sources.Helpers;

namespace Vostok.Configuration.Sources.Tests
{
    internal class ObservableExtensions_Tests
    {
        private TestObserver<(int value, Exception error)> observer;
        private Subject<int> signalsObservable;

        [SetUp]
        public void SetUp()
        {
            signalsObservable = new Subject<int>();
            observer = new TestObserver<(int value, Exception error)>();
        }
        
        [Test]
        public void SelectValueOrError_should_push_selected_value_and_null_when_selector_succeeds()
        {
            using (signalsObservable.SelectValueOrError(val => val + 1).Subscribe(observer))
            {
                signalsObservable.OnNext(1);
                observer.Values.Should().Equal((2, null));

                signalsObservable.OnNext(2);
                observer.Values.Should().Equal((2, null), (3, null));
            }
        }

        [Test]
        public void SelectValueOrError_should_push_default_and_error_when_selector_throws()
        {
            var error = new Exception();
            using (signalsObservable.SelectValueOrError<int, int>(val => throw error).Subscribe(observer))
            {
                signalsObservable.OnNext(1);
                
                observer.Values.Should().Equal((0, error));
            }
        }
    }
}