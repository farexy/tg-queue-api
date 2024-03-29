﻿using System;

namespace TG.Queue.Api.Models.Response
{
    public class EnqueueToBattleResponse
    {
        public Guid BattleId { get; set; }
        
        public int ExpectedWaitingTimeSec { get; set; }
        
        public long ApproximateCurrentUsersCount { get; set; }
    }
}