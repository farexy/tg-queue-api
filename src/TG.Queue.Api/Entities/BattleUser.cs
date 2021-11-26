using System;

namespace TG.Queue.Api.Entities
{
    public class BattleUser
    {
        public Guid BattleId { get; set; }
        public Guid UserId { get; set; }
        public Battle? Battle { get; set; }
    }
}