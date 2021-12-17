using System;

namespace TG.Queue.Api.Services
{
    public interface ITestBattlesHelper
    {
        bool IsTestServerAllowed(Guid? testBattleId);
        int GetWaitingTimeSec(Guid testBattleId);
        string CurrentBattleKey(string battleType, Guid? testBattleId);
    }
}