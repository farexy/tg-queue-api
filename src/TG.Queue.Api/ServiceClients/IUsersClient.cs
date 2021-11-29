using System;
using System.Threading.Tasks;
using RestEase;
using TG.Core.App.OperationResults;
using TG.Queue.Api.Models.Dto;

namespace TG.Queue.Api.ServiceClients
{
    public interface IUsersClient
    {
        [Get("v1/users/{userId}/check-money")]
        Task<OperationResult<UserMoneyCheckDto>> CheckMoneyAsync([Path] Guid userId, [Query] string battleType);
    }
}