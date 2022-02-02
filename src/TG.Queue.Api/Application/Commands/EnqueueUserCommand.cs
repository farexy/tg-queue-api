using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using TG.Core.App.OperationResults;
using TG.Core.App.Services;
using TG.Core.Redis.DistributedLock;
using TG.Queue.Api.Config.Options;
using TG.Queue.Api.Entities;
using TG.Queue.Api.Errors;
using TG.Queue.Api.Extensions;
using TG.Queue.Api.Models.Response;
using TG.Queue.Api.ServiceClients;
using TG.Queue.Api.Services;

namespace TG.Queue.Api.Application.Commands
{
    public record EnqueueUserCommand(Guid UserId, string BattleType, Guid? TestBattleId) : IRequest<OperationResult<EnqueueToBattleResponse>>;
    
    public class EnqueueUserCommandHandler : IRequestHandler<EnqueueUserCommand, OperationResult<EnqueueToBattleResponse>>
    {
        private readonly IBattleServersClient _battleServersClient;
        private readonly IDistributedLock _distributedLock;
        private readonly BattleSettings _battleSettings;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IUsersClient _usersClient;
        private readonly IBattlesStorage _battlesStorage;
        private readonly ITestBattlesHelper _testBattlesHelper;

        public EnqueueUserCommandHandler(IBattleServersClient battleServersClient, IDistributedLock distributedLock,
            IOptionsSnapshot<BattleSettings> battleSettings, IDateTimeProvider dateTimeProvider,
            IUsersClient usersClient, IBattlesStorage battlesStorage, ITestBattlesHelper testBattlesHelper)
        {
            _battleServersClient = battleServersClient;
            _distributedLock = distributedLock;
            _dateTimeProvider = dateTimeProvider;
            _usersClient = usersClient;
            _battlesStorage = battlesStorage;
            _testBattlesHelper = testBattlesHelper;
            _battleSettings = battleSettings.Value;
        }

        public async Task<OperationResult<EnqueueToBattleResponse>> Handle(EnqueueUserCommand request, CancellationToken cancellationToken)
        {
            if (!_testBattlesHelper.IsTestServerAllowed(request.TestBattleId))
            {
                return AppErrors.TestServerNotAllowed;
            }
            var moneyCheck = await _usersClient.CheckMoneyAsync(request.UserId, request.BattleType);
            if (moneyCheck.HasError)
            {
                return moneyCheck.Error!;
            }

            if (!moneyCheck.Result!.Enough)
            {
                return AppErrors.UserNotEnoughMoney;
            }

            var currentUsersCount = await _battlesStorage.IncrementCurrentUsersAsync(
                _testBattlesHelper.CurrentBattleKey(request.BattleType, request.TestBattleId), 1);
            var battleSettings = _battleSettings.BattleTypes[request.BattleType];

            CurrentBattleInfo? currentBattleInfo;
            if (currentUsersCount == 1 || currentUsersCount > battleSettings.UsersCount)
            {
                currentBattleInfo = await TryCreateBattle(request, battleSettings);
            }
            else
            {
                currentBattleInfo = await _battlesStorage.GetCurrentAsync(request.BattleType)
                                    ?? await TryCreateBattle(request, battleSettings);
            }

            await _battlesStorage.AddBattleUserAsync(currentBattleInfo.Id, request.UserId);

            return new EnqueueToBattleResponse
            {
                BattleId = currentBattleInfo.Id,
                ExpectedWaitingTimeSec = currentBattleInfo.ExpectedStartTime.GetWaitingTimeSec(_dateTimeProvider),
                ApproximateCurrentUsersCount = currentUsersCount,
            };
        }

        private async Task<CurrentBattleInfo> TryCreateBattle(EnqueueUserCommand command, BattleTypeSettings battleSettings)
        {
            var (userId, battleType, testBattleId) = command;
            var currentBattleKey = _testBattlesHelper.CurrentBattleKey(battleType, testBattleId);
            await using (await _distributedLock.AcquireLockAsync(currentBattleKey))
            {
                var currentBattleInfo = await _battlesStorage.GetCurrentAsync(currentBattleKey);
                if (currentBattleInfo is null)
                {
                    int waitingSec = testBattleId.HasValue
                        ? _testBattlesHelper.GetWaitingTimeSec(testBattleId.Value)
                        : battleSettings.ExpectedWaitingTimeSec;

                    Guid battleId;
                    if (testBattleId.HasValue)
                    {
                        battleId = testBattleId.Value;
                    }
                    else
                    {
                        var battleServer = await _battleServersClient.AllocateAsync();
                        if (battleServer.HasError)
                        {
                            throw new ApplicationException(battleServer.Error!.ErrorMessage);
                        }

                        battleId = battleServer.Result!.BattleId;
                    }
                    var newBattle = new Battle
                    {
                        Id = battleId,
                        Open = true,
                        BattleType = battleType,
                        CreatedAt = _dateTimeProvider.UtcNow,
                        ExpectedStartTime = _dateTimeProvider.UtcNow.AddSeconds(waitingSec)
                    };

                    currentBattleInfo = new CurrentBattleInfo
                    {
                        Id = newBattle.Id,
                        ExpectedStartTime = newBattle.ExpectedStartTime,
                    };
                    await Task.WhenAll(
                        _battlesStorage.InitCurrentUsersAsync(currentBattleKey, 1, waitingSec),
                        _battlesStorage.SetCurrentAsync(currentBattleKey, currentBattleInfo, waitingSec),
                        _battlesStorage.SetAsync(newBattle),
                        _battlesStorage.InitBattleUsersAsync(newBattle.Id, userId)
                    );
                }

                return currentBattleInfo;
            }
        }
    }
}