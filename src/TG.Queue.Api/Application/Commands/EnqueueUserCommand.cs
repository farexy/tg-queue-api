using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TG.Core.App.Extensions;
using TG.Core.App.OperationResults;
using TG.Core.App.Services;
using TG.Core.Db.Postgres.Extensions;
using TG.Core.Redis.DistributedLock;
using TG.Core.ServiceBus;
using TG.Core.ServiceBus.Messages;
using TG.Queue.Api.Config.Options;
using TG.Queue.Api.Db;
using TG.Queue.Api.Entities;
using TG.Queue.Api.Entities.Enums;
using TG.Queue.Api.Models.Response;

namespace TG.Queue.Api.Application.Commands
{
    public record EnqueueUserCommand(Guid UserId, string BattleType, BattleServerType ServerType) : IRequest<OperationResult<EnqueueToBattleResponse>>;
    
    public class EnqueueUserCommandHandler : IRequestHandler<EnqueueUserCommand, OperationResult<EnqueueToBattleResponse>>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IQueueProducer<PrepareBattleMessage> _queueProducer;
        private readonly IDistributedLock _distributedLock;
        private readonly BattleSettings _battleSettings;
        private readonly IDateTimeProvider _dateTimeProvider;

        public EnqueueUserCommandHandler(IQueueProducer<PrepareBattleMessage> queueProducer, IDistributedLock distributedLock,
            ApplicationDbContext dbContext, IOptionsSnapshot<BattleSettings> battleSettings, IDateTimeProvider dateTimeProvider)
        {
            _queueProducer = queueProducer;
            _distributedLock = distributedLock;
            _dbContext = dbContext;
            _dateTimeProvider = dateTimeProvider;
            _battleSettings = battleSettings.Value;
        }

        public async Task<OperationResult<EnqueueToBattleResponse>> Handle(EnqueueUserCommand request, CancellationToken cancellationToken)
        {
            if (request.ServerType.In(BattleServerType.Local, BattleServerType.Static))
            {
                return TestBattleResult(request);
            }
            var battleType = _battleSettings.BattleTypes[request.BattleType];
            await using (await _distributedLock.AcquireLockAsync(request.BattleType))
            {
                var currentBattle = await _dbContext.Battles
                    .Include(b => b.Users)
                    .FirstOrDefaultAsync(b => b.BattleType == request.BattleType && b.Open && b.UsersCount <= battleType.UsersCount, cancellationToken);

                if (currentBattle is null)
                {
                    currentBattle = new Battle
                    {
                        Id = Guid.NewGuid(),
                        Open = true,
                        BattleType = request.BattleType,
                        CreatedAt = _dateTimeProvider.UtcNow,
                        LastUpdate = _dateTimeProvider.UtcNow,
                        ExpectedStartTime = _dateTimeProvider.UtcNow.AddSeconds(battleType.ExpectedWaitingTimeSec)
                    };
                    await _dbContext.AddAsync(currentBattle, cancellationToken);
                    await _dbContext.SaveChangesAtomicallyAsync(() => _queueProducer.SendMessageAsync(new PrepareBattleMessage
                    {
                        BattleId = currentBattle.Id,
                        BattleType = request.BattleType
                    }));
                }

                currentBattle.UsersCount++;
                await _dbContext.AddAsync(new BattleUser
                {
                    UserId = request.UserId,
                    BattleId = currentBattle.Id
                }, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);

                return new EnqueueToBattleResponse
                {
                    BattleId = currentBattle.Id,
                    ExpectedWaitingTimeSec = battleType.ExpectedWaitingTimeSec,
                };
            }
        }

        private EnqueueToBattleResponse TestBattleResult(EnqueueUserCommand cmd)
        {
            var settings = _battleSettings.TestServers[cmd.ServerType];
            return new EnqueueToBattleResponse
            {
                BattleId = settings.Id,
                ExpectedWaitingTimeSec = settings.ExpectedWaitingTimeSec
            };
        }
    }
}