using GestionPersonal.Api.Extensions;
using GestionPersonal.Application.Centers;
using GestionPersonal.Application.Workers;

namespace GestionPersonal.Api.Endpoints;

public static class CentersEndpoints
{
    public static IEndpointRouteBuilder MapCentersEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/centers", async (ListCentersQueryHandler handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new ListCentersQuery(), cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ToProblemDetails();
        });

        app.MapGet("/api/centers/{id:int}/workers", async (int id, ListWorkersByCenterQueryHandler handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new ListWorkersByCenterQuery(id), cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ToProblemDetails();
        });

        return app;
    }
}
