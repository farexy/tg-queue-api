using System;

namespace TG.Queue.Api.Models.Dto
{
    public class BattleServerDto
    {
        public BattleServerState State { get; set; }
        
        public Guid BattleId { get; set; }

        public int LoadBalancerPort { get; set; }

        public string? LoadBalancerIp { get; set; }
    }
}