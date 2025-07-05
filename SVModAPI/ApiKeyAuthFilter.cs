using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SVModAPI;


[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ApiKeyAuthFilter : Attribute, IAsyncActionFilter
{
    private const string ApiKeyHeaderName = "X-API-Key";
        
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
        {
            context.Result = new UnauthorizedObjectResult(new { error = "Unauthorized", message = "API Key missing." });
            return;
        }

        var apiKeyStore = context.HttpContext.RequestServices.GetRequiredService<ApiKeyStore>();
        var apiKeyString = apiKeyStore.ApiKey;

        if (apiKeyString != extractedApiKey)
        {
            context.Result = new UnauthorizedObjectResult(new { error = "Unauthorized", message = "Invalid API Key." });
            return;
        }

        await next();
    }
}