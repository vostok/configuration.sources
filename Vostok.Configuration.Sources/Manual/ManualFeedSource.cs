using System;
using System.Reactive.Subjects;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources.Manual
{
    /// <summary>
    /// Represents a source whose settings can be updated externally by manually calling <see cref="Push"/> method.
    /// </summary>
    [PublicAPI]
    public class ManualFeedSource : IConfigurationSource
    {
        private readonly ReplaySubject<(ISettingsNode settings, Exception error)> subject = 
            new ReplaySubject<(ISettingsNode settings, Exception error)>(1);

        public ManualFeedSource()
        {
        }

        public ManualFeedSource([CanBeNull] ISettingsNode initialSettings)
        {
            Push(initialSettings);
        }

        /// <summary>
        /// Pushes given <paramref name="settings"/> and <paramref name="error"/> to all subscribed observers.
        /// </summary>
        public void Push([CanBeNull] ISettingsNode settings, [CanBeNull] Exception error = null)
        {
            subject.OnNext((settings, error));
        }

        /// <inheritdoc />
        public IObservable<(ISettingsNode settings, Exception error)> Observe() => subject;
    }
}