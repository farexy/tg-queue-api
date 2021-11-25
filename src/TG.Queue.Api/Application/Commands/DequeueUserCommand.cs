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
    public record DequeueUserCommand(Guid UserId, Guid BattleId) : IRequest<OperationResult>;
    
    public class DequeueUserCommandHandler : IRequestHandler<DequeueUserCommand, OperationResult>
    {
        private readonly IQueueProducer<PrepareBattleMessage> _queueProducer;

        public DequeueUserCommandHandler(IQueueProducer<PrepareBattleMessage> queueProducer)
        {
            _queueProducer = queueProducer;
        }

        public async Task<OperationResult> Handle(DequeueUserCommand request, CancellationToken cancellationToken)
        {
            return OperationResult.Success();
        }
    }
}