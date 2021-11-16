using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TG.Core.App.OperationResults;
using TG.Queue.Api.Db;
using TG.Queue.Api.Models.Response;

namespace TG.Queue.Api.Application.Commands
{
    public record EnqueueUserCommand : IRequest<OperationResult<EnqueueToBattleResponse>>;
    
    public class EnqueueUserCommandHandler : IRequestHandler<EnqueueUserCommand, OperationResult<EnqueueToBattleResponse>>
    {
        private readonly ApplicationDbContext _dbContext;

        public EnqueueUserCommandHandler(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<OperationResult<EnqueueToBattleResponse>> Handle(EnqueueUserCommand request, CancellationToken cancellationToken)
        {
            var battleId = Guid.NewGuid();
            return new EnqueueToBattleResponse
            {
                BattleId = battleId,
            };
        }
    }
}