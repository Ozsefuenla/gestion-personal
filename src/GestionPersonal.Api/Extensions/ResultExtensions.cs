using GestionPersonal.Application.Common;

namespace GestionPersonal.Api.Extensions;

public static class ResultExtensions
{
    public static IResult ToProblemDetails(this Result result)
    {
        var statusCode = result.Error.Code switch
        {
            "NotFound" => StatusCodes.Status404NotFound,
            "Conflict" => StatusCodes.Status409Conflict,
            "Validation" => StatusCodes.Status400BadRequest,
            "Forbidden" => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status400BadRequest
        };

        return Results.Problem(
            statusCode: statusCode,
            title: result.Error.Message,
            type: result.Error.Code);
    }
}
