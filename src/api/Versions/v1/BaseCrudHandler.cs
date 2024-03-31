using Microsoft.AspNetCore.Mvc;

namespace Homie.Api.v1;

public interface iCRUD<DTO>
{
    public ActionResult<DTO[]> GetAll();
    public Task<ActionResult<DTO[]>> GetAllAsync();
    public ActionResult<DTO> Get();
    public Task<ActionResult<DTO>> GetAsync();
    public ActionResult<DTO> Post();
    public Task<ActionResult<DTO>> PostAsync();
    public ActionResult<DTO> Put();
    public Task<ActionResult<DTO>> PutAsync();
    public ActionResult Delete();
    public Task<ActionResult> DeleteAsync();
}

public abstract class BaseCrudHandler<DTO> : BaseHandler<DTO>, iCRUD<DTO>
{
    public BaseCrudHandler(HttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
    { }

    public virtual ActionResult<DTO[]> GetAll()
    {
        return GetAllAsync().Result;
    }

    public virtual async Task<ActionResult<DTO[]>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public virtual ActionResult<DTO> Get()
    {
        return GetAsync().Result;
    }

    public virtual async Task<ActionResult<DTO>> GetAsync()
    {
        throw new NotImplementedException();
    }

    public virtual ActionResult<DTO> Post()
    {
        return PostAsync().Result;
    }

    public virtual async Task<ActionResult<DTO>> PostAsync()
    {
        throw new NotImplementedException();
    }

    public virtual ActionResult<DTO> Put()
    {
        return PutAsync().Result;
    }

    public virtual async Task<ActionResult<DTO>> PutAsync()
    {
        throw new NotImplementedException();
    }

    public virtual ActionResult Delete()
    {
        return DeleteAsync().Result;
    }

    public virtual async Task<ActionResult> DeleteAsync()
    {
        throw new NotImplementedException();
    }
}

