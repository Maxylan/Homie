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

    public async override Task<ActionResult<PlatformDTO[]>> GetAllAsync()
    {
        // Dissallow this method in production
        if (!ApiEnvironment.isEnvironment(ApiEnvironments.Development)) {
            return new StatusCodeResult(StatusCodes.Status405MethodNotAllowed);
        }

        return new OkResult();
    }

    public async override Task<ActionResult<PlatformDTO>> GetAsync()
    {
        throw new NotImplementedException();
    }

    public async override Task<ActionResult<PlatformDTO>> PostAsync()
    {
        throw new NotImplementedException();
    }

    public async override Task<ActionResult<PlatformDTO>> PutAsync()
    {
        throw new NotImplementedException();
    }

    public async override Task<ActionResult> DeleteAsync()
    {
        throw new NotImplementedException();
    }
}

