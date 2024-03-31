using Microsoft.AspNetCore.Mvc;

namespace Homie.Api.v1;

public interface iCRUD<DTO>
{
    public ActionResult<DTO[]> GetAll(params object[] args);
    public Task<ActionResult<DTO[]>> GetAllAsync(params object[] args);
    public ActionResult<DTO> Get(uint id);
    public Task<ActionResult<DTO>> GetAsync(uint id);
    public ActionResult<DTO> Post(DTO dto, params object[] args);
    public Task<ActionResult<DTO>> PostAsync(DTO dto, params object[] args);
    public ActionResult<DTO> Put(DTO dto, params object[] args);
    public Task<ActionResult<DTO>> PutAsync(DTO dto, params object[] args);
    public ActionResult Delete(uint id);
    public Task<ActionResult> DeleteAsync(uint id);
}

public abstract class BaseCrudHandler<DTO> : BaseHandler<DTO>, iCRUD<DTO>
{
    public BaseCrudHandler(HttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
    { }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public virtual ActionResult<DTO[]> GetAll(params object[] args)
    {
        return GetAllAsync(args).Result;
    }

    public virtual async Task<ActionResult<DTO[]>> GetAllAsync(params object[] args)
    {
        throw new NotImplementedException();
    }

    public virtual ActionResult<DTO> Get(uint id)
    {
        return GetAsync(id).Result;
    }

    public virtual async Task<ActionResult<DTO>> GetAsync(uint id)
    {
        throw new NotImplementedException();
    }

    public virtual ActionResult<DTO> Post(DTO dto, params object[] args)
    {
        return PostAsync(dto, args).Result;
    }

    public virtual async Task<ActionResult<DTO>> PostAsync(DTO dto, params object[] args)
    {
        throw new NotImplementedException();
    }

    public virtual ActionResult<DTO> Put(DTO dto, params object[] args)
    {
        return PutAsync(dto, args).Result;
    }

    public virtual async Task<ActionResult<DTO>> PutAsync(DTO dto, params object[] args)
    {
        throw new NotImplementedException();
    }

    public virtual ActionResult Delete(uint id)
    {
        return DeleteAsync(id).Result;
    }

    public virtual async Task<ActionResult> DeleteAsync(uint id)
    {
        throw new NotImplementedException();
    }
#pragma warning restore CS1998
}

