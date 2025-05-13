using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Metapsi.Html;
using Metapsi.Shoelace;
using Metapsi.Syntax;

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
            await EmbeddedFiles.Load.HtmlEmbeddedFiles();
            await EmbeddedFiles.Load.ShoelaceEmbeddedFiles();
            await EmbeddedFiles.Load.SyntaxCoreEmbeddedFiles();

            var docsApp = await CreateDocsApp(dc =>
            {
                dc.UseSqliteQueue(dbQueue);
                setProps(dc);
            });

            var getPathLocator = (HttpContext httpContext) =>
            {
                return (RouteDescription routeDescription) =>
                {
                    var linkGenerator = httpContext.RequestServices.GetRequiredService<LinkGenerator>();
                    var rootPath = linkGenerator.GetPathByName(docsApp.EndpointName);
                    var relativePath = string.Empty;

                    switch (routeDescription.Name)
                    {
                        case "list-documents-page":
                            relativePath = routeDescription.Get("docType");
                            break;
                        default:
                            relativePath = routeDescription.Name.Replace("-api", string.Empty) + "/" + routeDescription.Get("docType");
                            break;
                    }

                    return rootPath.TrimEnd('/') + "/" + relativePath.TrimStart('/');
                };
            };

            var docsRoute = groupEndpoint.MapGet("/", async (HttpContext httpContext) =>
            {
                var rootHtml = await docsApp.GetRootPage(getPathLocator(httpContext));
                await httpContext.WriteHtmlDocumentResponse(rootHtml);
            }).WithName(docsApp.EndpointName);

            var docTypeRoute = groupEndpoint.MapGet("{docType}", async (HttpContext httpContext, string docType) =>
            {
                var listDocumentsPage = await docsApp.GetListDocumentsPage(docType, getPathLocator(httpContext));
                await httpContext.WriteHtmlDocumentResponse(listDocumentsPage);
            });

            groupEndpoint.MapGet("init/{docType}", async (HttpContext httpContext, string docType) =>
            {
                await docsApp.WriteInitApiResponse(docType, new Web.HttpResponse(httpContext.Response));
            });

            groupEndpoint.MapGet("list/{docType}", async (HttpContext httpContext, string docType) =>
            {
                await docsApp.WriteListDocumentsApiResponse(docType, new Web.HttpResponse(httpContext.Response));
            });

            groupEndpoint.MapPost("save/{docType}", async (HttpContext httpContext, string docType) =>
            {
                await docsApp.HandleSaveDocumentApi(docType, new Web.HttpContext(httpContext));
            });

            groupEndpoint.MapPost("delete/{docType}", async (HttpContext httpContext, string docType) =>
            {
                await docsApp.HandleDeleteDocumentApi(docType, new Web.HttpContext(httpContext));
            });

            return docsRoute;
        }
    }
}
