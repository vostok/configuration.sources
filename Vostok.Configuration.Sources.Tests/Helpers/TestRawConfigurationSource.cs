using System;
using System.Reactive.Subjects;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources.Tests
{
    internal class TestRawConfigurationSource: IRawConfigurationSource
    {
        private readonly ReplaySubject<(ISettingsNode settings, Exception error)> subject = new ReplaySubject<(ISettingsNode settings, Exception error)>();

        public TestRawConfigurationSource()
        {
        }
        
        public TestRawConfigurationSource(ISettingsNode settings, Exception error = null)
        {
            PushNewConfiguration(settings, error);
        }

        public void PushNewConfiguration(ISettingsNode settings, Exception error = null)
        {
            subject.OnNext((settings, error));
        }

        public void ThrowError(Exception error)
        {
            subject.OnError(error);
        }

        public IObservable<(ISettingsNode settings, Exception error)> ObserveRaw() => subject;
    }
}