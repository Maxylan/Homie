using Homie.Api.v1.TransferModels;
using Homie.Database.Models;
using Homie.Utilities.Attributes;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Homie.Api.v1.Handlers;

/// <summary>
/// PlatformHandler is a scoped service that "handles" the CRUD operations for the `Platform` Controller/model.
/// </summary>
public class PlatformHandler : BaseCrudHandler<PlatformDTO>
{
    public PlatformHandler(HttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
    { }

    public async override Task<ActionResult<PlatformDTO[]>> GetAllAsync(params object[] args)
    {
        // Dissallow this method in production
        if (!ApiEnvironment.isEnvironment(ApiEnvironments.Development)) {
            return new StatusCodeResult(StatusCodes.Status423Locked);
        }

        return new OkResult();
    }

    public async override Task<ActionResult<PlatformDTO>> GetAsync(uint id)
    {
        throw new NotImplementedException();
    }

    public async override Task<ActionResult<PlatformDTO>> PostAsync(PlatformDTO platform)
    {
        throw new NotImplementedException();
    }

    public async override Task<ActionResult<PlatformDTO>> PutAsync(PlatformDTO platform)
    {
        throw new NotImplementedException();
    }

    public async override Task<ActionResult> DeleteAsync(uint id)
    {
        throw new NotImplementedException();
    }
}

