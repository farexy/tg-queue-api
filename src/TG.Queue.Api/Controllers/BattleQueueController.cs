using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TG.Core.App.Constants;
using TG.Core.App.Extensions;
using TG.Core.App.OperationResults;
using TG.Queue.Api.Application.Commands;
using TG.Queue.Api.Application.Query;
using TG.Queue.Api.Config;
using TG.Queue.Api.Models.Request;
using TG.Queue.Api.Models.Response;

namespace TG.Queue.Api.Controllers
{
    [ApiController]
    [ApiVersion(ApiVersions.V1)]
    [Route(ServiceConst.RoutePrefix)]
    [Authorize]
    public class BattleQueueController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BattleQueueController(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        [HttpGet("{battleId}")]
        public async Task<ActionResult<BattleInfoResponse>> GetInfo([FromRoute] Guid battleId)
        {
            var cmd = new GetBattleInfoQuery(battleId, User.GetUserId());
            var result = await _mediator.Send(cmd);
            return result.ToActionResult()
                .Ok();
        }

        [HttpPost]
        public async Task<ActionResult<EnqueueToBattleResponse>> Enqueue([FromBody] EnqueueUserRequest request)
        {
            var cmd = new EnqueueUserCommand(User.GetUserId(), request.BattleType, request.ServerType);
            var result = await _mediator.Send(cmd);
            return result.ToActionResult()
                .Ok();
        }
        
        [HttpDelete("{battleId}")]
        public async Task<ActionResult> Dequeue([FromRoute] Guid battleId, [FromQuery] string battleType)
        {
            var cmd = new DequeueUserCommand(User.GetUserId(), battleType, battleId);
            var result = await _mediator.Send(cmd);
            return result.ToActionResult()
                .NoContent();
        }
    }
}