using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using TG.Core.App.Extensions;
using TG.Core.App.OperationResults;
using TG.Core.App.Services;
using TG.Core.Redis.DistributedLock;
using TG.Core.ServiceBus;
using TG.Core.ServiceBus.Messages;
using TG.Queue.Api.Config.Options;
using TG.Queue.Api.Entities;
using TG.Queue.Api.Entities.Enums;
using TG.Queue.Api.Errors;
using TG.Queue.Api.Extensions;
using TG.Queue.Api.Models.Response;
using TG.Queue.Api.ServiceClients;
using TG.Queue.Api.Services;

namespace TG.Queue.Api.Application.Commands
{
    public record EnqueueUserCommand(Guid UserId, string BattleType, BattleServerType ServerType) : IRequest<OperationResult<EnqueueToBattleResponse>>;
    
    public class EnqueueUserCommandHandler : IRequestHandler<EnqueueUserCommand, OperationResult<EnqueueToBattleResponse>>
    {
        private readonly IQueueProducer<PrepareBattleMessage> _queueProducer;
        private readonly IDistributedLock _distributedLock;
        private readonly BattleSettings _battleSettings;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IUsersClient _usersClient;
        private readonly IBattlesStorage _battlesStorage;

        public EnqueueUserCommandHandler(IQueueProducer<PrepareBattleMessage> queueProducer, IDistributedLock distributedLock,
            IOptionsSnapshot<BattleSettings> battleSettings, IDateTimeProvider dateTimeProvider,
            IUsersClient usersClient, IBattlesStorage battlesStorage)
        {
            _queueProducer = queueProducer;
            _distributedLock = distributedLock;
            _dateTimeProvider = dateTimeProvider;
            _usersClient = usersClient;
            _battlesStorage = battlesStorage;
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

            var currentUsersCount = await _battlesStorage.IncrementCurrentUsersAsync(request.BattleType, 1);
            var battleSettings = _battleSettings.BattleTypes[request.BattleType];

            CurrentBattleInfo? currentBattleInfo;
            if (currentUsersCount == 1 || currentUsersCount > battleSettings.UsersCount)
            {
                currentBattleInfo = await TryCreateBattle(request.BattleType, battleSettings);
            }
            else
            {
                currentBattleInfo = await _battlesStorage.GetCurrentAsync(request.BattleType)
                                    ?? await TryCreateBattle(request.BattleType, battleSettings);
            }

            await _battlesStorage.AddBattleUserAsync(currentBattleInfo.Id, request.UserId);

            return new EnqueueToBattleResponse
            {
                BattleId = currentBattleInfo.Id,
                ExpectedWaitingTimeSec = currentBattleInfo.ExpectedStartTime.GetWaitingTimeSec(_dateTimeProvider),
                ApproximateCurrentUsersCount = currentUsersCount,
            };
        }

        private async Task<CurrentBattleInfo> TryCreateBattle(string battleType, BattleTypeSettings battleSettings)
        {
            await using (await _distributedLock.AcquireLockAsync(battleType))
            {
                var currentBattleInfo = await _battlesStorage.GetCurrentAsync(battleType);
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

                    currentBattleInfo = new CurrentBattleInfo
                    {
                        Id = newBattle.Id,
                        ExpectedStartTime = newBattle.ExpectedStartTime,
                    };
                    await Task.WhenAll(
                        _queueProducer.SendMessageAsync(
                            new PrepareBattleMessage
                            {
                                BattleId = newBattle.Id,
                                BattleType = battleType
                            }),
                        _battlesStorage.InitCurrentUsersAsync(battleType, 1,
                            battleSettings.ExpectedWaitingTimeSec),
                        _battlesStorage.SetCurrentAsync(battleType, currentBattleInfo,
                            battleSettings.ExpectedWaitingTimeSec),
                        _battlesStorage.SetAsync(newBattle)
                    );
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