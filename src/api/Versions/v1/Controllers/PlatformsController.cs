using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Homie.Api.v1.Controllers
{
    [ApiVersion(v1.Name, Deprecated = v1.Deprecated)]
    [Produces("application/json")]
    [Route("[controller]")]
    [ApiController()]
    public class PlatformsController : ControllerBase
    {
        public PlatformsController()
        {
            // This is a constructor
        }
    }
}

