using System.Threading;
using System.Threading.Tasks;
using TG.Core.ServiceBus;
using TG.Core.ServiceBus.Messages;
using TG.Queue.Api.Db;
using TG.Queue.Api.Entities;

namespace TG.Queue.Api.Application.SbHandlers
{
    public class BattleEndedMessageHandler : IMessageHandler<BattleEndedMessage>
    {
        private readonly ApplicationDbContext _dbContext;

        public BattleEndedMessageHandler(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task HandleMessage(BattleEndedMessage message, CancellationToken cancellationToken)
        {
            var battleToDelete = new Battle
            {
                Id = message.BattleId,
            };
            _dbContext.Remove(battleToDelete);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}