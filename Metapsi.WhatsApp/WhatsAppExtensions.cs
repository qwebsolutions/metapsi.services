using System;
using System.Net.Http.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Metapsi.WhatsApp;

public class WhatsAppConfiguration
{
    public HttpClient HttpClient { get; set; } = new HttpClient();
    public string WhatsappBearerToken { get; set; }
    public string WhatsappAppSecret { get; set; }
    public string WhatsappBusinessNumber { get; set; }
    public string WhatsappBusinessNumberId { get; set; }

    public WhatsAppConfiguration()
    {
        // Really? Really?!?!
        HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36");
    }
}

public class WebHookConfiguration
{
    public string WhatsAppAppSecret { get; set; }
    public List<Func<WhatsappApiMessage, object>> ContractConverters { get; set; } = new();
    public Func<CommandContext, object, Task> OnMessage { get; set; }
}

public static class WhatsAppExtensions
{
    public static void AddContractConverter(this WebHookConfiguration webHookConfiguration, Func<WhatsappApiMessage, object> converter)
    {
        webHookConfiguration.ContractConverters.Add(converter);
    }

    public static object ConvertMessage(this WebHookConfiguration webHookConfiguration, WhatsappApiMessage received)
    {
        foreach (var converter in webHookConfiguration.ContractConverters)
        {
            try
            {
                var message = converter(received);
                if (message != null)
                    return message;
            }
            catch (Exception ex)
            {

            }
        }
        return null;
    }

    public static void UseWhatsApp(
        this IEndpointRouteBuilder endpoint,
        WhatsAppConfiguration configuration,
        Func<CommandContext, object, Task> onMessage,
        Action<WebHookConfiguration> configureWebHook = null)
    {
        WebHookConfiguration webHookConfiguration = new WebHookConfiguration()
        {
            WhatsAppAppSecret = configuration.WhatsappAppSecret,
            OnMessage = onMessage
        };

        webHookConfiguration.AddContractConverter(IncomingTextMessage);
        webHookConfiguration.AddContractConverter(IncomingButtonReply);
        webHookConfiguration.AddContractConverter(IncomingListReply);
        webHookConfiguration.AddContractConverter(IncomingStatusUpdate);
        webHookConfiguration.AddContractConverter(IncomingMediaMessage);
        if (configureWebHook != null)
        {
            configureWebHook(webHookConfiguration);
        }

        var webhookEndpoint = endpoint.MapGroup("webhook");
        var webHookRoute = webhookEndpoint.UseWhatsAppWebHook(webHookConfiguration);
        webHookRoute.WithName("webhook");

        var postMessageEndpoint = endpoint.MapGroup("postmessage");

        var textMessagesRoute = postMessageEndpoint.MapPostMessage<WhatsAppOutboundTextMessage>(configuration);
        textMessagesRoute.WithName($"post-{nameof(WhatsAppOutboundTextMessage)}");

        var buttonRoute = postMessageEndpoint.MapPostMessage<WhatsAppOutboundButtonMessage>(configuration);
        buttonRoute.WithName($"post-{nameof(WhatsAppOutboundButtonMessage)}");

        var listRoute = postMessageEndpoint.MapPostMessage<WhatsAppOutboundListMessage>(configuration);
        listRoute.WithName($"post-{nameof(WhatsAppOutboundListMessage)}");

        var getMediaEndpoint = endpoint.MapGroup("getmedia");
        var getMediaRoute = getMediaEndpoint.MapGetMedia(configuration);
        getMediaRoute.WithName("get-media");

        endpoint.MapGet("/", async (CommandContext commandContext, HttpContext httpContext) =>
        {
            var linkGenerator = httpContext.RequestServices.GetRequiredService<LinkGenerator>();
            var webhookUrl = linkGenerator.GetPathByName(httpContext, $"webhook");
            var postTextUrl = linkGenerator.GetPathByName(httpContext, $"post-{typeof(WhatsAppOutboundTextMessage).Name}");
            var postButtonUrl = linkGenerator.GetPathByName(httpContext, $"post-{typeof(WhatsAppOutboundButtonMessage).Name}");
            var postListUrl = linkGenerator.GetPathByName(httpContext, $"post-{typeof(WhatsAppOutboundListMessage).Name}");
            var getMediaUrl = linkGenerator.GetPathByName("get-media", new RouteValueDictionary()
            {
                { "mediaId", "mediaId" }
            });

            return Results.Ok(new
            {
                WebhookPath = webhookUrl,
                PostWhatsAppTextMessagePath = postTextUrl,
                PostWhatsAppButtonMessagePath = postButtonUrl,
                PostWhatsAppListMessagePath = postListUrl,
                GetMediaPath = getMediaUrl
            });
        });
    }

    public static RouteHandlerBuilder MapPostMessage<T>(
        this IEndpointRouteBuilder postGroup,
        WhatsAppConfiguration configuration)
        where T : IWhatsAppOutboundMessage
    {
        return postGroup.MapPost($"/{typeof(T).Name}", async (CommandContext commandContext, HttpContext httpContext, T message) =>
        {
            var result = await PostMessage(message, configuration);

            if (!string.IsNullOrEmpty(result.error.message))
            {
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                commandContext.Logger.LogError(result.error.message);
                Console.WriteLine(result.error.message);
            }
        });
    }

    public static RouteHandlerBuilder MapGetMedia(
        this IEndpointRouteBuilder getMediaGroup,
        WhatsAppConfiguration configuration)
    {
        return getMediaGroup.MapGet("/{mediaId}", async (CommandContext commandContext, HttpContext httpContext, string mediaId) =>
        {
            var getMediaUrlRequest = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://graph.facebook.com/v18.0/{mediaId}");
            getMediaUrlRequest.Headers.Add("Authorization", "Bearer " + configuration.WhatsappBearerToken);

            var getMediaUrlResult = await configuration.HttpClient.SendAsync(getMediaUrlRequest);
            getMediaUrlResult.EnsureSuccessStatusCode();

            var resultBody = await getMediaUrlResult.Content.ReadAsStringAsync();
            Console.WriteLine(resultBody);

            var mediaUrl = Metapsi.Serialize.FromJson<MediaUrl>(resultBody);

            var getMediaContentRequest = new HttpRequestMessage(HttpMethod.Get, mediaUrl.url);
            getMediaContentRequest.Headers.Add("Authorization", "Bearer " + configuration.WhatsappBearerToken);
            
            var getMediaContentResult = await configuration.HttpClient.SendAsync(getMediaContentRequest);
            getMediaContentResult.EnsureSuccessStatusCode();

            httpContext.Response.Headers.ContentType = mediaUrl.mime_type;
            await getMediaContentResult.Content.CopyToAsync(httpContext.Response.Body);
            await httpContext.Response.Body.FlushAsync();
        });
    }

    public static async Task<WhatsappSendMessageResult> PostMessage<T>(
        T message,
        WhatsAppConfiguration configuration)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"https://graph.facebook.com/v18.0/{configuration.WhatsappBusinessNumberId}/messages");
        request.Headers.Add("Authorization", "Bearer " + configuration.WhatsappBearerToken);
        request.Content = JsonContent.Create(message);

        var postResult = await configuration.HttpClient.SendAsync(request);

        var resultBody = await postResult.Content.ReadAsStringAsync();
        Console.WriteLine(resultBody);

        var result = Metapsi.Serialize.FromJson<WhatsappSendMessageResult>(resultBody);
        return result;
    }

    /*
     * {
   "object":"whatsapp_business_account",
   "entry":[
      {
         "id":"111838255246355",
         "changes":[
            {
               "value":{
                  "messaging_product":"whatsapp",
                  "metadata":{
                     "display_phone_number":"15550685239",
                     "phone_number_id":"112033771893883"
                  },
                  "contacts":[
                     {
                        "profile":{
                           "name":"Calin Dobos"
                        },
                        "wa_id":"40726321476"
                     }
                  ],
                  "messages":[
                     {
                        "from":"40726321476",
                        "id":"wamid.HBgLNDA3MjYzMjE0NzYVAgASGBQzQTMxNDlCNDAwNTJEQ0Y5QTg2MAA=",
                        "timestamp":"1722446559",
                        "type":"image",
                        "image":{
                           "caption":"With caption",
                           "mime_type":"image\/jpeg",
                           "sha256":"vi4WyFgHw1qLCbB3QeSaU634eK2miyl74mHZMfnJ7sE=",
                           "id":"821116500122343"
                        }
                     }
                  ]
               },
               "field":"messages"
            }
         ]
      }
   ]
}
    
*/

    public static object IncomingTextMessage(WhatsappApiMessage apiMessage)
    {
        if (apiMessage.entry.FirstOrDefault() != null)
        {
            if (apiMessage.entry.First().changes.FirstOrDefault() != null)
            {
                if (apiMessage.entry.First().changes.First().value.messages.Any())
                {
                    var userMessage = apiMessage.entry.First().changes.First().value.messages.First();
                    var businessPhoneNumber = apiMessage.entry.First().changes.First().value.metadata.display_phone_number;
                    var userPhone = userMessage.from;
                    if (userMessage.text != null)
                    {
                        var messageText = userMessage.text.body;
                        var offset = DateTimeOffset.FromUnixTimeSeconds(long.Parse(userMessage.timestamp));

                        return new IncomingTextMessage()
                        {
                            PhoneNumber = userPhone,
                            Text = messageText,
                            Timestamp = offset.UtcDateTime.Roundtrip()
                        };
                    }
                }
            }
        }

        return null;
    }

    public static object IncomingMediaMessage(WhatsappApiMessage apiMessage)
    {
        if (apiMessage.entry.FirstOrDefault() != null)
        {
            if (apiMessage.entry.First().changes.FirstOrDefault() != null)
            {
                if (apiMessage.entry.First().changes.First().value.messages.Any())
                {
                    var userMessage = apiMessage.entry.First().changes.First().value.messages.First();
                    var businessPhoneNumber = apiMessage.entry.First().changes.First().value.metadata.display_phone_number;
                    var userPhone = userMessage.from;
                    MessageMedia messageMedia = null;
                    if (userMessage.image != null)
                    {
                        messageMedia = userMessage.image;
                    }
                    else if (userMessage.video != null)
                    {
                        messageMedia = userMessage.video;
                    }
                    else if (userMessage.audio != null)
                    {
                        messageMedia = userMessage.audio;
                    }
                    else if (userMessage.document != null)
                    {
                        messageMedia = userMessage.document;
                    }

                    if (messageMedia != null)
                    {
                        var offset = DateTimeOffset.FromUnixTimeSeconds(long.Parse(userMessage.timestamp));

                        return new IncomingMediaMessage()
                        {
                            PhoneNumber = userPhone,
                            Caption = messageMedia.caption,
                            MimeType = messageMedia.mime_type,
                            FileName = messageMedia.filename,
                            MediaId = messageMedia.id,
                            Timestamp = offset.UtcDateTime.Roundtrip()
                        };
                    }
                }
            }
        }

        return null;
    }

    public static object IncomingButtonReply(WhatsappApiMessage apiMessage)
    {
        if (apiMessage.entry.FirstOrDefault() != null)
        {
            if (apiMessage.entry.First().changes.FirstOrDefault() != null)
            {
                if (apiMessage.entry.First().changes.First().value.messages.Any())
                {
                    var userMessage = apiMessage.entry.First().changes.First().value.messages.First();
                    var businessPhoneNumber = apiMessage.entry.First().changes.First().value.metadata.display_phone_number;
                    var userPhone = userMessage.from;
                    var offset = DateTimeOffset.FromUnixTimeSeconds(long.Parse(userMessage.timestamp));

                    if (userMessage.interactive != null)
                    {
                        if (userMessage.interactive.button_reply != null)
                        {
                            return new IncomingButtonReplyMessage()
                            {
                                PhoneNumber = userPhone,
                                RelatedMessageId = userMessage.context?.id,
                                ButtonId = userMessage.interactive.button_reply.id,
                                Timestamp = offset.UtcDateTime.Roundtrip()
                            };
                        }
                    }

                }
            }
        }

        return null;
    }


    public static object IncomingListReply(WhatsappApiMessage apiMessage)
    {
        if (apiMessage.entry.FirstOrDefault() != null)
        {
            if (apiMessage.entry.First().changes.FirstOrDefault() != null)
            {
                if (apiMessage.entry.First().changes.First().value.messages.Any())
                {
                    var userMessage = apiMessage.entry.First().changes.First().value.messages.First();
                    var userPhone = userMessage.from;
                    var offset = DateTimeOffset.FromUnixTimeSeconds(long.Parse(userMessage.timestamp));

                    if (userMessage.interactive != null)
                    {
                        if (userMessage.interactive.list_reply != null)
                        {
                            return new IncomingListReplyMessage()
                            {
                                PhoneNumber = userPhone,
                                RelatedMessageId = userMessage.context?.id,
                                ItemId = userMessage.interactive.list_reply.id,
                                ItemTitle = userMessage.interactive.list_reply.title,
                                ItemDescription = userMessage.interactive.list_reply.description,
                                Timestamp = offset.UtcDateTime.Roundtrip()
                            };
                        }
                    }
                }
            }
        }

        return null;
    }

    public static object IncomingStatusUpdate(WhatsappApiMessage apiMessage)
    {
        if (apiMessage.entry.FirstOrDefault() != null)
        {
            if (apiMessage.entry.First().changes.FirstOrDefault() != null)
            {
                if (apiMessage.entry.First().changes.First().value.statuses.Any())
                {
                    var messageStatus = apiMessage.entry.First().changes.First().value.statuses.First();
                    var userPhone = messageStatus.recipient_id;
                    var offset = DateTimeOffset.FromUnixTimeSeconds(long.Parse(messageStatus.timestamp));
                    var messageId = messageStatus.id;

                    return new IncomingStatusUpdate()
                    {
                        PhoneNumber = userPhone,
                        Timestamp = offset.UtcDateTime.Roundtrip(),
                        MessageId = messageId,
                        Status = messageStatus.status
                    };
                }
            }
        }

        return null;
    }
}