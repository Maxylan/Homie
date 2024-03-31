// (c) 2024 @Maxylan
namespace Homie.Api.v1.Controllers;

using Asp.Versioning;
using Homie.Api.v1.Handlers;
using Homie.Api.v1.TransferModels;
using Homie.Utilities.Attributes;
using Microsoft.AspNetCore.Mvc;
using Homie.Database.Models;

[ApiVersion(Version.Numeric, Deprecated = Version.Deprecated)]
[Route("v{v:apiVersion}/platforms")]
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
    /// (Development) "GET" All users registered to a given platform.
    /// </summary>
    /// <param name="id"><see cref="Platform.Id"/> "platform_id"</param>
    /// <returns><see cref="UserDTO"/>[]</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /platforms/5/users
    /// </remarks>
    /// <response code="200">Returns an array of Users</response>
    /// <response code="423">If used in a production build (Locked)</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status423Locked)]
    [EnvironmentDependant(ApiEnvironments.Development)]
    [HttpGet("{id}/users")]
    public async Task<ActionResult<UserDTO[]>> GetAllUsersInPlatform(uint id)
    {
        if (!handler.Exists(id)) {
            return NotFound();
        }

        var result = await usersHandler.GetAllAsync();
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
    ///        "name": "Testylan",
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

