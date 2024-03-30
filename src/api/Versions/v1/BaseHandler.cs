using Microsoft.AspNetCore.Mvc;

namespace Homie.Api.v1
{
    public abstract class BaseHandler<T> where T : class
    {
        protected HttpContext httpContext;

        public BaseHandler(HttpContextAccessor httpContextAccessor)
        {
            if (httpContextAccessor.HttpContext is null) {
                throw new ArgumentNullException(nameof(httpContextAccessor.HttpContext));
            }
            httpContext = httpContextAccessor.HttpContext;
        }

        abstract public IActionResult Get();
    }
}

