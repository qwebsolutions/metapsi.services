using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Shoelace;
using Metapsi.Syntax;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;

namespace Metapsi
{
    public static partial class ServiceDoc
    {
        public class Config
        {
            internal List<Task> registerDocs = new();
            internal List<Func<CommandContext, Task<DocsService>>> getOverview = new();

            internal IEndpointRouteBuilder uiEndpoint { get; set; }
            internal ApplicationSetup applicationSetup { get; set; }
            internal ImplementationGroup ig { get; set; }
            internal string dbPath { get; set; }

            public void AddDoc<T>(Expression<Func<T, string>> idProperty, Func<CommandContext, Task<T>> createDocument, Func<CommandContext, Task<List<T>>> listDocuments = null)
            {
                if (listDocuments == null) listDocuments = async (cc) => await cc.Do(ServiceDoc.GetDocApi<T>().List);

                registerDocs.Add(uiEndpoint.UseDocs<T>(applicationSetup, ig, dbPath, idProperty, createDocument, listDocuments));
                getOverview.Add(async (CommandContext commandContext) =>
                {
                    var count = await listDocuments(commandContext);
                    return new DocsService()
                    {
                        DocTypeName = typeof(T).Name,
                        Count = (await listDocuments(commandContext)).Count,
                        ListUrl = typeof(T).Name + "/list"
                    };
                });
            }
        }

        public class Props<T>
        {
            public System.Linq.Expressions.Expression<Func<T, string>> IdProperty { get; set; }
            public Func<CommandContext, Task<T>> Create { get; set; }
            public Func<CommandContext, Task<List<T>>> List { get; set; }
        }

        public static Request<T> InitDocument<T>() => new Request<T>(nameof(InitDocument));
        public static Request<List<T>> ListDocuments<T>() => new Request<List<T>>(nameof(ListDocuments));

        public static async Task<RouteHandlerBuilder> UseDocsGroup(
            this IEndpointRouteBuilder uiEndpoint,
            ApplicationSetup applicationSetup,
            ImplementationGroup ig,
            string dbPath,
            Action<Config> setProps)
        {
            var propsConfigurator = new Config()
            {
                applicationSetup = applicationSetup,
                dbPath = dbPath,
                ig = ig,
                uiEndpoint = uiEndpoint
            };
            setProps(propsConfigurator);

            await Task.WhenAll(propsConfigurator.registerDocs);

            var groupName = Guid.NewGuid();

            uiEndpoint.Render<DocsOverviewModel>(ServiceDoc.Render);
            var docsRoute = uiEndpoint.MapGet("/docs", async (CommandContext commandContext, HttpContext httpContext) =>
            {
                var docsOverviewModel = new DocsOverviewModel();
                foreach (var getOverview in propsConfigurator.getOverview)
                {
                    docsOverviewModel.DocServices.Add(await getOverview(commandContext));
                }

                return Page.Result(docsOverviewModel);
            });
            return docsRoute;
        }

        private static async Task FillTypeEndpoint<T>(
            this IEndpointRouteBuilder typeEndpoint,
            ApplicationSetup applicationSetup,
            ImplementationGroup ig,
            string dbPath,
            System.Linq.Expressions.Expression<Func<T, string>> idProperty,
            Func<CommandContext, Task<T>> createDocument,
            Func<CommandContext, Task<List<T>>> listDocuments = null)
        {
            if (listDocuments == null) listDocuments = async (cc) => await cc.Do(ServiceDoc.GetDocApi<T>().List);

            typeEndpoint.RegisterDocUiHandlers<T>();

            var apiEndpoint = typeEndpoint.MapGroup("api");
            apiEndpoint.RegisterFrontendRestApi<T>();

            await applicationSetup.RegisterDocBackendApi<T>(ig, dbPath, idProperty);

            typeEndpoint.Render<ServiceDoc.ListDocsPage<T>>((b, model) => Metapsi.ServiceDoc.Render(b, model, idProperty));

            var noActualState = applicationSetup.AddBusinessState(new object());

            apiEndpoint.MapGet(
                InitDocument<T>().Name,
                async (CommandContext commandContext, HttpContext httpContext) =>
                {
                    return await createDocument(commandContext);
                }).AllowAnonymous();

            apiEndpoint.MapGet(
                ListDocuments<T>().Name,
                async (CommandContext commandContext, HttpContext httpContext) =>
                {
                    return await listDocuments(commandContext);
                }).AllowAnonymous();
        }

        public static async Task<string> UseDocs<T>(
            this IEndpointRouteBuilder uiEndpoint,
            ApplicationSetup applicationSetup,
            ImplementationGroup ig,
            string dbPath,
            System.Linq.Expressions.Expression<Func<T, string>> idProperty,
            Func<CommandContext, Task<T>> createDocument,
            Func<CommandContext, Task<List<T>>> listDocuments = null)
        {
            var typeName = typeof(T).Name;
            var typeEndpoint = uiEndpoint.MapGroup(typeName);
            await FillTypeEndpoint(typeEndpoint, applicationSetup, ig, dbPath, idProperty, createDocument, listDocuments);
            return typeName;
        }

        public static void Render<TModel>(this IEndpointRouteBuilder uiEndpoint, System.Action<HtmlBuilder, TModel> buildPage)
        {
            uiEndpoint.UseRenderer<TModel>(model =>
            {
                var document = HtmlBuilder.FromDefault(b => buildPage(b, model));
                return document.ToHtml();
            });
        }
    }

    public class DocsService
    {
        public string DocTypeName { get; set; } = string.Empty;
        public int Count { get; set; } = 0;
        public string ListUrl { get; set; } = string.Empty;
    }

    public class DocsOverviewModel
    {
        public List<DocsService> DocServices { get; set; } = new();
    }
}