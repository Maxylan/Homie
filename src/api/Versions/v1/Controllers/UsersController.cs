// (c) 2024 @Maxylan
namespace Homie.Api.v1.Controllers;

using Asp.Versioning;
using Homie.Api.v1.Handlers;
using Homie.Api.v1.TransferModels;
using Homie.Utilities.Attributes;
using Microsoft.AspNetCore.Mvc;
using Homie.Database.Models;

[ApiVersion(Version.Numeric, Deprecated = Version.Deprecated)]
[Route("v{v:apiVersion}/users")]
[Produces("application/json")]
[ApiController]
public class UsersController : ControllerBase
{
    protected UsersHandler handler { get; init; }
    protected PlatformsHandler platformsHandler { get; init; }
    public UsersController(UsersHandler _usersHandler, PlatformsHandler _platformsHandler) 
    {
        handler = _usersHandler;
        platformsHandler = _platformsHandler;
    }

    /// <summary>
    /// "GET" The user with PK `id`.
    /// </summary>
    /// <param name="id"><see cref="User.Id"/> "user_id"</param>
    /// <returns><see cref="UserDTO"/></returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /users/id/5
    /// </remarks>
    /// <response code="200">Returns an array of Users</response>
    /// <response code="404">If requested user isn't found</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("id/{id}")]
    public async Task<ActionResult<UserDTO>> GetUser(uint id)
    {
        var result = await handler.GetAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// "GET" The user by its unique `token`.
    /// </summary>
    /// <param name="id"><see cref="User.Id"/> "user_id"</param>
    /// <returns><see cref="UserDTO"/></returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /users/token/asdfghjkl123456789
    /// </remarks>
    /// <response code="200">Returns an array of Users</response>
    /// <response code="404">If requested user isn't found</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("token/{token}")]
    public async Task<ActionResult<UserDTO>> GetUser(string token)
    {
        var result = await handler.GetByTokenAsync(token);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// "GET" A user by a combination of its username and a Platform ID.
    /// </summary>
    /// <param name="platform_id"><see cref="Platform.Id"/> "platform_id"</param>
    /// <param name="username"><see cref="User.Username"/> "username"</param>
    /// <returns><see cref="UserDTO"/></returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /users/platform/5/username/Testylan
    /// </remarks>
    /// <response code="200">Returns an array of Users</response>
    /// <response code="404">If requested user isn't found</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("platform/{platform_id}/username/{username}")]
    public async Task<ActionResult<UserDTO>> GetUserByUsername(uint platform_id, string username)
    {
        if (string.IsNullOrWhiteSpace(username)) 
        {
            return new BadRequestObjectResult(
                new ArgumentNullException(nameof(username), "Username cannot be null or empty.")
            );
        }

        var result = await handler.GetByUsernameAsync(platform_id, username);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// "GET" All users registered to a given platform.
    /// </summary>
    /// <param name="platform_id"><see cref="Platform.Id"/> "platform_id"</param>
    /// <returns><see cref="UserDTO"/>[]</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /users/platform/5
    /// </remarks>
    /// <response code="200">Returns an array of Users</response>
    /// <response code="423">If used in a production build (Locked)</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("platform/{platform_id}")]
    public async Task<ActionResult<UserDTO[]>> GetAllUsersInPlatform(uint platform_id)
    {
        if (!handler.Exists(platform_id)) {
            return NotFound();
        }

        var result = await handler.GetAllAsync(("PlatformId", platform_id));
        return result;
    }

    /// <summary>
    /// (Development) "GET" All users registered in the database.
    /// </summary>
    /// <returns><see cref="UserDTO"/>[]</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /users
    /// </remarks>
    /// <response code="200">Returns an array of Users</response>
    /// <response code="423">If used in a production build (Locked)</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status423Locked)]
    [EnvironmentDependant(ApiEnvironments.Development)]
    [HttpGet]
    public async Task<ActionResult<UserDTO[]>> GetAllUsers()
    {
        var result = await handler.GetAllAsync();
        return result;
    }
}

