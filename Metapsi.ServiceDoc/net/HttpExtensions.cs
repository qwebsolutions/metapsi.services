﻿using Microsoft.AspNetCore.Builder;
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
            this IEndpointRouteBuilder groupEndpoint,
            DbQueue dbQueue,
            Action<DocsGroup> setProps)
        {
            var docsApp = await CreateDocsApp(dc =>
            {
                dc.UseSqliteQueue(dbQueue);
                setProps(dc);
            });

            return groupEndpoint.MapDocsApp(docsApp);
        }

        public static IEndpointConventionBuilder MapDocsApp(this IEndpointRouteBuilder groupEndpoint, DocsApp docsApp)
        {
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
                await docsApp.WriteInitApiResponse(docType, new Web.CfHttpResponse(httpContext.Response));
            });

            groupEndpoint.MapGet("list/{docType}", async (HttpContext httpContext, string docType) =>
            {
                await docsApp.WriteListDocumentsApiResponse(docType, new Web.CfHttpResponse(httpContext.Response));
            });

            groupEndpoint.MapPost("save/{docType}", async (HttpContext httpContext, string docType) =>
            {
                await docsApp.HandleSaveDocumentApi(docType, new Web.CfHttpContext(httpContext));
            });

            groupEndpoint.MapPost("delete/{docType}", async (HttpContext httpContext, string docType) =>
            {
                await docsApp.HandleDeleteDocumentApi(docType, new Web.CfHttpContext(httpContext));
            });

            return docsRoute;
        }
    }
}
