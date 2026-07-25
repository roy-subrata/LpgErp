using FluentValidation;
using LpgErp.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LpgErp.Api.Filters;

/// <summary>
/// Runs the registered FluentValidation validator for every action argument that has one, so a
/// controller cannot skip validation by forgetting to resolve <see cref="IValidator{T}"/> itself.
/// Arguments with no registered validator pass through untouched.
/// </summary>
public class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var errors = new List<string>();

        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null) continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator) continue;

            var validationContext = new ValidationContext<object>(argument);
            var result = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);
            if (!result.IsValid)
                errors.AddRange(result.Errors.Select(e => e.ErrorMessage));
        }

        if (errors.Count > 0)
        {
            context.Result = new BadRequestObjectResult(ApiResponse.Fail(errors));
            return;
        }

        await next();
    }
}
