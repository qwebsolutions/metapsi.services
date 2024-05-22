using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Shoelace;
using Metapsi.Syntax;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;

namespace Metapsi
{
    public static class Register
    {
        public static Request<T> InitDocument<T>() => new Request<T>(nameof(InitDocument));
        public static Request<List<T>> ListDocuments<T>() => new Request<List<T>>(nameof(ListDocuments));

        internal static List<System.Type> DocTypes { get; set; } = new();

        public static async Task<string> RegisterDocsUi<T>(
            this ApplicationSetup applicationSetup,
            ImplementationGroup ig,
            IEndpointRouteBuilder uiEndpoint,
            string dbPath,
            System.Linq.Expressions.Expression<Func<T, string>> idProperty,
            Func<CommandContext, Task<T>> createDocument,
            Func<CommandContext, Task<List<T>>> listDocuments = null)
        {
            if (listDocuments == null) listDocuments = async (cc) => await cc.Do(ServiceDoc.GetDocApi<T>().List);

            DocTypes.Add(typeof(T));

            var typeEndpoint = uiEndpoint.MapGroup(typeof(T).Name);
            typeEndpoint.RegisterDocUiHandlers<T>();

            var apiEndpoint = typeEndpoint.MapGroup("api");
            apiEndpoint.RegisterFrontendRestApi<T>();

            await applicationSetup.RegisterDocBackendApi<T>(ig, dbPath, idProperty);

            uiEndpoint.Render<ServiceDoc.ListDocsPage<T>>((b, model) => Metapsi.ServiceDoc.Render(b, model, idProperty));

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

            return typeof(T).Name;
        }

        public static string RegisterDocsOverview(
            this IEndpointRouteBuilder uiEndpoint)
        {
            uiEndpoint.Render<DocsOverviewModel>(ServiceDoc.Render);
            uiEndpoint.RegisterGetHandler<OverviewHandler, Docs.Overview>();

            return WebServer.Url<Docs.Overview>();
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

    public class Docs
    {
        public class Overview : Metapsi.Route.IGet
        {

        }
    }

    public class OverviewHandler : Http.Get<Docs.Overview>
    {
        public override async Task<IResult> OnGet(CommandContext commandContext, HttpContext httpContext)
        {
            var docsOverviewModel = new DocsOverviewModel();
            foreach (var type in Register.DocTypes)
            {
                docsOverviewModel.DocServices.Add(new DocsService()
                {
                    DocTypeName = type.Name,
                    ListUrl = "/" + type.Name + "/list"
                });
            }

            return Page.Result(docsOverviewModel);
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