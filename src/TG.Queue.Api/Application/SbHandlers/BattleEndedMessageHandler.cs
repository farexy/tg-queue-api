using System.Threading;
using System.Threading.Tasks;
using TG.Core.ServiceBus;
using TG.Core.ServiceBus.Messages;
using TG.Queue.Api.Services;

namespace TG.Queue.Api.Application.SbHandlers
{
    public class BattleEndedMessageHandler : IMessageHandler<BattleEndedMessage>
    {
        private readonly IBattlesStorage _battlesStorage;

        public BattleEndedMessageHandler(IBattlesStorage battlesStorage)
        {
            _battlesStorage = battlesStorage;
        }

        public async Task HandleMessage(BattleEndedMessage message, CancellationToken cancellationToken)
        {
            await _battlesStorage.ClearBattleAsync(message.BattleId);
        }
    }
}