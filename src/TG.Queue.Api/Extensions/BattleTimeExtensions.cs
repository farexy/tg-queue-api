using System;
using TG.Core.App.Services;

namespace TG.Queue.Api.Extensions
{
    public static class BattleTimeExtensions
    {
        public static int GetWaitingTimeSec(this DateTime battleExpectedStart, IDateTimeProvider dateTimeProvider) =>
            battleExpectedStart.Subtract(dateTimeProvider.UtcNow).Seconds + 1;
    }
}