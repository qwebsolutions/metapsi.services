using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;

namespace Metapsi.WhatsApp;

public static class WhatsAppWebHook
{
    public static RouteHandlerBuilder UseWhatsAppWebHook(
        this IEndpointRouteBuilder endpoint,
        WebHookConfiguration configuration)
    {
        endpoint.MapGet("/", async (HttpContext httpContext) =>
        {
            httpContext.Request.Query.TryGetValue("hub.challenge", out var challenge);
            await httpContext.Response.WriteAsync(challenge.First());
        });

        return endpoint.MapPost("/", async (CommandContext commandContext, HttpContext httpContext) =>
        {
            string body = await new StreamReader(httpContext.Request.Body).ReadToEndAsync();

            var requestSignature = httpContext.Request.Headers["x-hub-signature-256"];
            var asciiEscapedBody = EncodeNonAsciiCharacters(body);
            var computedSignature = CalculateSignature(configuration.WhatsAppAppSecret, asciiEscapedBody);

            if (requestSignature != computedSignature)
            {
                return Results.BadRequest();
            }

            var apiMessage = ParseWithWarnings(commandContext, body);

            var message = configuration.ConvertMessage(apiMessage);

            if (message != null)
            {
                await configuration.OnMessage(commandContext, message);
            }
            else
            {
                throw new Exception("WhatsApp message contains multiple entries. Parsing is not yet implemented");

                //commandContext.RaiseNotification(new Metapsi.LoggerService.Error()
                //{
                //    Message = "Cannot process incoming WhatsApp message",
                //    Details = body
                //});
            }


            return Results.Ok();
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

    private static WhatsappApiMessage ParseWithWarnings(CommandContext commandContext, string jsonBody)
    {
        var apiMessage = Serialize.FromJson<WhatsappApiMessage>(jsonBody);

        if (apiMessage.entry.Count > 1)
        {
            throw new Exception("WhatsApp message contains multiple entries. Parsing is not yet implemented");
            //commandContext.RaiseNotification(new Metapsi.LoggerService.Warning()
            //{
            //    Message = "WhatsApp message contains multiple entries. Parsing is not yet implemented",
            //    Details = jsonBody
            //});
        }
        else
        {

            if (apiMessage.entry.SelectMany(x => x.changes).Count() > 1)
            {
                throw new Exception("WhatsApp message contains multiple entries. Parsing is not yet implemented");
                //commandContext.RaiseNotification(new Metapsi.LoggerService.Warning()
                //{
                //    Message = "WhatsApp message contains multiple changes. Parsing is not yet implemented",
                //    Details = jsonBody
                //});
            }
            else
            {
                if (apiMessage.entry.SelectMany(x => x.changes).SelectMany(x => x.value.statuses).Count() > 1)
                {
                    throw new Exception("WhatsApp message contains multiple entries. Parsing is not yet implemented");
                    //commandContext.RaiseNotification(new Metapsi.LoggerService.Warning()
                    //{
                    //    Message = "WhatsApp message contains multiple statuses. Parsing is not yet implemented",
                    //    Details = jsonBody
                    //});
                }
            }
        }

        return apiMessage;
    }
}
