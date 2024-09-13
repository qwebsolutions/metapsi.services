using Dapper;
using Metapsi.Html;
using Metapsi.Sqlite;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Metapsi
{
    public static partial class ServiceDoc
    {
        public class DocsGroup
        {
            // The tasks handle the DocumentProps inside

            internal List<Func<Task>> registerDocs = new();
            internal List<Func<HttpContext, Task<DocTypeOverview>>> getOverview = new();
            internal IEndpointRouteBuilder uiEndpoint { get; set; }
            internal Metapsi.Sqlite.SqliteQueue sqliteQueue { get; set; }
            internal string overviewUrl { get; set; } = "docs";
            internal bool SetJournalWal = true;
            public Action<RouteHandlerBuilder> GroupRoutesBuilder { get; set; }
        }

        public interface IDocumentProps<T>
        {

        }

        internal class DocumentProps<T> : IDocumentProps<T>
        {
            internal IndexColumn IdColumn { get; set; }
            internal List<IndexColumn> IndexColumns { get; } = new();
            internal List<string> FrontendDefaultColumns { get; set; } = DataTable.GetColumns<T>();
            internal Action<RouteHandlerBuilder> DocumentRoutesBuilder { get; set; }
            internal Func<Task<T>> Create { get; set; }
            internal Func<Task<List<T>>> List { get; set; }
            internal Func<T, Task<SaveResult>> Save { get; set; }
            internal Func<T, Task<DeleteResult>> Delete { get; set; }
        }

        //internal class DocumentProps<T, TId> : DocumentProps<T>
        //{
        //    public DocumentProps(Expression<Func<T, TId>> idProperty) : base(idProperty.PropertyName())
        //    {
        //        this.IdProperty = idProperty;
        //    }

        //    public System.Linq.Expressions.Expression<Func<T, TId>> IdProperty { get; set; }
        //}

        public static void AddDoc<T, TId>(
            this DocsGroup docsGroup,
            Expression<Func<T, TId>> idProperty,
            Action<IDocumentProps<T>> setProps = null)
            where T : new()
        {
            docsGroup.AddDoc(
                idProperty,
                async () => new T(),
                setProps);
        }

        public static void AddDoc<T, TId>(
            this DocsGroup docsGroup,
            Expression<Func<T, TId>> idProperty,
            Func<Task<T>> createDocument,
            Action<IDocumentProps<T>> setProps = null)
        {
            DocumentProps<T> docProps = new DocumentProps<T>()
            {
                IdColumn = new IndexColumn()
                {
                    CSharpType = typeof(TId),
                    SourceProperty = idProperty.PropertyName()
                }
            };
            docProps.Create = createDocument;
            if (setProps != null)
            {
                setProps(docProps);
            }
            if (docProps.List == null) docProps.List = async () => await docsGroup.sqliteQueue.Enqueue(async (c) => await c.ListDocuments<T>());
            if (docProps.Save == null) docProps.Save = async (entity) =>
            {
                await docsGroup.sqliteQueue.WithCommit(async t => await t.SaveDocument(entity));
                return new SaveResult()
                {
                    Message = "Document saved",
                    Success = true
                };
            };
            if (docProps.Delete == null) docProps.Delete = async (entity) =>
            {
                await docsGroup.sqliteQueue.WithCommit(async t => await t.DeleteDocument<T, TId>(idProperty.Compile()(entity)));
                return new DeleteResult()
                {
                    Message = "Document deleted",
                    Success = true
                };
            };

            var typeName = typeof(T).Name;
            var typeEndpoint = docsGroup.uiEndpoint.MapGroup(typeName);

            docsGroup.registerDocs.Add(async () =>
            {
                await CreateDocumentTableAsync<T>(
                    docsGroup.sqliteQueue, 
                    GetTableName(typeof(T)),
                    docProps.IdColumn,
                    docProps.IndexColumns);

                await FillTypeEndpoint<T,TId>(
                    typeEndpoint,
                    docsGroup,
                    docProps,
                    idProperty);
            });
            docsGroup.getOverview.Add(async (HttpContext httpContext) =>
            {
                var linkGenerator = httpContext.RequestServices.GetRequiredService<LinkGenerator>();
                var count = await docsGroup.sqliteQueue.Enqueue(async (c) =>
                {
                    return await c.ExecuteScalarAsync<int>($"select count(1) from {GetTableName(typeof(T))}");
                });
                return new DocTypeOverview()
                {
                    DocTypeName = typeof(T).Name,
                    Count = count,
                    DocumentTypeUrl = linkGenerator.GetPathByName(httpContext, Frontend.DocumentPageEndpointName<T>()),
                    ApiUrl = linkGenerator.GetPathByName(httpContext, Frontend.DocumentRestApiBaseEndpointName<T>())
                };
            });
        }

        public static void AddDoc<T>(
            this DocsGroup docsGroup,
            Expression<Func<T, string>> idProperty,
            Action<IDocumentProps<T>> setProps = null)
            where T : new()
        {
            docsGroup.AddDoc<T, string>(idProperty, setProps);
        }

        public static void AddDoc<T>(
            this DocsGroup docsGroup,
            Expression<Func<T, string>> idProperty,
            Func<Task<T>> createDocument,
            Action<IDocumentProps<T>> setProps = null)
        {
            docsGroup.AddDoc<T, string>(idProperty, createDocument, setProps);
        }

        public static void AddDoc<T>(
            this DocsGroup docsGroup,
            Expression<Func<T, int>> idProperty,
            Action<IDocumentProps<T>> setProps = null)
            where T : new()
        {
            docsGroup.AddDoc<T, int>(idProperty, setProps);
        }

        public static void AddDoc<T>(
            this DocsGroup docsGroup,
            Expression<Func<T, int>> idProperty,
            Func<Task<T>> createDocument,
            Action<IDocumentProps<T>> setProps = null)
        {
            docsGroup.AddDoc<T, int>(idProperty, createDocument, setProps);
        }

        public static void AddDoc<T>(
            this DocsGroup docsGroup,
            Expression<Func<T, Guid>> idProperty,
            Action<IDocumentProps<T>> setProps = null)
            where T : new()
        {
            docsGroup.AddDoc<T, Guid>(idProperty, setProps);
        }

        public static void AddDoc<T>(
            this DocsGroup docsGroup,
            Expression<Func<T, Guid>> idProperty,
            Func<Task<T>> createDocument,
            Action<IDocumentProps<T>> setProps = null)
        {
            docsGroup.AddDoc<T, Guid>(idProperty, createDocument, setProps);
        }

        public static void SetOverviewUrl(this DocsGroup docsGroup, string path)
        {
            docsGroup.overviewUrl = "/" + path.Trim('/');
        }

        /// <summary>
        /// Keeps existing SQLITE journal mode, otherwise set to WAL
        /// </summary>
        /// <param name="docsGroup"></param>
        public static void PreserveExistingJournalMode(this DocsGroup docsGroup)
        {
            docsGroup.SetJournalWal = false;
        }

        public static async Task<RouteHandlerBuilder> UseDocs(
            this IEndpointRouteBuilder uiEndpoint,
            Sqlite.SqliteQueue sqliteQueue,
            Action<DocsGroup> setProps)
        {
            var propsConfigurator = new DocsGroup()
            {
                sqliteQueue = sqliteQueue,
                uiEndpoint = uiEndpoint
            };
            setProps(propsConfigurator);
            if (propsConfigurator.SetJournalWal)
            {
                await sqliteQueue.SetJournalModeWal();
            }

            // Execute sequentially because they share the same db
            foreach (var registerDoc in propsConfigurator.registerDocs)
            {
                await registerDoc();
            }

            uiEndpoint.Render<DocsOverviewModel>(ServiceDoc.Render);
            var docsRoute = uiEndpoint.MapGet(propsConfigurator.overviewUrl, async (HttpContext httpContext) =>
            {
                var docsOverviewModel = new DocsOverviewModel();
                foreach (var getOverview in propsConfigurator.getOverview)
                {
                    docsOverviewModel.DocTypes.Add(await getOverview(httpContext));
                }

                return Page.Result(docsOverviewModel);
            });

            if (propsConfigurator.GroupRoutesBuilder != null)
            {
                propsConfigurator.GroupRoutesBuilder(docsRoute);
            }

            return docsRoute;
        }

        private static async Task FillTypeEndpoint<T, TId>(
            this IEndpointRouteBuilder typeEndpoint,
            DocsGroup docsGroup,
            DocumentProps<T> documentProps,
            System.Linq.Expressions.Expression<Func<T,TId>> idProperty)
        {
            typeEndpoint.MapListDocsFrontendApi<T,TId>(docsGroup, documentProps);

            var apiEndpoint = typeEndpoint.MapGroup("api");
            apiEndpoint.MapRestApi<T, TId>(docsGroup.sqliteQueue);

            typeEndpoint.Render<ServiceDoc.ListDocsPage<T>>((b, model) => Metapsi.ServiceDoc.Render<T, TId>(b, model, idProperty));
        }

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
}