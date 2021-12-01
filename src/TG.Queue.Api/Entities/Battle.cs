using System;
using System.Collections.Generic;

namespace TG.Queue.Api.Entities
{
    public class Battle
    {
        public Guid Id { get; set; }

        public bool Open { get; set; }

        public string BattleType { get; set; } = default!;

        public int? ServerPort { get; set; }
        
        public string? ServerIp { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime ExpectedStartTime { get; set; }
        
        public IReadOnlyList<BattleUser>? Users { get; set; }
    }
}