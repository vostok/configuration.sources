using System;
using System.Reactive.Subjects;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources.Manual
{
    /// <summary>
    /// <para>Represents a source whose settings can be updated externally by manually calling <see cref="Push"/> method.</para>
    /// <para>Automatically applies a transform to all user-provided values to turn them into <see cref="ISettingsNode"/>s.</para>
    /// 
    /// </summary>
    [PublicAPI]
    public class ManualFeedSource<T> : IConfigurationSource
    {
        private readonly ReplaySubject<(ISettingsNode settings, Exception error)> subject =
            new ReplaySubject<(ISettingsNode settings, Exception error)>(1);

        private readonly Func<T, ISettingsNode> transform;

        public ManualFeedSource([NotNull] Func<T, ISettingsNode> transform)
        {
            this.transform = transform ?? throw new ArgumentNullException(nameof(transform));
        }

        /// <summary>
        /// <para>Attempts to convert given <paramref name="value"/> to <see cref="ISettingsNode"/> and push it to all observers.</para>
        /// <para>In the event of error, pushes a <c>(null, exception)</c> pair instead.</para>
        /// </summary>
        public void Push(T value)
        {
            try
            {
                subject.OnNext((transform(value), null));
            }
            catch (Exception error)
            {
                subject.OnNext((null, error));
            }
        }

        public IObservable<(ISettingsNode settings, Exception error)> Observe() => subject;
    }
}
