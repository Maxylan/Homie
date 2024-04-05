// (c) 2024 @Maxylan
namespace Homie.Api.v1.Handlers;

using System.Reflection;
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
public class PlatformsHandler : BaseCrudHandler<Platform, PlatformDTO>
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
    /// <param name="dto"><see cref="PlatformDTO"/></param>
    /// <param name="args">Variable arguments/filters</param>
    /// <returns><see cref="ActionResult"/></returns>
    /// <exception cref="DbUpdateException">If environment is "Development", cought in "Production".</exception>
    public async override Task<ActionResult<PlatformDTO>> PostAsync(PlatformDTO dto, params (string, object)[] args)
    {
        dto.GuestCode ??= await GenerateUniqueCodeAsync();
        dto.MemberCode ??= await GenerateUniqueCodeAsync();
        dto.ResetToken ??= GenerateUniqueTokenAsync();
        
        Platform platform;
        try {
            platform = dto.ToModel();
            db.Platforms.Add(platform);
            await db.SaveChangesAsync();
        }
        catch(ArgumentNullException nullException) 
        {
            // Let the `ArgumentNullException` format the returned message.
            return new BadRequestObjectResult($"{nullException.Message}");
        }
        catch(DbUpdateException dbException) 
        {
            if (Backoffice.isProduction) {
                Console.WriteLine($"Cought `DbUpdateException` \"{dbException.Message}\"");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
            else {
                throw dbException;
            }
        }

        return (PlatformDTO) platform;
    }

    /// <summary>
    /// Regenerate a platform code.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="userGroup">`<see cref="UserGroup"/>` corresponding to either `<see cref="Platform.GuestCode"/>` or `<see cref="Platform.MemberCode"/>`</param>
    /// <returns><see cref="ActionResult"/></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<ActionResult<string>> RegenerateCodeAsync(uint id, UserGroup? userGroup)
    {
        if (userGroup is null) {
            return new BadRequestObjectResult("userGroup cannot be null."); // 400
        }

        Platform? platform = await db.Platforms.FindAsync(id);

        if (platform is null) {
            return new NotFoundObjectResult($"Platform with ID {id} cannot be found."); // 404
        }

        string code = await GenerateUniqueCodeAsync();
        switch (userGroup) { // userGroup can be equated to code used in this context.
            case UserGroup.Member:
                platform.MemberCode = code;
                break;
            case UserGroup.Guest:
                platform.GuestCode = code;
                break;
            default:
                return new BadRequestObjectResult("Invalid property."); // 400
        }
        
        try {
            db.Platforms.Update(platform);
            await db.SaveChangesAsync();
        }
        catch(DbUpdateException dbException) 
        {
            if (Backoffice.isProduction) {
                Console.WriteLine($"Cought `DbUpdateException` \"{dbException.Message}\"");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
            else {
                throw dbException;
            }
        }
        
        return new NoContentResult();
    }

#pragma warning disable CS1998
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
#pragma warning restore CS1998

    #region Helpers/Utilities & Special operations

    public bool Exists(string? code)
    {
        return string.IsNullOrWhiteSpace(code) ? false : ExistsAsync(code).Result;
    }

    public async Task<bool> ExistsAsync(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) { return false; }
        return await db.Platforms.FirstOrDefaultAsync(p => p.GuestCode == code || p.MemberCode == code) is not null;
    }
    
    /// <summary>
    /// Converts a code to its corresponding group by querying the 'platforms' table 
    /// in the database.
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public UserGroup? DetermineGroup(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) {
            return null;
        }

        return DetermineGroupAsync(code).Result;
    }

    /// <summary>
    /// Asynchronously converts a code to its corresponding group by querying the 
    /// 'platforms' table in the database.
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public async Task<UserGroup?> DetermineGroupAsync(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) {
            return null;
        }

        Platform? platform = await db.Platforms.FirstOrDefaultAsync(p => p.GuestCode == code || p.MemberCode == code);
        if (platform is null) {
            return null;
        }

        return code == platform.MemberCode 
            ? UserGroup.Member 
            : UserGroup.Guest;
    }

    /// <summary>
    /// Asynchronously converts a code to its corresponding group by querying the 
    /// 'platforms' table in the database.
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public async Task<(uint, UserGroup)?> DeterminePlatformDetailsAsync(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) {
            return null;
        }

        Platform? platform = await db.Platforms.FirstOrDefaultAsync(p => p.GuestCode == code || p.MemberCode == code);
        if (platform is null) {
            return null;
        }

        return (
            (uint) platform.Id!,
            code == platform.MemberCode 
                ? UserGroup.Member 
                : UserGroup.Guest
        );
    }

    /// <summary>
    /// Generate a new, unique, "Member" or "Guest" codes for platforms.
    /// </summary>
    /// <param name="length">Optional, Default = 6. Skips call to "substring" if 0 is passed (length = 0)</param>
    /// <param name="upper">Optional, automatically uppercases all letters.</param>
    /// <param name="format">Optional, Default = "N"</param>
    /// <returns>string (<see cref="Guid"/>)</returns>
    private async Task<string> GenerateUniqueCodeAsync(uint length = 6, bool upper = true, string format = "N")
    {
        if (length > 24) { length = 24; } // Max out at 24.
        string code = string.Empty;
        do {
            code = Guid.NewGuid().ToString(format);
            if (length > 0) {
                code = code.Substring(0, (int) length);
            } 
        } 
        while (await db.Platforms.AnyAsync(p => p.GuestCode == code || p.MemberCode == code));

        if (upper) {
            return code.ToUpper();
        } 

        return code;
    }

    /// <summary>
    /// Generate a new 24-character long "Reset Token" for platforms. @see <see cref="Guid.ToString()"/>
    /// </summary>
    /// <param name="format">Optional, Default = "N"</param>
    /// <param name="provider">Optional, @see https://learn.microsoft.com/en-us/dotnet/api/system.iformatprovider?view=net-8.0</param>
    /// <returns>string (<see cref="Guid"/>)</returns>
    private string GenerateUniqueTokenAsync(string format = "N", IFormatProvider? provider = null) => 
        Guid.NewGuid().ToString(format, provider).Substring(0, 24);

    #endregion
}

