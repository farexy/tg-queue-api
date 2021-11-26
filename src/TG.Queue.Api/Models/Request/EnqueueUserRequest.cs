using TG.Queue.Api.Entities.Enums;

namespace TG.Queue.Api.Models.Request
{
    public class EnqueueUserRequest
    {
        public string BattleType { get; set; } = default!;
        
        public BattleServerType ServerType { get; set; }
    }
}