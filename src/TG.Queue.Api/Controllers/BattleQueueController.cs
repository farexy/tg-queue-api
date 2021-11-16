using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using TG.Core.App.OperationResults;
using TG.Queue.Api.Application.Commands;
using TG.Queue.Api.Models.Request;
using TG.Queue.Api.Models.Response;

namespace TG.Queue.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BattleQueueController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BattleQueueController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult<EnqueueToBattleResponse>> Enqueue([FromBody] EnqueueUserRequest request)
        {
            var cmd = new EnqueueUserCommand();
            var result = await _mediator.Send(cmd);
            return result.ToActionResult()
                .Ok();
        }
    }
}