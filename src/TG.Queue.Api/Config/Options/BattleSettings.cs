using System;
using System.Collections.Generic;
using TG.Queue.Api.Entities.Enums;

namespace TG.Queue.Api.Config.Options
{
    public class BattleSettings
    {
        public Dictionary<string, BattleTypeSettings> BattleTypes { get; set; } = default!;
        public Dictionary<string, TestServerSettings> TestServers { get; set; } = default!;
    }
    
    public class BattleTypeSettings
    {
        public int UsersCount { get; set; }
        
        public int ExpectedWaitingTimeSec { get; set; }
        
        public int CostCoins { get; set; }
    }

    public class TestServerSettings
    {
        public BattleServerType Type { get; set; }

        public string Ip { get; set; } = default!;
        
        public int Port { get; set; }
        
        public int ExpectedWaitingTimeSec { get; set; }
    }
}