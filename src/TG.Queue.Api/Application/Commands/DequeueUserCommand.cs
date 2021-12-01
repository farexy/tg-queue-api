using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TG.Core.App.OperationResults;
using TG.Queue.Api.Db;
using TG.Queue.Api.Errors;
using TG.Queue.Api.Services;

namespace TG.Queue.Api.Application.Commands
{
    public record DequeueUserCommand(Guid UserId, string BattleType, Guid BattleId) : IRequest<OperationResult>;
    
    public class DequeueUserCommandHandler : IRequestHandler<DequeueUserCommand, OperationResult>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IBattlesCache _battlesCache;

        public DequeueUserCommandHandler(ApplicationDbContext dbContext, IBattlesCache battlesCache)
        {
            _dbContext = dbContext;
            _battlesCache = battlesCache;
        }

        public async Task<OperationResult> Handle(DequeueUserCommand request, CancellationToken cancellationToken)
        {
            var battleUser = await _dbContext.BattleUsers
                .Include(b => b.Battle)
                .FirstOrDefaultAsync(b => b.BattleId == request.BattleId && b.UserId == request.UserId, cancellationToken);
            if (battleUser is null)
            {
                return AppErrors.NotFound;
            }

            await _battlesCache.DecrementCurrentUsersAsync(request.BattleType, 1);

            _dbContext.Remove(battleUser);
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            return OperationResult.Success();
        }
    }
}