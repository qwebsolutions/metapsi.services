using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Metapsi.Html;
using Metapsi.Web;

namespace Metapsi.WhatsApp;

public class ApiOverview
{
    public string WebhookPath { get; set; } = string.Empty;
    public string PostMessagePath { get; set; } = string.Empty;
    public string UploadMediaPath { get; set; } = string.Empty;
    public string GetMediaPath { get; set; } = string.Empty;
}

public static partial class WhatsAppService
{
    public const string WebhookPath = "webhook";
    public const string PostMessagePath = "postmessage";
    public const string UploadMediaPath = "uploadmedia";
    public const string GetMediaPath = "getmedia";

    public static async Task DefaultExceptionHandler(this CfHttpContext httpContext, Exception ex)
    {
        await httpContext.Response.SetStatusCode(500);
    }

    public static async Task HandleGetApiOverview(this Web.CfHttpContext httpContext, Func<RouteDescription, string> findUrl)
    {
        await httpContext.Response.WriteJsonReponse(new ApiOverview()
        {
            WebhookPath = findUrl(RouteDescription.New(WebhookPath)),
            PostMessagePath = findUrl(RouteDescription.New(PostMessagePath)),
            UploadMediaPath = findUrl(RouteDescription.New(UploadMediaPath)),
            GetMediaPath = findUrl(RouteDescription.New(GetMediaPath))
        });
    }

    public static async Task HandlePostMessage(this Web.CfHttpContext httpContext, WhatsAppCloudApiClient apiClient, Func<CfHttpContext, System.Exception, Task> onException)
    {
        try
        {
            var message = await httpContext.Request.ReadJsonBody<MessageObject>();
            var response = await apiClient.PostMessage(message);
            await httpContext.Response.WriteJsonReponse(response);
        }
        catch (Exception ex)
        {
            await onException(httpContext, ex);
        }
    }

    public static async Task HandlePostMedia(this Web.CfHttpContext httpContext, WhatsAppCloudApiClient apiClient, Func<CfHttpContext, Exception, Task> onException)
    {
        try
        {
            var request = await httpContext.Request.ReadJsonBody<UploadMediaRequest>();
            var response = await apiClient.UploadMedia(request.Content, request.ContentType, request.FilePath);
            await httpContext.Response.WriteJsonReponse(response);
        }
        catch (Exception ex)
        {
            await onException(httpContext, ex);
        }
    }

    public static async Task HandleGetMedia(this CfHttpContext httpContext, string mediaId, WhatsAppCloudApiClient apiClient, Func<CfHttpContext, Exception, Task> onException)
    {
        try
        {
            var mediaResponse = await apiClient.GetMedia(mediaId);
            httpContext.Response.Response.SetContentTypeHeader(mediaResponse.Content.Headers.ContentType.MediaType);

            await mediaResponse.Content.CopyToAsync(httpContext.Response.Body());
            await httpContext.Response.Body().FlushAsync();
        }
        catch (Exception ex)
        {
            await onException(httpContext, ex);
        }
    }

    public static async Task HandleGetChallenge(this CfHttpContext httpContext, WhatsAppCloudApiClient apiClient, Func<CfHttpContext, Exception, Task> onException)
    {
        try
        {
            var challenge = httpContext.Request.Request.GetChallengeQuery();
            await httpContext.Response.WriteAsync(challenge);
        }
        catch (Exception ex)
        {
            await onException(httpContext, ex);
        }
    }

    public static async Task HandlePostToWebhook(
        this CfHttpContext httpContext,
        string appSecret,
        Func<NotificationPayloadObject, Task> onMessage,
        Func<CfHttpContext, Exception, Task> onException)
    {
        try
        {
            string body = await new StreamReader(httpContext.Request.Body()).ReadToEndAsync();

            if (string.IsNullOrEmpty(body))
            {
                body = string.Empty;
            }

            var requestSignature = httpContext.Request.GetHeader("x-hub-signature-256");
            var asciiEscapedBody = EncodeNonAsciiCharacters(body);
            var computedSignature = CalculateSignature(appSecret, asciiEscapedBody);

            if (requestSignature != computedSignature)
            {
                // Bad Request
                await httpContext.Response.SetStatusCode(400);
                return;
            }

            var notificationObject = Metapsi.Serialize.FromJson<NotificationPayloadObject>(body);
            await onMessage(notificationObject);
        }
        catch (Exception ex)
        {
            await onException(httpContext, ex);
        }

        await httpContext.Response.SetStatusCode(200);
    }

    // Lifted from
    // https://github.com/gabrieldwight/Whatsapp-Business-Cloud-Api-Net/blob/1eb60fde2de9cb5c3d72638442416b8b151d408e/WhatsappBusiness.CloudApi/Extensions/FacebookWebhookHelper.cs#L20
    public static string CalculateSignature(string appSecret, string payload)
    {
        /*
         Please note that the calculation is made on the escaped unicode version of the payload, with lower case hex digits.
         If you just calculate against the decoded bytes, you will end up with a different signature.
         For example, the string äöå should be escaped to \u00e4\u00f6\u00e5.
         */

        using HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(appSecret));
        hmac.Initialize();
        byte[] hashArray = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        string hash = $"SHA256={BitConverter.ToString(hashArray).Replace("-", string.Empty)}";

        return hash.ToLower();
    }


    // Lifted from
    // https://itecnote.com/tecnote/c-convert-a-unicode-string-to-an-escaped-ascii-string/
    static string EncodeNonAsciiCharacters(string value)
    {
        StringBuilder sb = new StringBuilder();
        foreach (char c in value)
        {
            if (c > 127)
            {
                // This character is too big for ASCII
                string encodedValue = "\\u" + ((int)c).ToString("x4");
                sb.Append(encodedValue);
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }
}