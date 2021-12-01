using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using TG.Core.App.Exceptions;
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
using TG.Queue.Api.Entities.Cache;
using TG.Queue.Api.Entities.Enums;
using TG.Queue.Api.Errors;
using TG.Queue.Api.Models.Response;
using TG.Queue.Api.ServiceClients;
using TG.Queue.Api.Services;

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
        private readonly IUsersClient _usersClient;
        private readonly IBattlesCache _battlesCache;

        public EnqueueUserCommandHandler(IQueueProducer<PrepareBattleMessage> queueProducer, IDistributedLock distributedLock,
            ApplicationDbContext dbContext, IOptionsSnapshot<BattleSettings> battleSettings, IDateTimeProvider dateTimeProvider,
            IUsersClient usersClient, IBattlesCache battlesCache)
        {
            _queueProducer = queueProducer;
            _distributedLock = distributedLock;
            _dbContext = dbContext;
            _dateTimeProvider = dateTimeProvider;
            _usersClient = usersClient;
            _battlesCache = battlesCache;
            _battleSettings = battleSettings.Value;
        }

        public async Task<OperationResult<EnqueueToBattleResponse>> Handle(EnqueueUserCommand request,
            CancellationToken cancellationToken)
        {
            var moneyCheck = await _usersClient.CheckMoneyAsync(request.UserId, request.BattleType);
            if (moneyCheck.HasError)
            {
                return moneyCheck.Error!;
            }

            if (!moneyCheck.Result!.Enough)
            {
                return AppErrors.UserNotEnoughMoney;
            }

            if (request.ServerType.In(BattleServerType.Local, BattleServerType.Static))
            {
                return TestBattleResult(request);
            }

            var currentUsersCount = await _battlesCache.IncrementCurrentUsersAsync(request.BattleType, 1);
            var battleSettings = _battleSettings.BattleTypes[request.BattleType];

            CurrentBattleInfo? currentBattleInfo;
            if (currentUsersCount == 1 || currentUsersCount > battleSettings.UsersCount)
            {
                currentBattleInfo = await TryCreateBattle(request.BattleType, battleSettings, cancellationToken);
            }
            else
            {
                currentBattleInfo = await _battlesCache.GetCurrentAsync(request.BattleType)
                                    ?? await TryCreateBattle(request.BattleType, battleSettings, cancellationToken);
            }

            await _dbContext.AddAsync(new BattleUser
            {
                UserId = request.UserId,
                BattleId = currentBattleInfo.Id
            }, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new EnqueueToBattleResponse
            {
                BattleId = currentBattleInfo.Id,
                ExpectedWaitingTimeSec = currentBattleInfo.ExpectedStartTime.Subtract(_dateTimeProvider.UtcNow).Seconds,
                ApproximateCurrentUsersCount = currentUsersCount,
            };
        }

        private async Task<CurrentBattleInfo> TryCreateBattle(string battleType, BattleTypeSettings battleSettings, CancellationToken cancellationToken)
        {
            await using (await _distributedLock.AcquireLockAsync(battleType))
            {
                var currentBattleInfo = await _battlesCache.GetCurrentAsync(battleType);
                if (currentBattleInfo is null)
                {
                    var newBattle = new Battle
                    {
                        Id = Guid.NewGuid(),
                        Open = true,
                        BattleType = battleType,
                        CreatedAt = _dateTimeProvider.UtcNow,
                        ExpectedStartTime = _dateTimeProvider.UtcNow.AddSeconds(battleSettings.ExpectedWaitingTimeSec)
                    };
                    await _dbContext.AddAsync(newBattle, cancellationToken);

                    currentBattleInfo = new CurrentBattleInfo
                    {
                        Id = newBattle.Id,
                        ExpectedStartTime = newBattle.ExpectedStartTime,
                    };
                    await Task.WhenAll(
                        _battlesCache.InitCurrentUsersAsync(battleType, 1,
                            battleSettings.ExpectedWaitingTimeSec),
                        _battlesCache.SetCurrentAsync(battleType, currentBattleInfo,
                            battleSettings.ExpectedWaitingTimeSec)
                    );
                    await _dbContext.SaveChangesAtomicallyAsync(() => _queueProducer.SendMessageAsync(
                        new PrepareBattleMessage
                        {
                            BattleId = newBattle.Id,
                            BattleType = battleType
                        }));
                }

                return currentBattleInfo;
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