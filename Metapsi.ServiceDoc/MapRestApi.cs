using Metapsi.Sqlite;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Metapsi;

public static partial class ServiceDoc
{
    public static void MapRestApi<T, TId>(this IEndpointRouteBuilder endpoint, DbQueue dbQueue)
    {
        var listRoute = endpoint.MapGet(
            "/",
            async (HttpContext httpContext) =>
            {
                return await dbQueue.ListDocuments<T>();
            }).WithName(Frontend.DocumentRestApiBaseEndpointName<T>());
        ConfigureApiRoute(listRoute);

        var getRoute = endpoint.MapGet(
            "/{id}",
            async (TId id) =>
            {
                var document = await dbQueue.GetDocument<T, TId>(id);

                if (document is T item)
                {
                    return Results.Ok(item);
                }
                return Results.NotFound();
            });
        ConfigureApiRoute(getRoute);

        var saveRoute = endpoint.MapPost(
            "/",
            async (HttpContext httpContext, T item) =>
            {
                await dbQueue.SaveDocument(item);
            });
        ConfigureApiRoute(saveRoute);

        var deleteRoute = endpoint.MapDelete(
            "/{id}",
            async (HttpContext httpContext, TId id) =>
            {
                var doc = await dbQueue.DeleteDocument<T, TId>(id);
                if (doc is T item)
                {
                    return Results.Ok(doc);
                }
                return Results.NotFound();
            });
        ConfigureApiRoute(deleteRoute);
    }
}
