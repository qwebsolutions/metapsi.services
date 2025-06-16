using Metapsi.Html;
using Metapsi.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Metapsi
{
    public static partial class ServiceDoc
    {
        public interface IDocsApp
        {

        }

        private static HtmlDocument ErrorPage(string error)
        {
            return HtmlBuilder.FromDefault(
                b =>
                {
                    b.BodyAppend(b.Text(error));
                });
        }

        public class DocsApp : IDocsApp
        {
            internal Dictionary<string, DocHandler> docHandlers { get; set; } = new Dictionary<string, DocHandler>();
            //internal Func<Func<string, string>, Task<HtmlDocument>> GetRootPage { get; set; }
            internal string EndpointName { get; set; } = Guid.NewGuid().ToString();
            internal DocsGroup DocsGroup { get; set; }

            internal DocsApp()
            {
                this.DocsGroup = new DocsGroup(this);
            }

            public async Task<HtmlDocument> GetRootPage(Func<RouteDescription, string> findUrl)
            {
                var docsOverviewModel = new DocsOverviewModel();
                foreach (var docTypeHandler in docHandlers)
                {
                    docsOverviewModel.DocTypes.Add(await docTypeHandler.Value.GetDocTypeSummary(findUrl));
                }

                var overviewHtmlDocument = HtmlBuilder.FromDefault(b =>
                {
                    ServiceDoc.Render(b, docsOverviewModel);
                });

                return overviewHtmlDocument;
            }

            public async Task<HtmlDocument> GetListDocumentsPage(string docType, Func<RouteDescription, string> findUrl)
            {
                if (docHandlers.TryGetValue(docType, out var handler))
                {
                    var listDocumentsPage = await handler.GetListDocumentsPage(findUrl);
                    return listDocumentsPage;
                }
                return ErrorPage($"Type {docType} is not valid");
            }

            public async Task WriteInitApiResponse(string docType, Metapsi.Web.CfHttpResponse httpResponse)
            {
                if (docHandlers.TryGetValue(docType, out var handler))
                {
                    await handler.WriteInitApiResponse(httpResponse);
                }
            }

            public async Task WriteListDocumentsApiResponse(string docType, Metapsi.Web.CfHttpResponse httpResponse)
            {
                if (docHandlers.TryGetValue(docType, out var handler))
                {
                    await handler.WriteListDocumentsApiResponse(httpResponse);
                }
            }

            public async Task HandleSaveDocumentApi(string docType, CfHttpContext httpContext)
            {
                if (docHandlers.TryGetValue(docType, out var handler))
                {
                    await handler.HandleSaveDocumentApi(httpContext);
                }
            }

            public async Task HandleDeleteDocumentApi(string docType, CfHttpContext httpContext)
            {
                if (docHandlers.TryGetValue(docType, out var handler))
                {
                    await handler.HandleDeleteDocumentApi(httpContext);
                }
            }
        }

        internal class DocHandler
        {
            internal Func<Task> Migrate { get; set; }
            internal Func<Func<RouteDescription, string>, Task<DocTypeOverview>> GetDocTypeSummary { get; set; }
            internal Func<Func<RouteDescription, string>, Task<HtmlDocument>> GetListDocumentsPage { get; set; }
            internal Func<Metapsi.Web.CfHttpResponse, Task> WriteInitApiResponse { get; set; }
            internal Func<Metapsi.Web.CfHttpResponse, Task> WriteListDocumentsApiResponse { get; set; }
            internal Func<CfHttpContext, Task> HandleSaveDocumentApi { get; set; }
            internal Func<CfHttpContext, Task> HandleDeleteDocumentApi { get; set; }
        }

        /// <summary>
        /// Configuration of docs
        /// </summary>
        public class DocsGroup
        {
            internal DocsGroup(DocsApp docsApp)
            {
                this.docsApp = docsApp;
            }

            //internal List<Func<DocsApp, Task>> registerDocs = new();
            //internal List<Func<Func<string,string>, Task<DocTypeOverview>>> getOverview = new();
            internal string overviewUrl { get; set; } = "/";

            internal IDbDefaultActions defaultInitializer;
            internal readonly DocsApp docsApp;
        }


        public interface IDocumentProps<T>
        {
        }

        internal abstract class DocumentProps<T> : IDocumentProps<T>
        {
            internal IndexColumn IdColumn { get; set; }
            internal List<IndexColumn> IndexColumns { get; } = new();
            internal List<string> FrontendDefaultColumns { get; set; } = DataTable.GetColumns<T>();
            internal Func<Task> Migrate { get; set; }
            internal Func<Task<int>> Count { get; set; }
            internal Func<Task<T>> Create { get; set; }
            internal Func<Task<List<T>>> List { get; set; }
            internal Func<T, Task<SaveResult>> Save { get; set; }
            internal Func<T, Task<DeleteResult>> Delete { get; set; }
        }

        internal class DocumentProps<T,TId> : DocumentProps<T>
        {
            internal DocumentProps(Expression<Func<T, TId>> getId)
            {
                this.getId = getId;
            }

            internal System.Linq.Expressions.Expression<Func<T,TId>> getId { get; set; }

            public void FillDefaults(IDbDefaultActions defaultActions)
            {
                if (this.Migrate == null)
                {
                    this.Migrate = defaultActions.GetDefaultMigrate(new TableMetadata()
                    {
                        IdColumn = this.IdColumn,
                        IndexColumns = this.IndexColumns,
                        TableName = GetTableName(typeof(T))
                    });
                }

                if (this.Count == null)
                {
                    this.Count = defaultActions.GetDefaultCount<T>();
                }

                if(this.List == null)
                {
                    this.List = defaultActions.GetDefaultList<T>();
                }

                if(this.Save == null)
                {
                    this.Save= defaultActions.GetDefaultSave<T>();
                }

                if(this.Delete == null)
                {
                    this.Delete = defaultActions.GetDefaultDelete<T, TId>(this.getId);
                }
            }
        }

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
            var docProps = new DocumentProps<T,TId>(idProperty)
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

            var docType = DocumentTypeIdentifier<T>();

            docProps.FillDefaults(docsGroup.defaultInitializer);
            docsGroup.docsApp.docHandlers.Add(
                docType,
                new DocHandler()
                {
                    Migrate = docProps.Migrate,
                    GetDocTypeSummary = async (findApi) =>
                    {
                        var count = await docProps.Count();
                        var overview = new DocTypeOverview()
                        {
                            DocTypeName = typeof(T).Name,
                            Count = count,
                            DocumentTypeUrl = findApi(RouteDescription.New(
                                "list-documents-page",
                                b =>
                                {
                                    b.Add("docType", DocumentTypeIdentifier<T>());
                                }))
                        };

                        return overview;
                    },
                    GetListDocumentsPage = async (findApi) =>
                    {
                        var list = await docProps.List();

                        var descriptionAttributes = typeof(T).CustomAttributes.Where(x => x.AttributeType == typeof(DocDescriptionAttribute));

                        var withoutOrder = descriptionAttributes.Where(x => x.ConstructorArguments.Count == 1);
                        var withOrder = descriptionAttributes.Where(x => x.ConstructorArguments.Count == 2).OrderBy(x => (Int32)x.ConstructorArguments[1].Value);

                        var orderedAttributes = new List<System.Reflection.CustomAttributeData>();
                        orderedAttributes.AddRange(withOrder);
                        orderedAttributes.AddRange(withoutOrder);

                        StringBuilder descriptionBuilder = new StringBuilder();

                        foreach (var descriptionAttribute in orderedAttributes)
                        {
                            var constructor = descriptionAttribute.ConstructorArguments.Where(x => x.ArgumentType == typeof(string)).FirstOrDefault();
                            descriptionBuilder.AppendLine(constructor.Value.ToString());
                        }

                        var summaryHtml = descriptionBuilder.ToString();

                        var model = new ListDocsPage<T>()
                        {
                            DocumentSchema = JsonSchemaExtensions.GetJsonSchemaType(typeof(T)),
                            InitApiUrl = findApi(RouteDescription.New("init-api").Add("docType", docType)),
                            ListApiUrl = findApi(RouteDescription.New("list-api").Add("docType",docType)),
                            SaveApiUrl = findApi(RouteDescription.New("save-api").Add("docType", docType)),
                            DeleteApiUrl = findApi(RouteDescription.New("delete-api").Add("docType", docType)),
                            Documents = list,
                            SummaryHtml = summaryHtml,
                            Columns = docProps.FrontendDefaultColumns
                        };
                        var htmlDocument = HtmlBuilder.FromDefault(
                            b =>
                            {
                                Render(b, model, idProperty);
                            });

                        return htmlDocument;
                    },
                    WriteInitApiResponse = async (httpResponse) =>
                    {
                        var newObject = await docProps.Create();
                        await httpResponse.WriteJsonReponse(newObject);
                    },
                    WriteListDocumentsApiResponse = async (httpResponse) =>
                    {
                        var newList = await docProps.List();
                        await httpResponse.WriteJsonReponse(newList);
                    },
                    HandleSaveDocumentApi = async (httpContext) =>
                    {
                        var document = await httpContext.Request.ReadJsonBody<T>();
                        var saveResponse = await docProps.Save(document);
                        await httpContext.Response.WriteJsonReponse(saveResponse);
                    },
                    HandleDeleteDocumentApi = async (httpContext) =>
                    {
                        var document = await httpContext.Request.ReadJsonBody<T>();
                        var deleteResponse = await docProps.Delete(document);
                        await httpContext.Response.WriteJsonReponse(deleteResponse);
                    },
                });

            //if (docProps.List == null) docProps.List = docsGroup.dbQueue.ListDocuments<T>;
            //if (docProps.Save == null) docProps.Save = async (entity) =>
            //{
            //    await docsGroup.dbQueue.SaveDocument(entity);
            //    return new SaveResult()
            //    {
            //        Message = "Document saved",
            //        Success = true
            //    };
            //};
            //if (docProps.Delete == null) docProps.Delete = async (entity) =>
            //{
            //    await docsGroup.dbQueue.DeleteDocument<T, TId>(idProperty.Compile()(entity));
            //    return new DeleteResult()
            //    {
            //        Message = "Document deleted",
            //        Success = true
            //    };
            //};

            var typeName = typeof(T).Name;

            //docsGroup.registerDocs.Add(async (DocsApp docsApp) =>
            //{
            //    await docProps.Migrate();
            //    docsApp.RenderDocumentsList.Add(
            //        typeof(T),
            //        async (Func<string,string> findApi) =>
            //        {
            //            var list = await docProps.List();

            //            var descriptionAttributes = typeof(T).CustomAttributes.Where(x => x.AttributeType == typeof(DocDescriptionAttribute));

            //            var withoutOrder = descriptionAttributes.Where(x => x.ConstructorArguments.Count == 1);
            //            var withOrder = descriptionAttributes.Where(x => x.ConstructorArguments.Count == 2).OrderBy(x => (Int32)x.ConstructorArguments[1].Value);

            //            var orderedAttributes = new List<System.Reflection.CustomAttributeData>();
            //            orderedAttributes.AddRange(withOrder);
            //            orderedAttributes.AddRange(withoutOrder);

            //            StringBuilder descriptionBuilder = new StringBuilder();

            //            foreach (var descriptionAttribute in orderedAttributes)
            //            {
            //                var constructor = descriptionAttribute.ConstructorArguments.Where(x => x.ArgumentType == typeof(string)).FirstOrDefault();
            //                descriptionBuilder.AppendLine(constructor.Value.ToString());
            //            }

            //            var summaryHtml = descriptionBuilder.ToString();

            //            var model = new ListDocsPage<T>()
            //            {
            //                DocumentSchema = JsonSchemaExtensions.GetJsonSchemaType(typeof(T)),
            //                InitApiUrl = findApi(Frontend.InitApiEndpointName<T>()),
            //                ListApiUrl = findApi(Frontend.ListApiEndpointName<T>()),
            //                SaveApiUrl = findApi(Frontend.SaveApiEndpointName<T>()),
            //                DeleteApiUrl = findApi(Frontend.DeleteApiEndpointName<T>()),
            //                Documents = list,
            //                SummaryHtml = summaryHtml,
            //                Columns = docProps.FrontendDefaultColumns
            //            };
            //            var htmlDocument = HtmlBuilder.FromDefault(
            //                b =>
            //                {
            //                    Render(b, model, idProperty);
            //                });
            //            return htmlDocument;
            //        });
            //});
            //docsGroup.getOverview.Add(async (Func<string, string> findApi) =>
            //{
            //    //var count = await docsGroup.dbQueue.SqliteQueue.Enqueue(async (c) =>
            //    //{
            //    //    return await c.ExecuteScalarAsync<int>($"select count(1) from {GetTableName(typeof(T))}");
            //    //});
            //    var count = await docProps.Count();
            //    return new DocTypeOverview()
            //    {
            //        DocTypeName = typeof(T).Name,
            //        Count = count,
            //        DocumentTypeUrl = findApi(Frontend.DocumentPageEndpointName<T>()),
            //        ApiUrl = findApi(Frontend.DocumentRestApiBaseEndpointName<T>())
            //    };
            //});
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

        ///// <summary>
        ///// Keeps existing SQLITE journal mode, otherwise set to WAL
        ///// </summary>
        ///// <param name="docsGroup"></param>
        //public static void PreserveExistingJournalMode(this DocsGroup docsGroup)
        //{
        //    docsGroup.SetJournalWal = false;
        //}

        public static async Task<DocsApp> CreateDocsApp(
            Action<DocsGroup> configure)
        {
            DocsApp docsApp = new DocsApp();
            configure(docsApp.DocsGroup);
            foreach (var docType in docsApp.docHandlers)
            {
                await docType.Value.Migrate();
            }

            await EmbeddedFiles.AddAssembly(typeof(Metapsi.ServiceDoc).Assembly);

            //docsApp.GetRootPage = async (findApi) =>
            //{
            //    var docsOverviewModel = new DocsOverviewModel();
            //    foreach (var docTypeHandler in docsApp.docHandlers)
            //    {
            //        docsOverviewModel.DocTypes.Add(await docTypeHandler.Value.GetDocTypeSummary(findApi));
            //    }

            //    var overviewHtmlDocument = HtmlBuilder.FromDefault(b =>
            //    {
            //        ServiceDoc.Render(b, docsOverviewModel);
            //    });

            //    return overviewHtmlDocument;
            //};

            return docsApp;
        }


        //private static async Task FillTypeEndpoint<T, TId>(
        //    this IEndpointRouteBuilder typeEndpoint,
        //    DocsGroup docsGroup,
        //    DocumentProps<T> documentProps,
        //    System.Linq.Expressions.Expression<Func<T,TId>> idProperty)
        //{
        //    typeEndpoint.MapListDocsFrontendApi<T,TId>(docsGroup, documentProps);

        //    var apiEndpoint = typeEndpoint.MapGroup("api");
        //    apiEndpoint.MapRestApi<T, TId>(docsGroup.dbQueue);

        //    typeEndpoint.Render<ServiceDoc.ListDocsPage<T>>((b, model) => Metapsi.ServiceDoc.Render<T, TId>(b, model, idProperty));
        //}

        //public static void Render<TModel>(this IEndpointRouteBuilder uiEndpoint, System.Action<HtmlBuilder, TModel> buildPage)
        //{
        //    uiEndpoint.UseRenderer<TModel>(model =>
        //    {
        //        return HtmlBuilder.FromDefault(b => buildPage(b, model));
        //    });
        //}

        //private static void ConfigureRoute<T>(RouteHandlerBuilder route, DocsGroup docsGroup, DocumentProps<T> documentProps)
        //{
        //    if (documentProps.DocumentRoutesBuilder == null)
        //    {
        //        if (docsGroup.GroupRoutesBuilder == null)
        //        {
        //            route.AllowAnonymous();
        //        }
        //        else
        //        {
        //            docsGroup.GroupRoutesBuilder(route);
        //        }
        //    }
        //    else
        //    {
        //        documentProps.DocumentRoutesBuilder(route);
        //    }
        //}

        //private static void ConfigureApiRoute(RouteHandlerBuilder route)
        //{
        //    //route.RequireAuthorization(options =>
        //    //{
        //    //    options.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
        //    //    options.RequireAuthenticatedUser();
        //    //});
        //}
    }
}