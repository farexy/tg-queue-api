using System.Threading.Tasks;
using RestEase;
using TG.Core.App.OperationResults;
using TG.Queue.Api.Models.Dto;

namespace TG.Queue.Api.ServiceClients
{
    public interface IBattlesClient
    {
        [Post("v1/battles")]
        Task<OperationResult<object>> CreateAsync([Body] CreateBattleDto request);
    }
}