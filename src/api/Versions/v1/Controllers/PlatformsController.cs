using Asp.Versioning;
using Homie.Api.v1.Handlers;
using Homie.Api.v1.TransferModels;
using Homie.Utilities.Attributes;
using Microsoft.AspNetCore.Mvc;
using Homie.Database.Models;

namespace Homie.Api.v1.Controllers
{
    [ApiVersion(Version.Name, Deprecated = Version.Deprecated)]
    [Route("{v:apiVersion}/platforms")]
    [Produces("application/json")]
    [ApiController]
    public class PlatformsController : ControllerBase
    {
        protected PlatformsHandler handler { get; init; }
        protected UsersHandler usersHandler { get; init; }
        public PlatformsController(PlatformsHandler _platformsHandler, UsersHandler _usersHandler) {
            handler = _platformsHandler;
            usersHandler = _usersHandler;
        }

        /// <summary>
        /// (Development) "GET" All platforms registered in the database.
        /// </summary>
        /// <returns><see cref="PlatformDTO"/>[]</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /platforms
        /// </remarks>
        /// <response code="200">Returns an array of Platforms</response>
        /// <response code="423">If used in a production build (Locked)</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status423Locked)]
        [EnvironmentDependant(ApiEnvironments.Development)]
        [HttpGet]
        public async Task<ActionResult<PlatformDTO[]>> GetAllPlatforms()
        {
            var result = await handler.GetAllAsync();
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns><see cref="CreatePlatformSuccess"/></returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /platforms/create
        ///     {
        ///        "name": "Test Platform",
        ///        "master_pswd": "password123"
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Returns the newly created item</response>
        /// <response code="400">If the item is null</response>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("create")]
        public async Task<ActionResult<CreatePlatformSuccess>> CreatePlatform(CreatePlatform newPlatform)
        {
            var createPlatformResult = await handler.PostAsync((PlatformDTO) newPlatform, newPlatform.Username);
            if (createPlatformResult.Value is null) {
                return createPlatformResult.Result!;
            }

            CreateUser newUser = new CreateUser {
                PlatformId = (uint) createPlatformResult.Value.Id!,
                Username = newPlatform.Name,
                Group = UserGroup.Guest
            };

            var createUserResult = await usersHandler.PostAsync((UserDTO) newUser);
            if (createUserResult.Value is null) {
                return createUserResult.Result!;
            }

            return CreatedAtAction(
                nameof(GetAllPlatforms), 
                new { 
                    platform_id = createPlatformResult.Value!.Id,
                    user_id = createUserResult.Value!.Id
                }, 
                new CreatePlatformSuccess(
                    createPlatformResult.Value,
                    createUserResult.Value
                )
            );
        }
    }
}

