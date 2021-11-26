using System;
using System.Collections.Generic;

namespace TG.Queue.Api.Models.Dto
{
    public class CreateBattleDto
    {
        public Guid BattleId { get; set; }
        public string? BattleType { get; set; }
        public IEnumerable<Guid> UserIds { get; set; } = default!;
    }
}