using System;

namespace TG.Queue.Api.Entities
{
    public class CurrentBattleInfo
    {
        public Guid Id { get; set; }
        
        public DateTime ExpectedStartTime { get; set; }
    }
}