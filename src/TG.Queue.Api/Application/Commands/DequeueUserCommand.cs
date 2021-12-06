using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TG.Core.App.OperationResults;
using TG.Queue.Api.Services;

namespace TG.Queue.Api.Application.Commands
{
    public record DequeueUserCommand(Guid UserId, string BattleType, Guid BattleId) : IRequest<OperationResult>;
    
    public class DequeueUserCommandHandler : IRequestHandler<DequeueUserCommand, OperationResult>
    {
        private readonly IBattlesStorage _battlesStorage;

        public DequeueUserCommandHandler(IBattlesStorage battlesStorage)
        {
            _battlesStorage = battlesStorage;
        }

        public async Task<OperationResult> Handle(DequeueUserCommand request, CancellationToken cancellationToken)
        {
            await Task.WhenAll(
                _battlesStorage.DecrementCurrentUsersAsync(request.BattleType, 1),
                _battlesStorage.RemoveBattleUserAsync(request.BattleId, request.UserId)
            );

            return OperationResult.Success();
        }
    }
}