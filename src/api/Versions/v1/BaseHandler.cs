using Microsoft.AspNetCore.Mvc;

namespace Homie.Api.v1;

public abstract class BaseHandler<DTO>
{
    protected HttpContext httpContext;

    public BaseHandler(HttpContextAccessor httpContextAccessor)
    {
        if (httpContextAccessor.HttpContext is null) {
            throw new ArgumentNullException(nameof(httpContextAccessor.HttpContext));
        }
        
        httpContext = httpContextAccessor.HttpContext;
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
}

