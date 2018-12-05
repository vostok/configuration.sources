using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.Helpers;

namespace Vostok.Configuration.Sources
{
    public abstract class ConfigurationSourceAdapter : IConfigurationSource
    {
        protected IRawConfigurationSource rawSource;

        private TaskSource<(ISettingsNode settings, Exception error)> taskSource;
        private ReplaySubject<(ISettingsNode, Exception)> subject;
        private IObservable<(ISettingsNode settings, Exception error)> internalObservable;
        private (ISettingsNode settings, Exception error)? lastValue;

        protected ConfigurationSourceAdapter(IRawConfigurationSource rawSource)
        {
            this.rawSource = rawSource;
            taskSource = new TaskSource<(ISettingsNode settings, Exception error)>();
            subject = new ReplaySubject<(ISettingsNode, Exception)>(1);
            internalObservable = rawSource.ObserveRaw();
            internalObservable.Subscribe(
                newValue =>
                {
                    if (newValue.error != null)
                    {
                        if (lastValue.HasValue)
                            OnNext((lastValue.Value.settings, newValue.error));
                        else
                            OnError(newValue.error);
                    }
                    else
                        OnNext(newValue);
                });
        }

        /// <inheritdoc />
        /// <summary>
        ///     <para>Returns last parsed <see cref="ISettingsNode" /> tree.</para>
        ///     <para>Waits for first read.</para>
        /// </summary>
        /// <exception cref="Exception">Only on first read. Otherwise returns last parsed value.</exception>
        public ISettingsNode Get() => taskSource.Get(Observe).settings;

        /// <inheritdoc />
        /// <summary>
        ///     <para>Subscribtion to <see cref="ISettingsNode" /> tree changes.</para>
        ///     <para>Returns current value immediately on subscribtion.</para>
        /// </summary>
        public virtual IObservable<(ISettingsNode settings, Exception error)> Observe() => subject.AsObservable();

        private void OnNext((ISettingsNode settings, Exception error) newValue)
        {
            if (lastValue.HasValue && lastValue.Value.settings.Equals(newValue.settings) &&
                ExceptionsComparer.Equals(lastValue.Value.error, newValue.error))
                return;
            lastValue = newValue;
            subject.OnNext(newValue);
        }

        private void OnError(Exception error)
        {
            var oldSubject = subject;
            subject = new ReplaySubject<(ISettingsNode, Exception)>(1);
            oldSubject.OnError(error);
            oldSubject.Dispose();
        }
    }
}