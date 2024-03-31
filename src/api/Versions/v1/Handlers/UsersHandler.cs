using Homie.Api.v1.TransferModels;
using Homie.Database.Models;
using Homie.Utilities.Attributes;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Homie.Api.v1.Handlers;

/// <summary>
/// UsersHandler is a scoped service that "handles" the CRUD operations for the `User` Controller/Model.
/// </summary>
public class UsersHandler : BaseCrudHandler<UserDTO>
{
    /// <summary>UsersHandler constructor.</summary>
    /// <remarks>
    /// UsersHandler is a scoped service that "handles" the CRUD operations for the `User` Controller/Model.
    /// </remarks>
    public UsersHandler(HttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
    { }

    /// <summary>
    /// (Development) Get all users registered in the database.
    /// </summary>
    /// <param name="args">Variable arguments</param>
    /// <returns><see cref="ActionResult"/>.Value[]</returns>
    public async override Task<ActionResult<UserDTO[]>> GetAllAsync(params object[] args)
    {
        // Dissallow this method in production
        if (!ApiEnvironment.isEnvironment(ApiEnvironments.Development)) {
            return new StatusCodeResult(StatusCodes.Status423Locked);
        }

        return new OkResult();
    }

    /// <summary>
    /// Retrieve a user by its PK (id).
    /// </summary>
    /// <param name="id"></param>
    /// <returns><see cref="ActionResult"/></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async override Task<ActionResult<UserDTO>> GetAsync(uint id)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Retrieve a user by its Username + Platform (platform_id).
    /// </summary>
    /// <param name="username"><see cref="User.Username"/></param>
    /// <param name="platform_id"><see cref="Platform.Id"/> "platform_id"</param>
    /// <returns><see cref="ActionResult"/></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<ActionResult<PlatformDTO>> GetAsync(string? username, uint platform_id)
    {
        if (string.IsNullOrWhiteSpace(username)) {
            return new BadRequestObjectResult(new ArgumentNullException(nameof(username), "Username cannot be null or empty."));
        }

        throw new NotImplementedException();
    }

    /// <summary>
    /// Create a new platform.
    /// </summary>
    /// <param name="platform"><see cref="UserDTO"/></param>
    /// <param name="args">Variable arguments</param>
    /// <returns><see cref="ActionResult"/></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async override Task<ActionResult<UserDTO>> PostAsync(UserDTO platform, params object[] args)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Update a platform.
    /// </summary>
    /// <param name="platform"><see cref="UserDTO"/></param>
    /// <param name="args">Variable arguments</param>
    /// <returns><see cref="ActionResult"/></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async override Task<ActionResult<UserDTO>> PutAsync(UserDTO platform, params object[] args)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// (Administrative) Delete a user by its PK (id).
    /// </summary>
    /// <param name="id"></param>
    /// <returns><see cref="ActionResult"/></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async override Task<ActionResult> DeleteAsync(uint id)
    {
        throw new NotImplementedException();
    }
}

