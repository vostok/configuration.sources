using System;
using System.Reactive.Linq;
using JetBrains.Annotations;
using Vostok.Configuration.Sources.File;

namespace Vostok.Configuration.Sources.Helpers
{
    /// <summary>
    /// <para>Note: this class is intended to be used only by those implementing their own configuration sources.</para>
    /// <para>See <see cref="FileSource"/> implementation for a usage example.</para>
    /// </summary>
    [PublicAPI]
    public static class ObservableHelpers
    {
        /// <summary>
        /// Returns an observable where an event happens once each <paramref name="period"/>. First event occurs immediately.
        /// </summary>
        /// <param name="period"></param>
        /// <returns></returns>
        public static IObservable<object> PeriodicalSignalsFromNow(TimeSpan period) => 
            Observable.Timer(TimeSpan.Zero, period).Select(_ => null as object);
    }
}