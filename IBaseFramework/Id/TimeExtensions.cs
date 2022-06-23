using System;

namespace IBaseFramework.Id
{
    internal static class TimeExtensions
    {
        internal static Func<long> CurrentTimeFunc = InternalCurrentTimeMillis;

        internal static long CurrentTimeMillis()
        {
            return CurrentTimeFunc();
        }

        internal static IDisposable StubCurrentTime(Func<long> func)
        {
            CurrentTimeFunc = func;
            return new DisposableAction(() =>
            {
                CurrentTimeFunc = InternalCurrentTimeMillis;
            });
        }

        internal static IDisposable StubCurrentTime(long millis)
        {
            CurrentTimeFunc = () => millis;
            return new DisposableAction(() =>
            {
                CurrentTimeFunc = InternalCurrentTimeMillis;
            });
        }

        private static readonly DateTime Jan1St1970 = new DateTime
           (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static long InternalCurrentTimeMillis()
        {
            return (long)(DateTime.UtcNow - Jan1St1970).TotalMilliseconds;
        }
    }
}
