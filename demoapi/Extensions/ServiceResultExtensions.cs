using demoapi.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace demoapi.Extensions;

public static class ServiceResultExtensions
{
    public static IActionResult ToActionResult<T>(
        this ControllerBase controller,
        ServiceResult<T> result,
        Func<T, IActionResult>? onSuccess = null)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentNullException.ThrowIfNull(result);

        return result.Outcome switch
        {
            Outcome.Ok => onSuccess is null
                ? controller.Ok(result.Value)
                : onSuccess(result.Value!),

            Outcome.NotFound =>
                controller.NotFound(new { error = result.Error }),

            Outcome.Conflict =>
                controller.Conflict(new { error = result.Error }),

            Outcome.BadRequest =>
                controller.BadRequest(new { error = result.Error }),

            _ => throw new InvalidOperationException(
                $"Unsupported service outcome: {result.Outcome}.")
        };
    }
}