using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TG.Core.App.Constants;
using TG.Queue.Api.Config;
using TG.Queue.Api.Config.Options;

namespace TG.Queue.Api.Controllers
{
    [ApiController]
    [ApiVersion(ApiVersions.V1)]
    [Route(ServiceConst.RoutePrefix)]
    [Authorize]
    public class BattleTypesController : ControllerBase
    {
        private readonly BattleSettings _battleSettings;

        public BattleTypesController(IOptionsSnapshot<BattleSettings> battleSettings)
        {
            _battleSettings = battleSettings.Value;
        }

        [HttpGet]
        public Dictionary<string, BattleTypeSettings> Get()
        {
            return _battleSettings.BattleTypes;
        }
    }
}