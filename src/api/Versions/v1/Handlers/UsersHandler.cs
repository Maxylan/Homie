// (c) 2024 @Maxylan
namespace Homie.Api.v1.Handlers;

using Homie.Api.v1.TransferModels;
using Homie.Database;
using Homie.Database.Models;
using Homie.Utilities.Attributes;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        else {
            // Dissallow this method in production
            if (!ApiEnvironment.isEnvironment(ApiEnvironments.Development)) {
                return new StatusCodeResult(StatusCodes.Status423Locked);
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
    public async override Task<UserDTO?> GetAsync(uint id)
    {
        var user = await db.Users.FindAsync(id);
        return user is null ? null : (UserDTO) user;
    }

    /// <summary>
    /// Retrieve a user by a unique token.
    /// </summary>
    /// <param name="token"></param>
    /// <returns><see cref="UserDTO"/>?</returns>
    public async Task<UserDTO?> GetByTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) {
            return null;
        }

        var user = await db.Users.FirstOrDefaultAsync(p => p.Token == token);
        return user is null ? null : (UserDTO) user;
    }

    /// <summary>
    /// Retrieve a user by its Username + Platform (platform_id).
    /// </summary>
    /// <param name="platform_id"><see cref="Platform.Id"/> "platform_id"</param>
    /// <param name="username"></param>
    /// <returns><see cref="UserDTO"/>?</returns>
    public async Task<UserDTO?> GetByUsernameAsync(uint platform_id, string? username)
    {
        if (string.IsNullOrWhiteSpace(username)) {
            return null;
        }

        var user = await db.Users.FirstOrDefaultAsync(p => p.PlatformId == platform_id && p.Username == username);
        return user is null ? null : (UserDTO) user;
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

