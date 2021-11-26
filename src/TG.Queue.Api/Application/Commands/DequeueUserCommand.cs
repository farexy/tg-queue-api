using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TG.Core.App.OperationResults;
using TG.Core.ServiceBus;
using TG.Core.ServiceBus.Messages;
using TG.Queue.Api.Db;
using TG.Queue.Api.Errors;
using TG.Queue.Api.Models.Response;

namespace TG.Queue.Api.Application.Commands
{
    public record DequeueUserCommand(Guid UserId, Guid BattleId) : IRequest<OperationResult>;
    
    public class DequeueUserCommandHandler : IRequestHandler<DequeueUserCommand, OperationResult>
    {
        private readonly ApplicationDbContext _dbContext;

        public DequeueUserCommandHandler(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
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

            battleUser.Battle!.UsersCount--;
            _dbContext.Remove(battleUser);
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            return OperationResult.Success();
        }
    }
}