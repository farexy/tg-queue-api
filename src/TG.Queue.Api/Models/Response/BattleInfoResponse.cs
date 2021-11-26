using System;

namespace TG.Queue.Api.Models.Response
{
    public class BattleInfoResponse
    {
        public Guid Id { get; set; }

        public int? ServerPort { get; set; }
        
        public string? ServerIp { get; set; }
        
        public string? AccessToken { get; set; }
        
        public bool Ready { get; set; }
        
        public int? ExpectedWaitingTimeSec { get; set; }
    }
}