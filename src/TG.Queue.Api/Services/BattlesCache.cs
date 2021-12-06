using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;
using TG.Core.App.Json;
using TG.Queue.Api.Entities;

namespace TG.Queue.Api.Services
{
    public interface IBattlesStorage
    {
        Task InitCurrentUsersAsync(string battleType, int userCount, int expirationSec);
        Task<long> IncrementCurrentUsersAsync(string battleType, int userCount);
        Task DecrementCurrentUsersAsync(string battleType, int userCount);
        Task SetCurrentAsync(string battleType, CurrentBattleInfo battle, int expirationSec);
        Task<CurrentBattleInfo?> GetCurrentAsync(string battleType);
        Task<Battle?> FindAsync(Guid battleId);
        Task SetAsync(Battle battle);
        Task AddBattleUserAsync(Guid battleId, Guid userId);
        Task RemoveBattleUserAsync(Guid battleId, Guid userId);
        Task<bool> IsUserInBattleAsync(Guid battleId, Guid userId);
        Task<IEnumerable<Guid>> GetBattleUsers(Guid battleId);
        Task ClearBattleAsync(Guid battleId);
    }

    public class BattlesStorage : IBattlesStorage
    {
        private const string CurrentBattlePrefix = "q_current_battle_";
        private const string CurrentUsersPrefix = "q_current_battle_users_";
        private const string BattleUsersPrefix = "q_battle_users_";
        private const string BattlePrefix = "q_battle_";
        private readonly IDatabase _redis;

        public BattlesStorage(IDatabase redis)
        {
            _redis = redis;
        }

        public Task InitCurrentUsersAsync(string battleType, int userCount, int expirationSec)
        {
            return _redis.StringSetAsync(CurrentUsersPrefix + battleType, userCount, TimeSpan.FromSeconds(expirationSec));
        }
        
        public Task<long> IncrementCurrentUsersAsync(string battleType, int userCount)
        {
            return _redis.StringIncrementAsync(CurrentUsersPrefix + battleType, userCount);
        }

        public Task DecrementCurrentUsersAsync(string battleType, int userCount)
        {
            return _redis.StringDecrementAsync(CurrentUsersPrefix + battleType, userCount);
        }

        public async Task<Battle?> FindAsync(Guid battleId)
        {
            var data = await _redis.StringGetAsync(BattlePrefix + battleId);
            return data.HasValue ? TgJsonSerializer.Deserialize<Battle>(data) : null;
        }

        public Task SetAsync(Battle battle)
        {
            const int battleExpirationMin = 1;
            return _redis.StringSetAsync(BattlePrefix + battle.Id, TgJsonSerializer.Serialize(battle), TimeSpan.FromMinutes(battleExpirationMin));
        }

        public Task SetCurrentAsync(string battleType, CurrentBattleInfo battle, int expirationSec)
        {
            return _redis.StringSetAsync(
                CurrentBattlePrefix + battleType, TgJsonSerializer.Serialize(battle), TimeSpan.FromSeconds(expirationSec));
        }

        public async Task<CurrentBattleInfo?> GetCurrentAsync(string battleType)
        {
            var data = await _redis.StringGetAsync(CurrentBattlePrefix + battleType);
            return data.HasValue ? TgJsonSerializer.Deserialize<CurrentBattleInfo>(data) : null;
        }

        public Task AddBattleUserAsync(Guid battleId, Guid userId)
        {
            return _redis.SetAddAsync(BattleUsersPrefix + battleId, userId.ToString());
        }
        
        public Task RemoveBattleUserAsync(Guid battleId, Guid userId)
        {
            return _redis.SetRemoveAsync(BattleUsersPrefix + battleId, userId.ToString());
        }

        public Task<bool> IsUserInBattleAsync(Guid battleId, Guid userId)
        {
            return _redis.SetContainsAsync(BattleUsersPrefix + battleId, userId.ToString());
        }

        public async Task<IEnumerable<Guid>> GetBattleUsers(Guid battleId)
        {
            var members = await _redis.SetMembersAsync(BattleUsersPrefix + battleId);
            return members.Select(id => Guid.Parse(id));
        }
        
        public Task ClearBattleAsync(Guid battleId)
        {
            return Task.WhenAll(
                _redis.KeyDeleteAsync(BattlePrefix + battleId),
                _redis.KeyDeleteAsync(BattleUsersPrefix + battleId));
        }
    }
}