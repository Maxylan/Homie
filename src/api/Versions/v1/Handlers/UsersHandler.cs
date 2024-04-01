using Homie.Api.v1.TransferModels;
using Homie.Database;
using Homie.Database.Models;
using Homie.Utilities.Attributes;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    public UsersHandler(HttpContextAccessor httpContextAccessor, HomieDB db) : base(httpContextAccessor, db)
    { }

    /// <summary>
    /// (Development) Get all users registered in the database.
    /// </summary>
    /// <param name="args">Variable arguments/filters</param>
    /// <returns><see cref="ActionResult"/>.Value[]</returns>
    public async override Task<ActionResult<UserDTO[]>> GetAllAsync(params (string, object)[] args)
    {
        // Dissallow this method in production
        if (!ApiEnvironment.isEnvironment(ApiEnvironments.Development)) {
            return new StatusCodeResult(StatusCodes.Status423Locked);
        }

        IQueryable<User> usersTable = db.Users;
        args = FilterArgs(args).ToArray();

        if (args.Length > 0) {
            foreach ((string key, object value) in args) {
                switch (key) {
                    case "PlatformId":
                        usersTable = usersTable.Where(u => u.PlatformId == (uint) value);
                        break;
                    default:
                        break;
                }
            }
        }

        User[] users = await usersTable.ToArrayAsync() ?? [];
        return users.Select(user => (UserDTO) user).ToArray();
    }

    /// <summary>
    /// Retrieve a user by its PK (id).
    /// </summary>
    /// <param name="id"></param>
    /// <returns><see cref="UserDTO"/>?</returns>
    /// <exception cref="NotImplementedException"></exception>
    public async override Task<UserDTO?> GetAsync(uint id)
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
    public async Task<ActionResult<PlatformDTO>> GetByUsernameAsync(string? username, uint platform_id)
    {
        if (string.IsNullOrWhiteSpace(username)) {
            return new BadRequestObjectResult(new ArgumentNullException(nameof(username), "Username cannot be null or empty."));
        }

        throw new NotImplementedException();
    }

    /// <summary>
    /// Create a new user.
    /// </summary>
    /// <param name="user"><see cref="UserDTO"/></param>
    /// <param name="args">Variable arguments/filters</param>
    /// <returns><see cref="ActionResult"/></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async override Task<ActionResult<UserDTO>> PostAsync(UserDTO user, params (string, object)[] args)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Update a user.
    /// </summary>
    /// <param name="user"><see cref="UserDTO"/></param>
    /// <param name="args">Variable arguments/filters</param>
    /// <returns><see cref="ActionResult"/></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async override Task<ActionResult<UserDTO>> PutAsync(UserDTO user, params (string, object)[] args)
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

