using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TG.Queue.Api.Models.Request;

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
        public async Task Enqueue([FromBody] EnqueueUserRequest request)
        {
            var cmd = new e
        }
    }
}