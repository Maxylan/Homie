// (c) 2024 @Maxylan
namespace Homie.Api.v1;

using Homie.Database;
using Homie.Database.Models;
using Microsoft.AspNetCore.Mvc;

public interface iCRUD<DTOT>
{
    public ActionResult<DTOT[]> GetAll(params (string, object)[] args);
    public Task<ActionResult<DTOT[]>> GetAllAsync(params (string, object)[] args);
    public DTOT? Get(uint id);
    public Task<DTOT?> GetAsync(uint id);
    public ActionResult<DTOT> Post(DTOT DTOT, params (string, object)[] args);
    public Task<ActionResult<DTOT>> PostAsync(DTOT DTOT, params (string, object)[] args);
    public ActionResult<DTOT> Put(DTOT DTOT, params (string, object)[] args);
    public Task<ActionResult<DTOT>> PutAsync(DTOT DTOT, params (string, object)[] args);
    public ActionResult Delete(uint id);
    public Task<ActionResult> DeleteAsync(uint id);
}

// TModel = Database Model Type
// DTOT = Data Transfer Object Type

public abstract class BaseCrudHandler<TModel, DTOT> : BaseHandler<TModel, DTOT>, iCRUD<DTOT> 
    where TModel : class, IBaseModel<TModel> 
    where DTOT : class 
{
    public BaseCrudHandler(IHttpContextAccessor httpContextAccessor, HomieDB db) : base(httpContextAccessor, db)
    { }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public virtual ActionResult<DTOT[]> GetAll(params (string, object)[] args)
    {
        return GetAllAsync(args).Result;
    }

    public virtual async Task<ActionResult<DTOT[]>> GetAllAsync(params (string, object)[] args)
    {
        throw new NotImplementedException();
    }

    public virtual ActionResult<DTOT> Post(DTOT DTOT, params (string, object)[] args)
    {
        return PostAsync(DTOT, args).Result;
    }

    public virtual async Task<ActionResult<DTOT>> PostAsync(DTOT DTOT, params (string, object)[] args)
    {
        throw new NotImplementedException();
    }

    public virtual ActionResult<DTOT> Put(DTOT DTOT, params (string, object)[] args)
    {
        return PutAsync(DTOT, args).Result;
    }

    public virtual async Task<ActionResult<DTOT>> PutAsync(DTOT DTOT, params (string, object)[] args)
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

