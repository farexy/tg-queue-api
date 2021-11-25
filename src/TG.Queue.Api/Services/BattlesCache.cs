using System;
using System.Threading.Tasks;
using Azure.Identity;
using StackExchange.Redis;
using TG.Core.App.Json;
using TG.Queue.Api.Entities;

namespace TG.Queue.Api.Services
{
    public interface IBattlesCache
    {
        Task<Battle?> FindAsync(Guid battleId);
        Task AddAsync(Battle battle);
    }

    public class BattlesCache : IBattlesCache
    {
        private const string CacheKeyPrefix = "battles_queue_";
        private static readonly TimeSpan Expiration = TimeSpan.FromSeconds(15);
        private readonly IDatabase _redis;

        public BattlesCache(IDatabase redis)
        {
            _redis = redis;
        }

        public async Task<Battle?> FindAsync(Guid battleId)
        {
            var data = await _redis.StringGetAsync(CacheKeyPrefix + battleId);
            return data.HasValue ? TgJsonSerializer.Deserialize<Battle>(data) : null;
        }

        public async Task AddAsync(Battle battle)
        {
            await _redis.StringSetAsync(CacheKeyPrefix + battle.Id, TgJsonSerializer.Serialize(battle), Expiration);
        }
    }
}