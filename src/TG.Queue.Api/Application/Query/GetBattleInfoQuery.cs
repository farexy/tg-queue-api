using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using TG.Core.App.OperationResults;
using TG.Core.App.Services;
using TG.Core.Redis.DistributedLock;
using TG.Queue.Api.Config.Options;
using TG.Queue.Api.Errors;
using TG.Queue.Api.Extensions;
using TG.Queue.Api.Helpers;
using TG.Queue.Api.Models.Dto;
using TG.Queue.Api.Models.Response;
using TG.Queue.Api.ServiceClients;
using TG.Queue.Api.Services;

namespace TG.Queue.Api.Application.Query
{
    public record GetBattleInfoQuery(Guid BattleId, Guid UserId) : IRequest<OperationResult<BattleInfoResponse>>;
    
    public class GetBattleInfoQueryHandler : IRequestHandler<GetBattleInfoQuery, OperationResult<BattleInfoResponse>>
    {
        private readonly IDistributedLock _distributedLock;
        private readonly BattleSettings _battleSettings;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IBattleServersClient _battleServersClient;
        private readonly IBattlesClient _battlesClient;
        private readonly IBattlesStorage _battlesStorage;

        public GetBattleInfoQueryHandler(IDistributedLock distributedLock, IOptionsSnapshot<BattleSettings> battleSettings,
            IDateTimeProvider dateTimeProvider, IBattleServersClient battleServersClient, IBattlesClient battlesClient, IBattlesStorage battlesStorage)
        {
            _distributedLock = distributedLock;
            _battleSettings = battleSettings.Value;
            _dateTimeProvider = dateTimeProvider;
            _battleServersClient = battleServersClient;
            _battlesClient = battlesClient;
            _battlesStorage = battlesStorage;
        }

        public async Task<OperationResult<BattleInfoResponse>> Handle(GetBattleInfoQuery request, CancellationToken cancellationToken)
        {
            if (TestBattles.IsTest(request.BattleId))
            {
                return TestBattleResult(request.BattleId);
            }

            if (!await _battlesStorage.IsUserInBattleAsync(request.BattleId, request.UserId))
            {
                return AppErrors.NotFound;
            }

            var battle = await _battlesStorage.FindAsync(request.BattleId);
            if (battle is null)
            {
                return AppErrors.NotFound;
            }

            if (battle.Open)
            {
                if (_dateTimeProvider.UtcNow < battle.ExpectedStartTime.ToUniversalTime())
                {
                    return new BattleInfoResponse
                    {
                        Id = battle.Id,
                        Ready = false,
                        ExpectedWaitingTimeSec = battle.ExpectedStartTime.GetWaitingTimeSec(_dateTimeProvider),
                    };
                }

                await using (await _distributedLock.AcquireLockAsync(battle.Id.ToString()))
                {
                    battle = await _battlesStorage.FindAsync(request.BattleId)!;
                    if (battle is not null && battle.Open)
                    {
                        var res = await _battleServersClient.GetBattleServerAsync(battle.Id);
                        if (res.HasError)
                        {
                            return res.Error!;
                        }

                        var battleServer = res.Result!;
                        if (battleServer.State is BattleServerState.Initializing || battleServer.LoadBalancerIp is null)
                        {
                            var settings = _battleSettings.BattleTypes[battle.BattleType];
                            return new BattleInfoResponse
                            {
                                Id = battle.Id,
                                Ready = false,
                                ExpectedWaitingTimeSec = settings.ExpectedWaitingTimeSec / 3,
                            };
                        }

                        battle.Open = false;
                        battle.ServerIp = battleServer.LoadBalancerIp;
                        battle.ServerPort = battleServer.LoadBalancerPort;

                        var userIds = await _battlesStorage.GetBattleUsers(battle.Id);
                        await Task.WhenAll(
                            _battlesStorage.SetAsync(battle),
                            _battlesClient.CreateAsync(new CreateBattleDto
                            {
                                BattleId = battle.Id,
                                BattleType = battle.BattleType,
                                UserIds = userIds,
                            }));
                    }
                }
            }

            return new BattleInfoResponse
            {
                Id = battle!.Id,
                ServerIp = battle.ServerIp,
                ServerPort = battle.ServerPort,
                Ready = true,
                AccessToken = null, // todo
            };
        }
        
        private BattleInfoResponse TestBattleResult(Guid battleId)
        {
            var settings = _battleSettings.TestServers.First(b => b.Value.Id == battleId).Value;
            return new BattleInfoResponse
            {
                Id = battleId,
                ServerIp = settings.Ip,
                ServerPort = settings.Port,
                Ready = true
            };
        }
    }
}