using System.Threading;
using System.Threading.Tasks;
using TG.Core.ServiceBus;
using TG.Core.ServiceBus.Messages;
using TG.Queue.Api.Db;
using TG.Queue.Api.Entities;
using TG.Queue.Api.Services;

namespace TG.Queue.Api.Application.SbHandlers
{
    public class BattleEndedMessageHandler : IMessageHandler<BattleEndedMessage>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IBattlesCache _battlesCache;

        public BattleEndedMessageHandler(ApplicationDbContext dbContext, IBattlesCache battlesCache)
        {
            _dbContext = dbContext;
            _battlesCache = battlesCache;
        }

        public async Task HandleMessage(BattleEndedMessage message, CancellationToken cancellationToken)
        {
            var battleToDelete = new Battle
            {
                Id = message.BattleId,
            };
            _dbContext.Remove(battleToDelete);
            await Task.WhenAll(
                _battlesCache.ClearBattleUsersAsync(message.BattleId),
                _dbContext.SaveChangesAsync(cancellationToken));
        }
    }
}