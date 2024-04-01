// (c) 2024 @Maxylan
namespace Homie.Api.v1;

using Homie.Database;
using Homie.Database.Models;
using Microsoft.AspNetCore.Mvc;

// TModel = Database Model Type
// DTOT = Data Transfer Object Type

public abstract class BaseHandler<TModel, DTOT> 
    where TModel : class, IBaseModel<TModel> 
    where DTOT : class
{
    protected HttpContext httpContext { get; init; }
    protected HomieDB db { get; init; }

    public BaseHandler(IHttpContextAccessor httpContextAccessor, HomieDB db)
    {
        if (httpContextAccessor.HttpContext is null) {
            throw new ArgumentNullException(nameof(httpContextAccessor.HttpContext));
        }
        
        httpContext = httpContextAccessor.HttpContext;
        this.db = db;
    }

    public virtual DTOT? Get(uint id)
    {
        return GetAsync(id).Result;
    }

    public virtual async Task<DTOT?> GetAsync(uint id)
    {
        throw new NotImplementedException();
    }

    public virtual bool Exists(TModel model)
    {
        return model is null ? false : ExistsAsync(model).Result;
    }

    public virtual async Task<bool> ExistsAsync(TModel model)
    {
        if (model is null ) { return false; }
        return await ExistsAsync(model.Id);
    }

    public virtual bool Exists(uint? id)
    {
        return id is null ? false : ExistsAsync(id).Result;
    }

    public virtual async Task<bool> ExistsAsync(uint? id)
    {
        if (id is null) { return false; }
        return await db.Set<TModel>().FindAsync(id) is not null;
    }

    protected virtual IEnumerable<(string, object)> FilterArgs(params (string, object)[] args)
    {
        if (args.Length > 0) 
        {
            IEnumerable<string> props = typeof(DTOT).GetProperties().Select(p => p.Name);
            IEnumerable<string> matches = args.Select(a => a.Item1).Intersect(props);
            return args.Where(a => matches.Contains(a.Item1));
        }
        
        return args;
    }
}