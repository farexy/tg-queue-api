using System;
using TG.Core.App.Services;

namespace TG.Queue.Api.Extensions
{
    public static class BattleTimeExtensions
    {
        public static int GetWaitingTimeSec(this DateTime battleExpectedStart, IDateTimeProvider dateTimeProvider)
        {
            var now = dateTimeProvider.UtcNow;
            var timeDiff = battleExpectedStart.Subtract(now);
            if (timeDiff <= TimeSpan.Zero)
            {
                return 1;
            }

            return timeDiff.Seconds + 1;
        }
    }
}