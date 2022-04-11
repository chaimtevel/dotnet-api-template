using Microsoft.AspNetCore.Mvc.Filters;
using static Api.Extensions.AspNetCoreExtensions;

namespace Api.Mvc;

public class BadRequestActionFilter : ActionFilterAttribute
{
    public override void OnActionExecuted(ActionExecutedContext context)
    {
        base.OnActionExecuted(context);

        if (!context.ModelState.IsValid)
        {
            context.Result = context.ModelState.FormatBadRequestResult();
        }
    }
}