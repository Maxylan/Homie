// (c) 2024 @Maxylan
namespace Homie.Api.v1;

using Homie.Database;
using Microsoft.AspNetCore.Mvc;

public abstract class BaseHandler<DTO>
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

    public virtual DTO? Get(uint id)
    {
        return GetAsync(id).Result;
    }

    public virtual async Task<DTO?> GetAsync(uint id)
    {
        throw new NotImplementedException();
    }

    public virtual bool Exists(uint? id)
    {
        return id is null ? false : ExistsAsync(id).Result;
    }

    public virtual async Task<bool> ExistsAsync(uint? id)
    {
        if (id is null) { return false; }
        throw new NotImplementedException();
    }

    protected virtual IEnumerable<(string, object)> FilterArgs(params (string, object)[] args)
    {
        if (args.Length > 0) 
        {
            IEnumerable<string> props = typeof(DTO).GetProperties().Select(p => p.Name);
            IEnumerable<string> matches = args.Select(a => a.Item1).Intersect(props);
            return args.Where(a => matches.Contains(a.Item1));
        }
        
        return args;
    }
}