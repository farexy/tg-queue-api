using System;
using System.Threading.Tasks;
using Azure.Identity;
using StackExchange.Redis;

namespace TG.Queue.Api.Services
{
    public class BattleQueueProvider
    {
        private readonly IDatabase _redis;

        public BattleQueueProvider(IDatabase redis)
        {
            _redis = redis;
        }

        public async Task EnqueueAsync(Guid userId, string battleType)
        {
        }
    }
}