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
    /// (Development) "GET" The platform with PK `id`.
    /// </summary>
    /// <param name="id">"platform_id" (`<see cref="Platform.Id"/>`)</param>
    /// <returns><see cref="PlatformDTO"/></returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /platforms/id/5
    /// </remarks>
    /// <response code="200">Returns an array of Users</response>
    /// <response code="404">If requested platform isn't found</response>
    /// <response code="423">If used in a production build (Locked)</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status423Locked)]
    [EnvironmentDependant(ApiEnvironments.Development)]
    [HttpGet("id/{id}")]
    public async Task<ActionResult<PlatformDTO>> GetPlatformById(uint id)
    {
        var result = await handler.GetAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// (Development) "GET" The platform by a unique `code`.
    /// </summary>
    /// <param name="code">Unique Code, like `<see cref="Platform.GuestCode"/>` or `<see cref="Platform.MemberCode"/>`</param>
    /// <returns><see cref="PlatformDTO"/></returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /platforms/token/5
    /// </remarks>
    /// <response code="200">Returns an array of Users</response>
    /// <response code="404">If requested platform isn't found</response>
    /// <response code="423">If used in a production build (Locked)</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status423Locked)]
    [EnvironmentDependant(ApiEnvironments.Development)]
    [HttpGet("token/{id}")]
    public async Task<ActionResult<PlatformDTO>> GetPlatformById(string code)
    {
        var result = await handler.GetByCodeAsync(code);
        return Ok(result);
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
    /// Create a new platform.
    /// </summary>
    /// <param name="newPlatform">"CreatePlatform" Model (`<see cref="TransferModels.CreatePlatform"/>`)</param>
    /// <returns><see cref="CreatePlatformSuccess"/></returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /platforms/create
    ///     {
    ///        "name": "Test Platform",
    ///        "master_pswd": "password123"
    ///        "username": "Testylan",
    ///     }
    ///
    /// </remarks>
    /// <response code="201">Returns the newly created platform (and user) in their entirety. Only time you'll see its Reset Token.</response>
    /// <response code="400">If some required props are null/empty.</response>
    /// <response code="404">If the existance of the newly created platform couldn't be verified when creating the new user.</response>
    /// <response code="500">`<see cref="ArgumentNullException"/>`'s and `<see cref="Microsoft.EntityFrameworkCore.DbUpdateException"/>`'s</response>
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost("create")]
    public async Task<ActionResult<CreatePlatformSuccess>> CreatePlatform(CreatePlatform newPlatform)
    {
        var createPlatformResult = await handler.PostAsync((PlatformDTO) newPlatform, ("Username", newPlatform.Username));
        if (createPlatformResult.Value is null) {
            return createPlatformResult.Result!;
        }

        CreateUser newUser = new CreateUser {
            PlatformId = (uint) createPlatformResult.Value.Id!,
            Username = newPlatform.Name,
            Group = UserGroup.Member
        };

        var createUserResult = await usersHandler.PostAsync((UserDTO) newUser);
        if (createUserResult.Value is null) {
            return createUserResult.Result!;
        }

        return CreatedAtAction(
            nameof(CreatePlatform), 
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

    /// <summary>
    /// (Development) Join an existing platform. Made to test the API.
    /// </summary>
    /// <param name="newUser">"CreateUser" Model (`<see cref="CreateUser"/>`)</param>
    /// <param name="id">"platform_id" (`<see cref="Platform.Id"/>`)</param>
    /// <returns>Newly created user (`<see cref="UserDTO"/>`)</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /platforms/join/id/5
    ///     {
    ///        "platform_id": 5,
    ///        "username": "Testylan",
    ///        "group": <see cref="UserGroup"/>,
    ///        "first_name": "Testy",
    ///        "last_name": "Testsson"
    ///     }
    ///
    /// </remarks>
    /// <response code="201">Returns the newly created user in its entirety.</response>
    /// <response code="400">If some required props are null/empty.</response>
    /// <response code="404">If the requested platform couldn't be found.</response>
    /// <response code="423">If used in a production build (Locked)</response>
    /// <response code="500">`<see cref="ArgumentNullException"/>`'s and `<see cref="Microsoft.EntityFrameworkCore.DbUpdateException"/>`'s</response>
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status423Locked)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [EnvironmentDependant(ApiEnvironments.Development)]
    [HttpPost("join/id/{id:int}")]
    public async Task<ActionResult<UserDTO>> JoinPlatform(CreateUser newUser, uint id)
    {
        if (!await handler.ExistsAsync(id)) {
            return NotFound();
        }

        var createUserResult = await usersHandler.PostAsync((UserDTO) newUser);
        if (createUserResult.Value is null) {
            return createUserResult.Result!;
        }

        return CreatedAtAction(
            nameof(JoinPlatform), 
            new { 
                user_id = createUserResult.Value!.Id,
                token = createUserResult.Value!.Token
            }, 
            createUserResult.Value
        );
    }

    /// <summary>
    /// Join an existing platform using a code.
    /// </summary>
    /// <param name="newUser">"CreateUser" Model (`<see cref="CreateUser"/>`)</param>
    /// <param name="code" maxLength="6">
    /// Unique "guest_code" / "member_code", Max Length = 6. (`<see cref="Platform.GuestCode"/>` | `<see cref="Platform.MemberCode"/>`)
    /// </param>
    /// <returns>Newly created user (`<see cref="UserDTO"/>`)</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /platforms/join/code/asdf12
    ///     {
    ///        "platform_id": 5,
    ///        "username": "Testylan",
    ///        "group": *determined by {code}*,
    ///        "first_name": "Testy",
    ///        "last_name": "Testsson"
    ///     }
    ///
    /// </remarks>
    /// <response code="201">Returns the newly created user in its entirety.</response>
    /// <response code="400">If some required props are null/empty.</response>
    /// <response code="404">If the requested platform couldn't be found.</response>
    /// <response code="500">`<see cref="ArgumentNullException"/>`'s and `<see cref="Microsoft.EntityFrameworkCore.DbUpdateException"/>`'s</response>
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [EnvironmentDependant(ApiEnvironments.Development)]
    [HttpPost("join/code/{code}")]
    public async Task<ActionResult<UserDTO>> JoinPlatform(CreateUser newUser, string code)
    {
        if (!await handler.ExistsAsync(code)) {
            return NotFound();
        }

        var createUserResult = await usersHandler.PostAsync((UserDTO) newUser);
        if (createUserResult.Value is null) {
            return createUserResult.Result!;
        }

        return CreatedAtAction(
            nameof(JoinPlatform), 
            new { 
                user_id = createUserResult.Value!.Id,
                token = createUserResult.Value!.Token
            }, 
            createUserResult.Value
        );
    }
}

