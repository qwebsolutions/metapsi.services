using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Metapsi.ServiceDoc;

namespace Metapsi
{
    public static partial class ServiceDoc
    {
        public class SaveResult
        {
            public bool Success { get; set; }
            public string Message { get; set; }
        }

        public class DeleteResult
        {
            public bool Success { get; set; }
            public string Message { get; set; }
        }

        //internal static void MapListDocsFrontendApi<T, TId>(
        //    this Microsoft.AspNetCore.Routing.IEndpointRouteBuilder endpoint,
        //    DocsGroup docsGroup,
        //    DocumentProps<T> props)
        //{
        //    var pageRoute = endpoint.MapGet("/", async (HttpContext httpContext) =>
        //    {
        //        var list = await props.List();

        //        var descriptionAttributes = typeof(T).CustomAttributes.Where(x => x.AttributeType == typeof(DocDescriptionAttribute));

        //        var withoutOrder = descriptionAttributes.Where(x => x.ConstructorArguments.Count == 1);
        //        var withOrder = descriptionAttributes.Where(x => x.ConstructorArguments.Count == 2).OrderBy(x => (Int32)x.ConstructorArguments[1].Value);

        //        var orderedAttributes = new List<System.Reflection.CustomAttributeData>();
        //        orderedAttributes.AddRange(withOrder);
        //        orderedAttributes.AddRange(withoutOrder);

        //        StringBuilder descriptionBuilder = new StringBuilder();

        //        foreach (var descriptionAttribute in orderedAttributes)
        //        {
        //            var constructor = descriptionAttribute.ConstructorArguments.Where(x => x.ArgumentType == typeof(string)).FirstOrDefault();
        //            descriptionBuilder.AppendLine(constructor.Value.ToString());
        //        }

        //        var summaryHtml = descriptionBuilder.ToString();

        //        // Frontend calls for create / list are relative to the page url itself
        //        var linkGenerator = httpContext.RequestServices.GetRequiredService<LinkGenerator>();

        //        return Page.Result(
        //            new ListDocsPage<T>()
        //            {
        //                DocumentSchema = JsonSchemaExtensions.GetJsonSchemaType(typeof(T)),
        //                InitApiUrl = linkGenerator.GetPathByName(httpContext, Frontend.InitApiEndpointName<T>()),
        //                ListApiUrl = linkGenerator.GetPathByName(httpContext, Frontend.ListApiEndpointName<T>()),
        //                SaveApiUrl = linkGenerator.GetPathByName(httpContext, Frontend.SaveApiEndpointName<T>()),
        //                DeleteApiUrl = linkGenerator.GetPathByName(httpContext, Frontend.DeleteApiEndpointName<T>()),
        //                Documents = list,
        //                SummaryHtml = summaryHtml,
        //                Columns = props.FrontendDefaultColumns
        //            });
        //    });
        //    pageRoute.WithName(Frontend.DocumentPageEndpointName<T>());

        //ConfigureRoute(pageRoute, docsGroup, props);

        //var initRoute = endpoint.MapGet(
        //    Frontend.Url.Init,
        //    async (HttpContext httpContext) =>
        //    {
        //        return await props.Create();
        //    });
        //initRoute.WithName(Frontend.InitApiEndpointName<T>());

        //var listRoute = endpoint.MapGet(
        //    Frontend.Url.List,
        //    async (HttpContext httpContext) =>
        //    {
        //        return await props.List();
        //    });
        //listRoute.WithName(Frontend.ListApiEndpointName<T>());

        //var saveRoute = endpoint.MapPost(
        //    $"{Frontend.Url.Save}",
        //    async (HttpContext httpContext, T entity) =>
        //    {
        //        return Results.Json(await props.Save(entity));
        //    });
        //saveRoute.WithName(Frontend.SaveApiEndpointName<T>());

        //var deleteRoute = endpoint.MapPost(
        //    $"{Frontend.Url.Delete}",
        //    async (HttpContext httpContext, T entity) =>
        //    {
        //        return Results.Json(await props.Delete(entity));
        //    });
        //deleteRoute.WithName(Frontend.DeleteApiEndpointName<T>());

        //if (props.DocumentRoutesBuilder == null)
        //{
        //    if (docsGroup.GroupRoutesBuilder == null)
        //    {
        //        initRoute.AllowAnonymous();
        //        listRoute.AllowAnonymous();
        //        saveRoute.AllowAnonymous();
        //        deleteRoute.AllowAnonymous();
        //    }
        //    else
        //    {
        //        docsGroup.GroupRoutesBuilder(initRoute);
        //        docsGroup.GroupRoutesBuilder(listRoute);
        //        docsGroup.GroupRoutesBuilder(saveRoute);
        //        docsGroup.GroupRoutesBuilder(deleteRoute);
        //    }
        //}
        //else
        //{
        //    props.DocumentRoutesBuilder(initRoute);
        //    props.DocumentRoutesBuilder(listRoute);
        //    props.DocumentRoutesBuilder(saveRoute);
        //    props.DocumentRoutesBuilder(deleteRoute);
        //}
        //}

        //internal static class Frontend
        //{
        //    public static class Url
        //    {
        //        public const string Init = "init";
        //        public const string List = "list";
        //        public const string Save = "save";
        //        public const string Delete = "delete";
        //    }

        public static string DocumentTypeIdentifier<T>()
        {
            return $"document-{typeof(T).CSharpTypeName()}";
        }

        //    //public static string DocumentRestApiBaseEndpointName<T>()
        //    //{
        //    //    return $"rest-api-{typeof(T).CSharpTypeName()}";
        //    //}

        //    public static string InitApiEndpointName<T>()
        //    {
        //        return $"init-{typeof(T).CSharpTypeName()}";
        //    }

        //    public static string ListApiEndpointName<T>()
        //    {
        //        return $"list-{typeof(T).CSharpTypeName()}";
        //    }

        //    public static string SaveApiEndpointName<T>()
        //    {
        //        return $"save-{typeof(T).CSharpTypeName()}";
        //    }

        //    public static string DeleteApiEndpointName<T>()
        //    {
        //        return $"delete-{typeof(T).CSharpTypeName()}";
        //    }
        //}
    }
}