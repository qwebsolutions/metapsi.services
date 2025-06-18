using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Metapsi.WhatsApp;

public static partial class WhatsAppService
{
    public static string GetChallengeQuery(this HttpRequest request)
    {
        request.Query.TryGetValue("hub.challenge", out var challenge);
        return challenge.First();
    }

    public static void SetContentTypeHeader(this HttpResponse response, string contentType)
    {
        response.Headers.ContentType = contentType;
    }

    public static void UseWhatsApp(
        this IEndpointRouteBuilder endpoint,
        WhatsAppCloudApiClient cloudApiClient,
        string whatsAppSecret,
        Func<NotificationPayloadObject, Task> onMessage,
        Func<HttpContext, System.Exception, Task> onException = null)
    {
        Func<Web.CfHttpContext, Exception, Task> wrappedExceptionHandler =
            onException == null ? DefaultExceptionHandler :
            (httpContext, exception) => onException(httpContext.Context, exception);

        string rootEndpointName = Guid.NewGuid().ToString();

        endpoint.MapGet(WebhookPath, (HttpContext httpContext) => HandleGetChallenge(new Web.CfHttpContext(httpContext), cloudApiClient, wrappedExceptionHandler));
        endpoint.MapPost(WebhookPath, (HttpContext httpContext) => HandlePostToWebhook(new Web.CfHttpContext(httpContext), whatsAppSecret, onMessage, wrappedExceptionHandler));
        endpoint.MapPost(PostMessagePath, (HttpContext httpContext) => HandlePostMessage(new Web.CfHttpContext(httpContext), cloudApiClient, wrappedExceptionHandler));
        endpoint.MapPost(UploadMediaPath, (HttpContext httpContext) => HandlePostMedia(new Web.CfHttpContext(httpContext), cloudApiClient, wrappedExceptionHandler));
        endpoint.MapGet(GetMediaPath + "/{mediaId}", (HttpContext httpContext, string mediaId) => HandleGetMedia(new Web.CfHttpContext(httpContext), mediaId, cloudApiClient, wrappedExceptionHandler));
        endpoint.MapGet(
            "/",
            (HttpContext httpContext) => HandleGetApiOverview(
                new Web.CfHttpContext(httpContext),
                (RouteDescription routeDescription) =>
                {
                    LinkGenerator linkGenerator = httpContext.RequestServices.GetRequiredService<LinkGenerator>();
                    var rootPath = linkGenerator.GetPathByName(rootEndpointName);
                    return rootPath.TrimEnd('/') + "/" + routeDescription.Name;
                })).WithName(rootEndpointName);
    }
}