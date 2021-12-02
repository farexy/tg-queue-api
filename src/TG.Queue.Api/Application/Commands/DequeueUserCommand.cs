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
        private readonly IBattlesCache _battlesCache;

        public DequeueUserCommandHandler(IBattlesCache battlesCache)
        {
            _battlesCache = battlesCache;
        }

        public async Task<OperationResult> Handle(DequeueUserCommand request, CancellationToken cancellationToken)
        {
            await Task.WhenAll(
                _battlesCache.DecrementCurrentUsersAsync(request.BattleType, 1),
                _battlesCache.RemoveBattleUserAsync(request.BattleId, request.UserId)
            );

            return OperationResult.Success();
        }
    }
}