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

public class NotificationBodyEvent : IData
{
    public string RequestJson { get; set; }
}


public static class WhatsAppServiceExtensions
{
    public const string WebhookPath = "webhook";
    public const string PostMessagePath = "postmessage";
    public const string UploadMediaPath = "uploadmedia";
    public const string GetMediaPath = "getmedia";

    public static void UseWhatsApp(
        this IEndpointRouteBuilder endpoint,
        WhatsAppCloudApiClient cloudApiClient,
        string whatsAppSecret,
        Func<CommandContext, NotificationPayloadObject, Task> onMessage,
        Func<CommandContext, HttpContext, System.Exception, Task> onException = null)
    {
        if (onException == null)
        {
            onException = async (CommandContext commandContext, HttpContext httpContext, System.Exception ex) =>
            {

            };
        }

        var webhookEndpoint = endpoint.MapGroup(WebhookPath).MapWhatsAppWebHook(whatsAppSecret, onMessage, onException);
        var postMessageEndpoint = endpoint.MapGroup(PostMessagePath).MapPostMessage(cloudApiClient, onException);
        var postMediaEndpoint = endpoint.MapGroup(UploadMediaPath).MapPostMedia(cloudApiClient, onException);
        var getMediaEndpoint = endpoint.MapGroup(GetMediaPath).MapGetMedia(cloudApiClient, onException);

        endpoint.MapGet("/", async (CommandContext commandContext, HttpContext httpContext) =>
        {
            return Results.Ok(new
            {
                WebhookPath,
                PostMessagePath,
                UploadMediaPath,
                GetMediaPath
            });
        });
    }

    public static RouteHandlerBuilder MapPostMessage(
        this IEndpointRouteBuilder postGroup,
        WhatsAppCloudApiClient apiClient,
        Func<CommandContext, HttpContext, System.Exception, Task> onException)
    {
        return postGroup.MapPost($"/", async (CommandContext commandContext, HttpContext httpContext, MessageObject message) =>
        {
            try
            {
                var response = await apiClient.PostMessage(message);
                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                await onException(commandContext, httpContext, ex);
                return Results.Problem();
            }
        });
    }

    public static RouteHandlerBuilder MapPostMedia(
        this IEndpointRouteBuilder postGroup,
        WhatsAppCloudApiClient apiClient,
        Func<CommandContext, HttpContext, System.Exception, Task> onException)
    {
        return postGroup.MapPost($"/", async (CommandContext commandContext, HttpContext httpContext, UploadMediaRequest request) =>
        {
            try
            {
                var response = await apiClient.UploadMedia(request.Content, request.ContentType, request.FilePath);
                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                await onException(commandContext, httpContext, ex);
                return Results.Problem();
            }
        });
    }

    public static RouteHandlerBuilder MapWhatsAppWebHook(
           this IEndpointRouteBuilder endpoint,
           string appSecret,
           Func<CommandContext, NotificationPayloadObject, Task> onMessage,
           Func<CommandContext, HttpContext, System.Exception, Task> onException)
    {
        endpoint.MapGet("/", async (HttpContext httpContext) =>
        {
            httpContext.Request.Query.TryGetValue("hub.challenge", out var challenge);
            await httpContext.Response.WriteAsync(challenge.First());
        });

        return endpoint.MapPost("/", async (CommandContext commandContext, HttpContext httpContext) =>
        {
            try
            {
                string body = await new StreamReader(httpContext.Request.Body).ReadToEndAsync();

                commandContext.PostEvent(new NotificationBodyEvent()
                {
                    RequestJson = body
                });

                if (string.IsNullOrEmpty(body))
                {
                    body = string.Empty;
                }

                var requestSignature = httpContext.Request.Headers["x-hub-signature-256"];
                var asciiEscapedBody = EncodeNonAsciiCharacters(body);
                var computedSignature = CalculateSignature(appSecret, asciiEscapedBody);

                if (requestSignature != computedSignature)
                {
                    return Results.BadRequest();
                }

                Console.WriteLine($"Decrypted message {body}");

                var notificationObject = Metapsi.Serialize.FromJson<NotificationPayloadObject>(body);
                await onMessage(commandContext, notificationObject);
            }
            catch (Exception ex)
            {
                await onException(commandContext, httpContext, ex);
            }
            return Results.Ok();
        });
    }

    public static RouteHandlerBuilder MapGetMedia(
        this IEndpointRouteBuilder getMediaGroup,
        WhatsAppCloudApiClient apiClient,
        Func<CommandContext, HttpContext, System.Exception, Task> onException)
    {
        return getMediaGroup.MapGet("/{mediaId}", async (CommandContext commandContext, HttpContext httpContext, string mediaId) =>
        {
            try
            {
                var mediaResponse = await apiClient.GetMedia(mediaId);
                httpContext.Response.Headers.ContentType = mediaResponse.Content.Headers.ContentType.MediaType;
                Console.WriteLine(mediaResponse.Content.Headers.ContentType.MediaType);
                await mediaResponse.Content.CopyToAsync(httpContext.Response.Body);
                await httpContext.Response.Body.FlushAsync();
            }
            catch (Exception ex)
            {
                await onException(commandContext, httpContext, ex);
                throw;
            }
        });
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