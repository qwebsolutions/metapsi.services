using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Shoelace;
using Metapsi.Syntax;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;

namespace Metapsi
{
    public static partial class ServiceDoc
    {
        public class DocsGroup
        {
            // The tasks handle the DocumentProps inside

            internal List<Task> registerDocs = new();
            internal List<Func<CommandContext, HttpContext, Task<DocTypeOverview>>> getOverview = new();

            internal IEndpointRouteBuilder uiEndpoint { get; set; }
            internal ApplicationSetup applicationSetup { get; set; }
            internal ImplementationGroup ig { get; set; }
            internal string dbPath { get; set; }

            public Action<RouteHandlerBuilder> GroupRoutesBuilder { get; set; }
        }

        public class DocumentProps<T>
        {
            public System.Linq.Expressions.Expression<Func<T, string>> IdProperty { get; set; }
            public Func<CommandContext, Task<T>> Create { get; set; }
            public Func<CommandContext, Task<List<T>>> List { get; set; }
            public List<string> TableColumns { get; set; } = DataTable.GetColumns<T>();
            public Action<RouteHandlerBuilder> DocumentRoutesBuilder { get; set; }
        }

        public static void AddDoc<T>(
            this DocsGroup docsGroup,
            Expression<Func<T, string>> idProperty,
            Action<DocumentProps<T>> setProps = null)
            where T : new()
        {
            docsGroup.AddDoc(
                idProperty,
                async (cc) => new T(),
                setProps);
        }

        public static void AddDoc<T>(
            this DocsGroup docsGroup,
            Expression<Func<T, string>> idProperty,
            Func<CommandContext, Task<T>> createDocument,
            Action<DocumentProps<T>> setProps = null)
        {
            DocumentProps<T> docProps = new DocumentProps<T>() { IdProperty = idProperty };
            docProps.Create = createDocument;
            if (setProps != null)
            {
                setProps(docProps);
            }
            if (docProps.List == null) docProps.List = async (cc) => await cc.Do(ServiceDoc.GetDocApi<T>().List);

            var typeName = typeof(T).Name;
            var typeEndpoint = docsGroup.uiEndpoint.MapGroup(typeName);

            docsGroup.registerDocs.Add(
                FillTypeEndpoint(
                    typeEndpoint,
                    docsGroup,
                    docProps));
            docsGroup.getOverview.Add(async (CommandContext commandContext, HttpContext httpContext) =>
            {
                var linkGenerator = httpContext.RequestServices.GetRequiredService<LinkGenerator>();
                var apiUrl = linkGenerator.GetPathByName(httpContext, $"api-{typeof(T).Name}");
                var listUrl = "/" + string.Join("/", apiUrl.Split("/", StringSplitOptions.RemoveEmptyEntries).SkipLast(1).Append("list"));
                var count = await commandContext.Do(GetDocApi<T>().Count);
                return new DocTypeOverview()
                {
                    DocTypeName = typeof(T).Name,
                    Count = count,
                    ListUrl = listUrl,
                    ApiUrl = apiUrl
                };
            });
        }

        public static void SetDefaultColumns<T>(this DocumentProps<T> b, params string[] columnNames)
        {
            b.TableColumns = columnNames.ToList();
        }

        public static void ConfigureGroupRoutes(this DocsGroup docsGroup, Action<RouteHandlerBuilder> builder)
        {
            docsGroup.GroupRoutesBuilder = builder;
        }

        public static void ConfigureDocumentRoutes<T>(this DocumentProps<T> b, Action<RouteHandlerBuilder> builder)
        {
            b.DocumentRoutesBuilder = builder;
        }

        public static Request<T> InitDocument<T>() => new Request<T>(nameof(InitDocument));
        public static Request<List<T>> ListDocuments<T>() => new Request<List<T>>(nameof(ListDocuments));

        public static async Task<RouteHandlerBuilder> UseDocs(
            this IEndpointRouteBuilder uiEndpoint,
            ApplicationSetup applicationSetup,
            ImplementationGroup ig,
            string dbPath,
            Action<DocsGroup> setProps)
        {
            var propsConfigurator = new DocsGroup()
            {
                applicationSetup = applicationSetup,
                dbPath = dbPath,
                ig = ig,
                uiEndpoint = uiEndpoint
            };
            setProps(propsConfigurator);

            // Execute sequentially because they share the same db
            foreach(var task in propsConfigurator.registerDocs)
            {
                await task;
            }

            uiEndpoint.Render<DocsOverviewModel>(ServiceDoc.Render);
            var docsRoute = uiEndpoint.MapGet("/docs", async (CommandContext commandContext, HttpContext httpContext) =>
            {
                var docsOverviewModel = new DocsOverviewModel();
                foreach (var getOverview in propsConfigurator.getOverview)
                {
                    docsOverviewModel.DocTypes.Add(await getOverview(commandContext, httpContext));
                }

                return Page.Result(docsOverviewModel);
            });

            if (propsConfigurator.GroupRoutesBuilder != null)
            {
                propsConfigurator.GroupRoutesBuilder(docsRoute);
            }

            return docsRoute;
        }

        private static async Task FillTypeEndpoint<T>(
            this IEndpointRouteBuilder typeEndpoint,
            DocsGroup docsGroup,
            DocumentProps<T> documentProps)
        {
            typeEndpoint.RegisterDocUiHandlers<T>(docsGroup, documentProps);

            var apiEndpoint = typeEndpoint.MapGroup("api");
            apiEndpoint.RegisterFrontendRestApi<T>(docsGroup, documentProps);

            await docsGroup.applicationSetup.RegisterDocBackendApi<T>(docsGroup.ig, docsGroup.dbPath, documentProps.IdProperty);

            typeEndpoint.Render<ServiceDoc.ListDocsPage<T>>((b, model) => Metapsi.ServiceDoc.Render(b, model, documentProps.IdProperty));

            var noActualState = docsGroup.applicationSetup.AddBusinessState(new object());

            var initRoute = apiEndpoint.MapGet(
                InitDocument<T>().Name,
                async (CommandContext commandContext, HttpContext httpContext) =>
                {
                    return await documentProps.Create(commandContext);
                });

            var listRoute = apiEndpoint.MapGet(
                ListDocuments<T>().Name,
                async (CommandContext commandContext, HttpContext httpContext) =>
                {
                    return await documentProps.List(commandContext);
                });

            if (documentProps.DocumentRoutesBuilder == null)
            {
                if (docsGroup.GroupRoutesBuilder == null)
                {
                    initRoute.AllowAnonymous();
                    listRoute.AllowAnonymous();
                }
                else
                {
                    docsGroup.GroupRoutesBuilder(initRoute);
                    docsGroup.GroupRoutesBuilder(listRoute);
                }
            }
            else
            {
                documentProps.DocumentRoutesBuilder(initRoute);
                documentProps.DocumentRoutesBuilder(listRoute);
            }
        }

        //public static async Task<string> UseDocs<T>(
        //    this IEndpointRouteBuilder uiEndpoint,
        //    ApplicationSetup applicationSetup,
        //    ImplementationGroup ig,
        //    string dbPath,
        //    System.Linq.Expressions.Expression<Func<T, string>> idProperty,
        //    List<string> columns,
        //    Func<CommandContext, Task<T>> createDocument,
        //    Func<CommandContext, Task<List<T>>> listDocuments = null)
        //{
        //    var typeName = typeof(T).Name;
        //    var typeEndpoint = uiEndpoint.MapGroup(typeName);
        //    await FillTypeEndpoint(typeEndpoint, applicationSetup, ig, dbPath, idProperty, columns, createDocument, listDocuments);
        //    return typeName;
        //}

        public static void Render<TModel>(this IEndpointRouteBuilder uiEndpoint, System.Action<HtmlBuilder, TModel> buildPage)
        {
            uiEndpoint.UseRenderer<TModel>(model =>
            {
                var document = HtmlBuilder.FromDefault(b => buildPage(b, model));
                return document.ToHtml();
            });
        }

        private static void ConfigureRoute<T>(RouteHandlerBuilder route, DocsGroup docsGroup, DocumentProps<T> documentProps)
        {
            if (documentProps.DocumentRoutesBuilder == null)
            {
                if (docsGroup.GroupRoutesBuilder == null)
                {
                    route.AllowAnonymous();
                }
                else
                {
                    docsGroup.GroupRoutesBuilder(route);
                }
            }
            else
            {
                documentProps.DocumentRoutesBuilder(route);
            }
        }

        private static void ConfigureApiRoute(RouteHandlerBuilder route)
        {
            route.RequireAuthorization(options =>
            {
                options.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
                options.RequireAuthenticatedUser();
            });
        }
    }

    internal class DocTypeOverview
    {
        public string DocTypeName { get; set; } = string.Empty;
        public int Count { get; set; } = 0;
        public string ListUrl { get; set; } = string.Empty;
        public string ApiUrl { get; set; } = string.Empty;
    }

    internal class DocsOverviewModel
    {
        public List<DocTypeOverview> DocTypes { get; set; } = new();
    }
}