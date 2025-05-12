using Dapper;
using Metapsi.Sqlite;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace Metapsi
{
    public static partial class ServiceDoc
    {
        public static async Task<IEndpointConventionBuilder> UseDocs(
            this RouteGroupBuilder groupEndpoint,
            DbQueue dbQueue,
            Action<DocsGroup> setProps)
        {
            await EmbeddedFiles.AddAssembly(typeof(Metapsi.ServiceDoc).Assembly);

            var docsApp = await CreateDocsApp(dc =>
            {
                dc.UseSqliteQueue(dbQueue);
                setProps(dc);
            });

            var buildAbsolutePath = (HttpContext httpContext, string relativePath) =>
            {
                var linkGenerator = httpContext.RequestServices.GetRequiredService<LinkGenerator>();
                var rootPath = linkGenerator.GetPathByName(docsApp.EndpointName);
                var absolutePath = rootPath.TrimEnd('/') + "/" + relativePath.TrimStart('/');
                return absolutePath;
            };

            var docsRoute = groupEndpoint.MapGet("/", async (HttpContext httpContext) =>
            {
                var linkGenerator = httpContext.RequestServices.GetRequiredService<LinkGenerator>();
                await docsApp.WriteOverviewHtmlResponse(
                    async (content, contentType) =>
                    {
                        httpContext.Response.ContentType = contentType;
                        await content.CopyToAsync(httpContext.Response.Body);
                    },
                    relativePath => buildAbsolutePath(httpContext, relativePath));
            }).WithName(docsApp.EndpointName);

            var docTypeRoute = groupEndpoint.MapGet("{docType}", async (HttpContext httpContext, string docType) =>
            {
                if (docsApp.docHandlers.TryGetValue(docType, out var handler))
                {
                    var linkGenerator = httpContext.RequestServices.GetRequiredService<LinkGenerator>();
                    await handler.WriteListDocumentsHtmlResponse(
                        async (content, contentType) =>
                        {
                            httpContext.Response.ContentType = contentType;
                            await content.CopyToAsync(httpContext.Response.Body);
                        },
                        relativePath => buildAbsolutePath(httpContext, relativePath));
                }
            });
            groupEndpoint.MapGet("init/{docType}", async (HttpContext httpContext, string docType) =>
            {
                if (docsApp.docHandlers.TryGetValue(docType, out var handler))
                {
                    await handler.WriteInitApiResponse(
                        async (content, contentType) =>
                        {
                            httpContext.Response.ContentType = contentType;
                            await content.CopyToAsync(httpContext.Response.Body);
                        });
                }
            });

            groupEndpoint.MapGet("list/{docType}", async (HttpContext httpContext, string docType) =>
            {
                if (docsApp.docHandlers.TryGetValue(docType, out var handler))
                {
                    await handler.WriteListDocumentsApiResponse(
                        async (content, contentType) =>
                        {
                            httpContext.Response.ContentType = contentType;
                            await content.CopyToAsync(httpContext.Response.Body);
                        });
                }
            });

            groupEndpoint.MapPost("save/{docType}", async (HttpContext httpContext, string docType) =>
            {
                if (docsApp.docHandlers.TryGetValue(docType, out var handler))
                {
                    await handler.WriteSaveDocumentApiResponse(
                        httpContext.Request.Body,
                        async (content, contentType) =>
                        {
                            httpContext.Response.ContentType = contentType;
                            await content.CopyToAsync(httpContext.Response.Body);
                        });
                }
            });

            groupEndpoint.MapPost("delete/{docType}", async (HttpContext httpContext, string docType) =>
            {
                if (docsApp.docHandlers.TryGetValue(docType, out var handler))
                {
                    await handler.WriteDeleteDocumentApiResponse(
                        httpContext.Request.Body,
                        async (content, contentType) =>
                        {
                            httpContext.Response.ContentType = contentType;
                            await content.CopyToAsync(httpContext.Response.Body);
                        });
                }
            });

            return docsRoute;
        }

        public static void UseSqliteQueue(this DocsGroup docsGroup, DbQueue dbQueue)
        {
            docsGroup.defaultInitializer = new DbQueueDefaultActions(dbQueue);
        }
    }
}
