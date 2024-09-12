using Metapsi.Sqlite;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Metapsi;

public static partial class ServiceDoc
{
    public static void MapRestApi<T, TId>(this IEndpointRouteBuilder endpoint, SqliteQueue sqliteQueue)
    {
        var listRoute = endpoint.MapGet(
            "/",
            async (HttpContext httpContext) =>
            {
                return await sqliteQueue.Enqueue(ListDocuments<T>);
            }).WithMetadata(new EndpointNameAttribute($"api-{typeof(T).Name}"));
        ConfigureApiRoute(listRoute);

        var getRoute = endpoint.MapGet(
            "/{id}",
            async (TId id) =>
            {
                return await sqliteQueue.Enqueue(async c =>
                {
                    var document = await c.GetDocument<T, TId>(id);
                    if (document is T item)
                    {
                        return Results.Ok(item);
                    }
                    return Results.NotFound();
                });
            });
        ConfigureApiRoute(getRoute);

        var saveRoute = endpoint.MapPost(
            "/",
            async (HttpContext httpContext, T item) =>
            {
                await sqliteQueue.Enqueue(async c => await c.SaveDocument(item));
            });
        ConfigureApiRoute(saveRoute);

        var deleteRoute = endpoint.MapDelete(
            "/{id}",
            async (HttpContext httpContext, TId id) =>
            {
                var deleteResult = await sqliteQueue.WithCommit(async t =>
                {
                    return await t.DeleteReturnDocument<T, TId>(id);
                });
                if (deleteResult != null)
                {
                    return Results.Ok(deleteResult);
                }
                return Results.NotFound();
            });
        ConfigureApiRoute(deleteRoute);
    }
}
