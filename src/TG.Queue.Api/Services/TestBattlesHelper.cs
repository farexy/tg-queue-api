using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TG.Core.App.Extensions;
using TG.Queue.Api.Config.Options;

namespace TG.Queue.Api.Services
{
    public class TestBattlesHelper : ITestBattlesHelper
    {
        private readonly IHostEnvironment _hostEnvironment;
        private readonly BattleSettings _battleSettings;

        public TestBattlesHelper(IHostEnvironment hostEnvironment, IOptionsSnapshot<BattleSettings> battleSettings)
        {
            _hostEnvironment = hostEnvironment;
            _battleSettings = battleSettings.Value;
        }

        public bool IsTestServerAllowed(Guid? testBattleId)
        {
            return !testBattleId.HasValue ||
                   _hostEnvironment.IsDevelopmentOrDebug() && _battleSettings.TestServers.ContainsKey(testBattleId.Value.ToString());
        }

        public int GetWaitingTimeSec(Guid testBattleId)
        {
            return _battleSettings.TestServers[testBattleId.ToString()].ExpectedWaitingTimeSec;
        }

        public string CurrentBattleKey(string battleType, Guid? testBattleId) => battleType + testBattleId;
    }
}