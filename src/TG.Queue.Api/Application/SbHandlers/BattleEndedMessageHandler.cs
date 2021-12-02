using System.Threading;
using System.Threading.Tasks;
using TG.Core.ServiceBus;
using TG.Core.ServiceBus.Messages;
using TG.Queue.Api.Services;

namespace TG.Queue.Api.Application.SbHandlers
{
    public class BattleEndedMessageHandler : IMessageHandler<BattleEndedMessage>
    {
        private readonly IBattlesCache _battlesCache;

        public BattleEndedMessageHandler(IBattlesCache battlesCache)
        {
            _battlesCache = battlesCache;
        }

        public async Task HandleMessage(BattleEndedMessage message, CancellationToken cancellationToken)
        {
            await _battlesCache.ClearBattleAsync(message.BattleId);
        }
    }
}