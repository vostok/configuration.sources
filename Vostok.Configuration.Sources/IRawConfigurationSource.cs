using System;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources
{
    public interface IRawConfigurationSource
    {
        IObservable<(ISettingsNode settings, Exception error)> ObserveRaw();
    }
}