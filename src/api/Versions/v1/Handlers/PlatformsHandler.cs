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
/// PlatformsHandler is a scoped service that "handles" the CRUD operations for the `Platform` Controller/Model.
/// </summary>
public class PlatformsHandler : BaseCrudHandler<PlatformDTO>
{
    /// <summary>PlatformsHandler constructor.</summary>
    /// <remarks>
    /// PlatformsHandler is a scoped service that "handles" the CRUD operations for the `Platform` Controller/Model.
    /// </remarks>
    public PlatformsHandler(IHttpContextAccessor httpContextAccessor, HomieDB db) : base(httpContextAccessor, db)
    { }

    /// <summary>
    /// (Development) Get all platforms registered in the database.
    /// </summary>
    /// <param name="args">Variable arguments/filters</param>
    /// <returns><see cref="ActionResult"/>.Value[]</returns>
    public async override Task<ActionResult<PlatformDTO[]>> GetAllAsync(params (string, object)[] args)
    {
        // Dissallow this method in production
        if (!ApiEnvironment.isEnvironment(ApiEnvironments.Development)) {
            return new StatusCodeResult(StatusCodes.Status423Locked);
        }

        IQueryable<Platform> platformTable = db.Platforms;
        args = FilterArgs(args).ToArray();

        if (args.Length > 0) {
            // ..TODO?
        }

        Platform[] platforms = await platformTable.ToArrayAsync() ?? [];
        return platforms.Select(platform => (PlatformDTO) platform).ToArray();
    }

    /// <summary>
    /// Retrieve a platform by its PK (id).
    /// </summary>
    /// <param name="id"></param>
    /// <returns><see cref="PlatformDTO"/>?</returns>
    public async override Task<PlatformDTO?> GetAsync(uint id)
    {
        var platform =  await db.Platforms.FindAsync(id);
        return platform is null ? null : (PlatformDTO) platform;
    }

    /// <summary>
    /// Retrieve a platform by a unique code.
    /// </summary>
    /// <param name="code">"Unique" code</param>
    /// <returns><see cref="PlatformDTO"/>?</returns>
    public async Task<PlatformDTO?> GetByCodeAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) {
            return null;
        }

        var platform =  await db.Platforms.FirstOrDefaultAsync(p => p.GuestCode == code || p.MemberCode == code);
        return platform is null ? null : (PlatformDTO) platform;
    }

    /// <summary>
    /// Create a new platform.
    /// </summary>
    /// <param name="platform"><see cref="PlatformDTO"/></param>
    /// <param name="args">Variable arguments/filters</param>
    /// <returns><see cref="ActionResult"/></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async override Task<ActionResult<PlatformDTO>> PostAsync(PlatformDTO platform, params (string, object)[] args)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Update a platform.
    /// </summary>
    /// <param name="platform"><see cref="PlatformDTO"/></param>
    /// <param name="args">Variable arguments/filters</param>
    /// <returns><see cref="ActionResult"/></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async override Task<ActionResult<PlatformDTO>> PutAsync(PlatformDTO platform, params (string, object)[] args)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// (Development / Administrative) Delete a platform by its PK (id).
    /// </summary>
    /// <param name="id"></param>
    /// <returns><see cref="ActionResult"/></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async override Task<ActionResult> DeleteAsync(uint id)
    {
        // Dissallow this method in production
        if (!ApiEnvironment.isEnvironment(ApiEnvironments.Development)) {
            return new StatusCodeResult(StatusCodes.Status423Locked);
        }

        throw new NotImplementedException();
    }
}

