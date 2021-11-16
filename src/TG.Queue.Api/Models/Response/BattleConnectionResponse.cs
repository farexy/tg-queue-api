using System;

namespace TG.Queue.Api.Models.Response
{
    public class BattleConnectionResponse
    {
        public Guid BattleId { get; set; }

        public string ServerIp { get; set; } = default!;
        
        public int Port { get; set; }

        public string ConnectionToken { get; set; } = default!;
    }
}