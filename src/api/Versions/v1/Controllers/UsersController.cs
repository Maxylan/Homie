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

