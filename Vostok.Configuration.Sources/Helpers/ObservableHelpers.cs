using System;
using System.Reactive.Linq;

namespace Vostok.Configuration.Sources.Helpers
{
    public static class ObservableHelpers
    {
        public static IObservable<object> PeriodicalSignalsFromNow(TimeSpan period)
        {
            return Observable.Timer(TimeSpan.Zero, period).Select(_ => null as object);
        }
    }
}