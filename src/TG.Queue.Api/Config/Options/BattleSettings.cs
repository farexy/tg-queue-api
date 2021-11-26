using System;
using System.Collections.Generic;
using TG.Queue.Api.Entities.Enums;

namespace TG.Queue.Api.Config.Options
{
    public class BattleSettings
    {
        public Dictionary<string, BattleTypeSettings> BattleTypes { get; set; } = default!;
        public Dictionary<BattleServerType, TestServerSettings> TestServers { get; set; } = default!;
    }

    public class TestServerSettings
    {
        public Guid Id { get; set; }

        public string Ip { get; set; } = default!;
        
        public int Port { get; set; }
        
        public int ExpectedWaitingTimeSec { get; set; }
    }
}