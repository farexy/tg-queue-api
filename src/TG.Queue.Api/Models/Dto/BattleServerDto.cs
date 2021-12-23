using System;

namespace TG.Queue.Api.Models.Dto
{
    public class BattleServerDto
    {
        public BattleServerState State { get; set; }

        public int Port { get; set; }

        public string? Ip { get; set; }
    }
}