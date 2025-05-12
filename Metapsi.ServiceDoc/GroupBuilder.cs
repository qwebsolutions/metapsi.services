using Dapper;
using Metapsi.Html;
using Metapsi.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace Metapsi
{
    public static partial class ServiceDoc
    {
        public interface IDocsApp
        {

        }

        //public static async Task<HtmlDocument> RenderOverview(
        //    this IDocsApp docsApp,
        //    Func<string, string> findApi)
        //{
        //    var internalDocsApp = docsApp as DocsApp;
        //    return await internalDocsApp.RenderOverview(findApi);
        //}

        //public static async Task<HtmlDocument> RenderList(this IDocsApp docsApp, string documentType, Func<string, string> findApi)
        //{
        //    var app = docsApp as DocsApp;
        //    if (app != null)
        //    {
        //        if (app.docHandlers.TryGetValue(documentType, out var handler))
        //        {
        //            var render = handler.RenderDocumentsList;
        //            if (render != null)
        //            {
        //                return await render(findApi);
        //            }
        //        }
        //    }

        //    return HtmlBuilder.FromDefault(
        //        b =>
        //        {
        //            return b.HtmlDiv(
        //                b =>
        //                {

        //                },
        //                b.Text($"Type {documentType} not available"));
        //        });
        //}

        //public static async Task WriteOverviewHtmlResponse(
        //    this IDocsApp docsApp,
        //    Func<Stream, string, Task> writeResponse,
        //    Func<string, string> findApi)
        //{
        //    var internalDocsApp = docsApp as DocsApp;
        //    var overviewHtmlDocument = await internalDocsApp.RenderOverview(findApi);

        //    await writeResponse(
        //        new MemoryStream(Encoding.UTF8.GetBytes(overviewHtmlDocument.ToHtml())),
        //        "text/html");
        //}

        //public static async Task WriteListDocumentsHtmlResponse(
        //    this IDocsApp docsApp,
        //    string documentType,
        //    Func<Stream, string, Task> writeResponse,
        //    Func<string, string> findApi)
        //{
        //    var app = docsApp as DocsApp;
        //    if (app != null)
        //    {
        //        if (app.docHandlers.TryGetValue(documentType, out var handler))
        //        {
        //            var render = handler.WriteListDocumentsHtmlResponse(writeResponse);
        //        }
        //    }

        //    var internalDocsApp = docsApp as DocsApp;
        //    var overviewHtmlDocument = await internalDocsApp.RenderOverview(findApi);

        //    await writeResponse(
        //        new MemoryStream(Encoding.UTF8.GetBytes(overviewHtmlDocument.ToHtml())),
        //        "text/html");
        //}

        //public static async Task WriteInitDocumentApiResponse(this IDocsApp docsApp, string documentType, Func<Stream, string, Task> writeResponse)
        //{
        //    var app = docsApp as DocsApp;
        //    if (app != null)
        //    {
        //        if (app.docHandlers.TryGetValue(documentType, out var handler))
        //        {
        //            await handler.WriteInitApiResponse(writeResponse);
        //        }
        //    }
        //}

        //public static async Task WriteListDocumentsApiResponse(this IDocsApp docsApp, string documentType, Func<Stream, string, Task> writeResponse)
        //{
        //    var app = docsApp as DocsApp;
        //    if (app != null)
        //    {
        //        if (app.docHandlers.TryGetValue(documentType, out var handler))
        //        {
        //            await handler.WriteListDocumentsApiResponse(writeResponse);
        //        }
        //    }
        //}

        //public static async Task WriteSaveDocumentApiResponse(this IDocsApp docsApp, string documentType, Func<Stream, string, Task> writeResponse)
        //{
        //    var app = docsApp as DocsApp;
        //    if (app != null)
        //    {
        //        if (app.docHandlers.TryGetValue(documentType, out var handler))
        //        {
        //            await handler.WriteSaveDocumentApiResponse(writeResponse);
        //        }
        //    }
        //}

        //public static async Task WriteDeleteDocumentApiResponse(this IDocsApp docsApp, string documentType, Func<Stream, string, Task> writeResponse)
        //{
        //    var app = docsApp as DocsApp;
        //    if (app != null)
        //    {
        //        if (app.docHandlers.TryGetValue(documentType, out var handler))
        //        {
        //            await handler.WriteSaveDocumentApiResponse(writeResponse);
        //        }
        //    }
        //}

        internal class DocsApp : IDocsApp
        {
            internal Dictionary<string, DocHandler> docHandlers { get; set; } = new Dictionary<string, DocHandler>();
            internal Func<Func<Stream,string,Task>, Func<string,string>, Task> WriteOverviewHtmlResponse { get; set; }
            internal string EndpointName { get; set; } = Guid.NewGuid().ToString();
            internal DocsGroup DocsGroup { get; set; }

            internal DocsApp()
            {
                this.DocsGroup = new DocsGroup(this);
            }
        }

        internal class DocHandler
        {
            internal Func<Task> Migrate { get; set; }
            internal Func<Func<string, string>, Task<DocTypeOverview>> GetOverview { get; set; }
            internal Func<Stream, Task<object>> ReadBody { get; set; }
            internal Func<Func<Stream, string, Task>, Func<string, string>, Task> WriteListDocumentsHtmlResponse { get; set; }
            internal Func<Func<Stream, string, Task>, Task> WriteInitApiResponse { get; set; }
            internal Func<Func<Stream, string, Task>, Task> WriteListDocumentsApiResponse { get; set; }
            internal Func<Stream, Func<Stream, string, Task>, Task> WriteSaveDocumentApiResponse { get; set; }
            internal Func<Stream, Func<Stream, string, Task>, Task> WriteDeleteDocumentApiResponse { get; set; }
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
                //this.Migrate = initializer.DefaultMigrate<T>();
                //this.Count = initializer.DefaultCount<T>();
                //this.Create = initializer.DefaultCreate<T>();
                //this.List = initializer.DefaultList<T>();
                //this.Save = initializer.DefaultSave<T>();
                //this.Delete = initializer.DefaultDelete<T>();
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

            var docTypePath = DocumentTypeIdentifier<T>();

            docProps.FillDefaults(docsGroup.defaultInitializer);
            docsGroup.docsApp.docHandlers.Add(
                docTypePath,
                new DocHandler()
                {
                    Migrate = docProps.Migrate,
                    ReadBody = async (Stream httpBody) =>
                    {
                        return await System.Text.Json.JsonSerializer.DeserializeAsync<T>(httpBody);
                    },
                    GetOverview = async (findApi) =>
                    {
                        var count = await docProps.Count();
                        var overview = new DocTypeOverview()
                        {
                            DocTypeName = typeof(T).Name,
                            Count = count,
                            DocumentTypeUrl = findApi(DocumentTypeIdentifier<T>())
                        };

                        return overview;
                    },
                    WriteListDocumentsHtmlResponse = async (writeHtmlResponse, findApi) =>
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
                            InitApiUrl = findApi($"init/{docTypePath}"),
                            ListApiUrl = findApi($"list/{docTypePath}"),
                            SaveApiUrl = findApi($"save/{docTypePath}"),
                            DeleteApiUrl = findApi($"delete/{docTypePath}"),
                            Documents = list,
                            SummaryHtml = summaryHtml,
                            Columns = docProps.FrontendDefaultColumns
                        };
                        var htmlDocument = HtmlBuilder.FromDefault(
                            b =>
                            {
                                Render(b, model, idProperty);
                            });

                        await Metapsi.MetadataExtensions.LoadMetadataResources(htmlDocument.Metadata);

                        await writeHtmlResponse(
                            new MemoryStream(
                                System.Text.Encoding.UTF8.GetBytes(htmlDocument.ToHtml())),
                            "text/html");
                    },
                    WriteInitApiResponse = async (writeJsonResponse) =>
                    {
                        var newObject = await docProps.Create();
                        await writeJsonResponse(
                            new MemoryStream(
                                System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(newObject))),
                            "application/json");
                    },
                    WriteListDocumentsApiResponse = async (writeJsonResponse) =>
                    {
                        var newList = await docProps.List();

                        await writeJsonResponse(
                            new MemoryStream(
                                System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(newList))),
                            "application/json");
                    },
                    WriteSaveDocumentApiResponse = async (httpBody, writeJsonResponse) =>
                    {
                        var document = await System.Text.Json.JsonSerializer.DeserializeAsync<T>(httpBody);
                        var saveResponse = await docProps.Save(document);
                        await writeJsonResponse(
                            new MemoryStream(
                                System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(saveResponse))),
                            "application/json");
                    },
                    WriteDeleteDocumentApiResponse = async (httpBody, writeJsonResponse) =>
                    {
                        var document = await System.Text.Json.JsonSerializer.DeserializeAsync<T>(httpBody);
                        var deleteResponse = await docProps.Delete(document);
                        await writeJsonResponse(
                            new MemoryStream(
                                System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(deleteResponse))),
                            "application/json");
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

        internal static async Task<DocsApp> CreateDocsApp(
            Action<DocsGroup> configure)
        {
            DocsApp docsApp = new DocsApp();
            configure(docsApp.DocsGroup);
            foreach (var docType in docsApp.docHandlers)
            {
                await docType.Value.Migrate();
            }

            await EmbeddedFiles.AddAssembly(typeof(Metapsi.ServiceDoc).Assembly);

            docsApp.WriteOverviewHtmlResponse = async (writeResponse, findApi) =>
            {
                var docsOverviewModel = new DocsOverviewModel();
                foreach (var docTypeHandler in docsApp.docHandlers)
                {
                    docsOverviewModel.DocTypes.Add(await docTypeHandler.Value.GetOverview(findApi));
                }

                var overviewHtmlDocument = HtmlBuilder.FromDefault(b =>
                {
                    ServiceDoc.Render(b, docsOverviewModel);
                });

                await Metapsi.MetadataExtensions.LoadMetadataResources(overviewHtmlDocument.Metadata);

                await writeResponse(
                    new MemoryStream(
                        System.Text.Encoding.UTF8.GetBytes(overviewHtmlDocument.ToHtml())),
                        "text/html");
            };

            //var docsRoute = groupEndpoint.MapGet(docsGroup.overviewUrl, async (HttpContext httpContext) =>
            //{
            //    var docsOverviewModel = new DocsOverviewModel();
            //    foreach (var getOverview in docsGroup.getOverview)
            //    {
            //        docsOverviewModel.DocTypes.Add(await getOverview(httpContext));
            //    }

            //    return Page.Result(docsOverviewModel);
            //});

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