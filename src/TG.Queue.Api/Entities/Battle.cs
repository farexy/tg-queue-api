using System;

namespace TG.Queue.Api.Entities
{
    public class Battle
    {
        public Guid Id { get; set; }

        public int ServerPort { get; set; }
        
        public string? ServerIp { get; set; }

        public DateTime CreatedAt { get; set; }
        
        public DateTime LastUpdate { get; set; }
    }
}