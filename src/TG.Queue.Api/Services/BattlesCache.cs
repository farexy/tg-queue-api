using System;
using System.Threading.Tasks;
using StackExchange.Redis;
using TG.Core.App.Json;
using TG.Queue.Api.Entities;
using TG.Queue.Api.Entities.Cache;

namespace TG.Queue.Api.Services
{
    public interface IBattlesCache
    {
        Task InitCurrentUsersAsync(string battleType, int userCount, int expirationSec);
        Task<long> IncrementCurrentUsersAsync(string battleType, int userCount);
        Task DecrementCurrentUsersAsync(string battleType, int userCount);
        Task<Battle?> FindAsync(Guid battleId);
        Task SetCurrentAsync(string battleType, CurrentBattleInfo battle, int expirationSec);
        Task<CurrentBattleInfo?> GetCurrentAsync(string battleType);
    }

    public class BattlesCache : IBattlesCache
    {
        private const string CurrentBattlePrefix = "q_current_battle_";
        private const string CurrentUsersPrefix = "q_current_battle_users_";
        private readonly IDatabase _redis;

        public BattlesCache(IDatabase redis)
        {
            _redis = redis;
        }

        public async Task InitCurrentUsersAsync(string battleType, int userCount, int expirationSec)
        {
            await _redis.StringSetAsync(CurrentUsersPrefix + battleType, userCount, TimeSpan.FromSeconds(expirationSec));
        }
        
        public async Task<long> IncrementCurrentUsersAsync(string battleType, int userCount)
        {
            return await _redis.StringIncrementAsync(CurrentUsersPrefix + battleType, userCount);
        }

        public async Task DecrementCurrentUsersAsync(string battleType, int userCount)
        {
            await _redis.StringDecrementAsync(CurrentUsersPrefix + battleType, userCount);
        }

        public async Task<Battle?> FindAsync(Guid battleId)
        {
            var data = await _redis.StringGetAsync(CurrentBattlePrefix + battleId);
            return data.HasValue ? TgJsonSerializer.Deserialize<Battle>(data) : null;
        }

        public async Task SetCurrentAsync(string battleType, CurrentBattleInfo battle, int expirationSec)
        {
            await _redis.StringSetAsync(
                CurrentBattlePrefix + battleType, TgJsonSerializer.Serialize(battle), TimeSpan.FromSeconds(expirationSec));
        }

        public async Task<CurrentBattleInfo?> GetCurrentAsync(string battleType)
        {
            var data = await _redis.StringGetAsync(CurrentBattlePrefix + battleType);
            return data.HasValue ? TgJsonSerializer.Deserialize<CurrentBattleInfo>(data) : null;
        }
    }
}