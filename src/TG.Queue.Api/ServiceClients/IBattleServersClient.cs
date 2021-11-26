using System;
using System.Threading.Tasks;
using RestEase;
using TG.Core.App.OperationResults;
using TG.Queue.Api.Models.Dto;

namespace TG.Queue.Api.ServiceClients
{
    public interface IBattleServersClient
    {
        [Get("v1/BattleServers/{battleId}")]
        Task<OperationResult<BattleServerDto>> GetBattleServerAsync([Path] Guid battleId);
    }
}