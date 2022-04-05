using Microsoft.AspNetCore.Mvc.ModelBinding;
using Shared.Validation;

namespace Shared.Extensions;

public static class ValidationExtensions
{
    public static ModelStateDictionary AddModelError(this ModelStateDictionary modelState, (string code, string message) error)
    {
        modelState.AddModelError(error.code, error.message);

        return modelState;
    }

    public static ServiceValidation CopyToModelState(this ServiceValidation serviceValidation, ModelStateDictionary modelState)
    {
        serviceValidation.Each(e => modelState.AddModelError(e.Code, e.Message));
        return serviceValidation;
    }
}
