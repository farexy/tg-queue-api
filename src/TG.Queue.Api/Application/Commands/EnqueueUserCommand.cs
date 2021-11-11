using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TG.Core.App.OperationResults;
using TG.Queue.Api.Db;

namespace TG.Queue.Api.Application.Commands
{
    public record EnqueueUserCommand : IRequest<OperationResult>;
    
    public class EnqueueUserCommandHandler : IRequestHandler<EnqueueUserCommand, OperationResult>
    {
        private readonly ApplicationDbContext _dbContext;

        public EnqueueUserCommandHandler(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<OperationResult> Handle(EnqueueUserCommand request, CancellationToken cancellationToken)
        {
        }
    }
}