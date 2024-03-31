using Asp.Versioning;
using Homie.Api.v1.Handlers;
using Homie.Api.v1.TransferModels;
using Homie.Utilities.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Homie.Api.v1.Controllers
{
    [ApiVersion(Version.Name, Deprecated = Version.Deprecated)]
    [Route("{v:apiVersion}/platforms")]
    [Produces("application/json")]
    [ApiController]
    public class PlatformsController : ControllerBase
    {
        protected PlatformHandler handler { get; init; }
        public PlatformsController(PlatformHandler _platformHandler) => 
            handler = _platformHandler;

        [EnvironmentDependant(ApiEnvironments.Development)]
        [HttpGet]
        public async Task<ActionResult<PlatformDTO[]>> GetAllPlatforms()
        {
            var result = await handler.GetAllAsync();
            return result;
        }
    }
}

