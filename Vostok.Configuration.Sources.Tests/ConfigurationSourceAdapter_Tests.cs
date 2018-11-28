using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources.Tests
{
    [TestFixture]
    internal class ConfigurationSourceAdapter_Tests
    {
        private ConfigurationSourceAdapter adapter;
        private IRawConfigurationSource rawSource;
        private Subject<(ISettingsNode, Exception)> rawSubject;
        private readonly List<IDisposable> subscriptions = new List<IDisposable>();
        private readonly object lockObject = new object();
        private List<(ISettingsNode settings, Exception error)> sequence;

        [SetUp]
        public void SetUp()
        {
            rawSubject = new Subject<(ISettingsNode, Exception)>();
            
            rawSource = Substitute.For<IRawConfigurationSource>();
            rawSource.ObserveRaw().Returns(rawSubject);
            adapter = new TestConfigurationSourceAdapter(rawSource);
            
            // ReSharper disable once InconsistentlySynchronizedField
            sequence = new List<(ISettingsNode settings, Exception error)>();
        }

        [TearDown]
        public void TearDown()
        {
            rawSubject.Dispose();
            subscriptions.ForEach(disposable => disposable.Dispose());
        }
        
        [Test]
        public void Get_should_cache_settings()
        {
            PushSettings(rawSubject, "value");
            
            adapter.Get().Should().BeSameAs(adapter.Get());
        }
        
        [Test]
        public void Get_should_wait_for_the_first_value_from_raw_source_subscription()
        {
            var getTask = Task.Run(() => adapter.Get());
            getTask.Wait(50.Milliseconds());
            getTask.IsCompleted.Should().BeFalse();

            var settings = PushSettings(rawSubject, "value");

            getTask.Wait(1.Seconds());
            getTask.IsCompleted.Should().BeTrue();
            getTask.Result.Should().Be(settings);
        }

        [Test]
        public void Get_should_return_new_settings_when_raw_source_push_new_settings()
        {
            var settings1 = PushSettings(rawSubject, "value1");
            
            adapter.Get().Should().Be(settings1);

            var settings2 = PushSettings(rawSubject, "value2");
            
            adapter.Get().Should().Be(settings2);
        }

        [Test]
        public void Get_should_ignore_errors_when_there_were_valid_settings()
        {
            var settings = PushSettings(rawSubject, "value1");
            
            adapter.Get().Should().Be(settings);

            rawSubject.OnNext((null, new IOException()));
            
            adapter.Get().Should().Be(settings);
        }

        [Test]
        public void Get_should_resubscribe_when_current_observable_completes_with_error()
        {
            var getTask = Task.Run(() => adapter.Get());
            getTask.Wait(50.Milliseconds());
            getTask.IsCompleted.Should().BeFalse();
            
            rawSubject.OnNext((null, new IOException()));
            
            getTask.Wait(100.Milliseconds());
            getTask.IsCompleted.Should().BeFalse();
            
            var settings = PushSettings(rawSubject, "value");

            getTask.Wait(1.Seconds());
            getTask.IsCompleted.Should().BeTrue();
            getTask.Result.Should().Be(settings);
        }

        [Test]
        public void Get_should_throw_when_observables_completes_with_error_twice()
        {
            var getTask = Task.Run(() => adapter.Get());
            getTask.Wait(50.Milliseconds());
            getTask.IsCompleted.Should().BeFalse();
            
            rawSubject.OnNext((null, new IOException()));

            getTask.Wait(100.Milliseconds());
            getTask.IsCompleted.Should().BeFalse();
            
            rawSubject.OnNext((null, new IOException()));
        
            Task.WaitAny(new Task[] {getTask}, 1.Seconds());
            getTask.IsCompleted.Should().BeTrue();
            getTask.IsFaulted.Should().BeTrue();
        }
        
        [Test]
        public void Observe_should_immediately_push_last_value_to_new_subscriber_when_last_value_exists()
        {
            var settings = PushSettings(rawSubject, "value");
            
            var observable = adapter.Observe();
            
            subscriptions.Add(observable.Subscribe(AddSequenceItem));
            subscriptions.Add(observable.Subscribe(AddSequenceItem));

            var sequenceArray = CopySequence();
            sequenceArray.Length.Should().Be(2);
            sequenceArray[0].error.Should().BeNull();
            sequenceArray[0].settings.Should().Be(settings);
            sequenceArray[1].Should().BeEquivalentTo(sequenceArray[0]);
        }

        [Test]
        public void Observe_should_push_new_settings_when_raw_observable_push_new_settings()
        {
            var settings1 = PushSettings(rawSubject, "value1");
            
            SubscribeToAdapter();

            var settings2 = PushSettings(rawSubject, "value2");
            
            Action assertion = () =>
            {
                var sequenceArray = CopySequence();
                sequenceArray.Length.Should().Be(2);
                
                sequenceArray[0].settings.Should().Be(settings1);
                sequenceArray[0].error.Should().BeNull();
                
                sequenceArray[1].settings.Should().Be(settings2);
                sequenceArray[1].error.Should().BeNull();
            };
            assertion.ShouldPassIn(1.Seconds());
        }

        [Test]
        public void Observe_should_push_last_successful_settings_and_error_when_raw_source_push_error()
        {
            var settings = PushSettings(rawSubject, "value");
            
            SubscribeToAdapter();

            var error = new IOException(); 
            rawSubject.OnNext((null, error));
            
            Action assertion = () =>
            {
                var sequenceArray = CopySequence();
                sequenceArray.Length.Should().Be(2);
                
                sequenceArray[0].settings.Should().Be(settings);
                sequenceArray[0].error.Should().BeNull();
                
                sequenceArray[1].settings.Should().Be(sequenceArray[0].settings);
                sequenceArray[1].error.Should().Be(error);
            };
            assertion.ShouldPassIn(1.Seconds());
        }
        
        [Test]
        public void Observe_should_push_old_settings_and_new_error_when_raw_observable_push_new_error()
        {
            var settings = PushSettings(rawSubject, "initialValue");
            
            SubscribeToAdapter();

            var error1 = new IOException();
            var error2 = new FormatException();
            
            rawSubject.OnNext((null, error1));
            rawSubject.OnNext((null, error2));
            
            Action assertion = () =>
            {
                var sequenceArray = CopySequence();
                sequenceArray.Length.Should().Be(3);

                sequenceArray[1].Should().Be((settings, error1));
                sequenceArray[2].Should().Be((settings, error2));
            };
            assertion.ShouldPassIn(1.Seconds());
        }
        
        [Test]
        public void Observe_should_not_push_same_settings_twice()
        {
            PushSettings(rawSubject, "value");
            
            SubscribeToAdapter();

            PushSettings(rawSubject, "value");
            
            Action assertion = () =>
            {
                var sequenceArray = CopySequence();
                sequenceArray.Length.Should().Be(1);
            };
            assertion.ShouldPassIn(1.Seconds());
            assertion.ShouldNotFailIn(1.Seconds());
        }
        
        [Test]
        public void Observe_should_not_push_same_settings_and_error_twice()
        {
            PushSettings(rawSubject, "initialValue");
            
            SubscribeToAdapter();

            var error1 = new IOException();
            var error2 = new IOException();
            
            rawSubject.OnNext((null, error1));
            rawSubject.OnNext((null, error2));
            
            Action assertion = () =>
            {
                var sequenceArray = CopySequence();
                sequenceArray.Length.Should().Be(2);
                sequenceArray[1].settings.Should().Be(sequenceArray[0].settings);
                sequenceArray[1].error.Should().Be(error1);
            };
            assertion.ShouldPassIn(1.Seconds());
            assertion.ShouldNotFailIn(1.Seconds());
        }
        
        [Test]
        public void Observe_should_call_onError_on_observers_when_raw_source_pushed_only_errors()
        {
            var onErrorCalled = false;
            
            var observable = adapter.Observe();
            subscriptions.Add(observable.Subscribe(AddSequenceItem, e => onErrorCalled = true));

            var error = new IOException();
            rawSubject.OnNext((null, error));
            
            Action assertion = () =>
            {
                CopySequence().Length.Should().Be(0);
                onErrorCalled.Should().BeTrue();
            };
            assertion.ShouldPassIn(1.Seconds());
        }

        private void AddSequenceItem((ISettingsNode, Exception) item)
        {
            lock(lockObject)
                sequence.Add(item);
        }

        private (ISettingsNode settings, Exception error)[] CopySequence()
        {
            lock(lockObject)
                return sequence.ToArray();
        }

        private void SubscribeToAdapter()
        {
            var observable = adapter.Observe();
            subscriptions.Add(observable.Subscribe(AddSequenceItem));
        }

        private static ISettingsNode PushSettings(IObserver<(ISettingsNode, Exception)> observer, string value)
        {
            var settings = new ValueNode(value);
            observer.OnNext((settings, null));
            return settings;
        }
        
        private class TestConfigurationSourceAdapter : ConfigurationSourceAdapter
        {
            public TestConfigurationSourceAdapter(IRawConfigurationSource rawSource)
                : base(rawSource)
            {
            }
        }
    }
}