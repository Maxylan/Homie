namespace Homie.Utilities.Attributes;

using Homie.Database.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public enum ApiEnvironments
{
    Development,
    Production
}

public class ApiEnvironment
{
    public static bool isEnvironment(ApiEnvironments apiEnvironment) { 
        switch(apiEnvironment) {
            case ApiEnvironments.Development:
                return Backoffice.isDevelopment;
            case ApiEnvironments.Production:
                return Backoffice.isProduction;
            default:
                return false;
        }
    }
}

[System.AttributeUsage(System.AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public class EnvironmentDependantAttribute : ActionFilterAttribute
{
    protected ApiEnvironments dependsOn { get; init; }
    protected bool isApiEnvironment() => 
        ApiEnvironment.isEnvironment(dependsOn);

    public EnvironmentDependantAttribute(ApiEnvironments apiEnvironment) 
    {
        dependsOn = apiEnvironment;
    }

    /// <summary>
    /// This method is called before the action method is invoked.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next) 
    {
        if (isApiEnvironment()) {
            await next();
        }
        else {
            context.Result = new StatusCodeResult(StatusCodes.Status423Locked);
        }
        
        return;
    }
}