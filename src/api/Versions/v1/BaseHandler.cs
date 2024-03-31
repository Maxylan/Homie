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
}

