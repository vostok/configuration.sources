using System;
using System.Reactive.Subjects;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources.ManualFeed
{
    /// <summary>
    /// <para>Note: this class is intended to be used only by those implementing their own configuration sources.</para>
    /// <para>Represents a source whose settings should be updated by manually calling a method from outside.</para>
    /// </summary>
    [PublicAPI]
    public class BaseManualFeedSource : IConfigurationSource
    {
        private readonly ReplaySubject<(ISettingsNode settings, Exception error)> subject = 
            new ReplaySubject<(ISettingsNode settings, Exception error)>();

        protected void PushNewConfiguration(ISettingsNode settings, Exception error = null)
        {
            subject.OnNext((settings, error));
        }

        /// <inheritdoc />
        public IObservable<(ISettingsNode settings, Exception error)> Observe() => subject;
    }
}