namespace TG.Queue.Api.Config.Options;

public class BattleJwtTokenOptions
{
    public string PrivateKey { get; set; } = default!;
    
    public int LifetimeSec { get; set; }
}