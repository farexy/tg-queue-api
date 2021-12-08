using System;
using TG.Queue.Api.Entities.Enums;

namespace TG.Queue.Api.Services
{
    public interface ITestBattlesHelper
    {
        bool IsTestServerAllowed(Guid? testBattleId);
    }
}