using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TG.Core.App.OperationResults;
using TG.Core.ServiceBus;
using TG.Core.ServiceBus.Messages;
using TG.Queue.Api.Models.Response;

namespace TG.Queue.Api.Application.Commands
{
    public record EnqueueUserCommand(Guid UserId, string BattleType) : IRequest<OperationResult<EnqueueToBattleResponse>>;
    
    public class EnqueueUserCommandHandler : IRequestHandler<EnqueueUserCommand, OperationResult<EnqueueToBattleResponse>>
    {
        private readonly IQueueProducer<PrepareBattleMessage> _queueProducer;

        public EnqueueUserCommandHandler(IQueueProducer<PrepareBattleMessage> queueProducer)
        {
            _queueProducer = queueProducer;
        }

        public async Task<OperationResult<EnqueueToBattleResponse>> Handle(EnqueueUserCommand request, CancellationToken cancellationToken)
        {
            var battleId = Guid.NewGuid();

            await _queueProducer.SendMessageAsync(new PrepareBattleMessage
            {
                BattleId = battleId,
                BattleType = request.BattleType
            });
            return new EnqueueToBattleResponse
            {
                BattleId = battleId,
            };
        }
    }
}