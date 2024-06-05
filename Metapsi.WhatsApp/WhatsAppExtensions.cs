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
        WebHookConfiguration webHookConfiguration = new WebHookConfiguration();
        webHookConfiguration.OnMessage = onMessage;
        webHookConfiguration.AddContractConverter(IncomingTextMessage);
        webHookConfiguration.AddContractConverter(IncomingButtonReply);
        webHookConfiguration.AddContractConverter(IncomingListReply);
        webHookConfiguration.AddContractConverter(IncomingStatusUpdate);
        if (configureWebHook != null)
        {
            configureWebHook(webHookConfiguration);
        }
        var webhookEndpoint = endpoint.MapGroup("webhook");
        var webHookRoute = webhookEndpoint.UseWhatsAppWebHook(webHookConfiguration);
        webHookRoute.WithName("webhook");
        
        var postEndpoint = endpoint.MapGroup("postmessage");
        
        var textMessagesRoute = postEndpoint.MapPostMessage<WhatsAppOutboundTextMessage>(configuration);
        textMessagesRoute.WithName($"post-{nameof(WhatsAppOutboundTextMessage)}");

        var buttonRoute = postEndpoint.MapPostMessage<WhatsAppOutboundButtonMessage>(configuration);
        buttonRoute.WithName($"post-{nameof(WhatsAppOutboundButtonMessage)}");

        var listRoute = postEndpoint.MapPostMessage<WhatsAppOutboundListMessage>(configuration);
        listRoute.WithName($"post-{nameof(WhatsAppOutboundListMessage)}");

        endpoint.MapGet("/", async (CommandContext commandContext, HttpContext httpContext) =>
        {
            var linkGenerator = httpContext.RequestServices.GetRequiredService<LinkGenerator>();
            var webhookUrl = linkGenerator.GetPathByName(httpContext, $"webhook");
            var postTextUrl = linkGenerator.GetPathByName(httpContext, $"post-{typeof(WhatsAppOutboundTextMessage).Name}");
            var postButtonUrl = linkGenerator.GetPathByName(httpContext, $"post-{typeof(WhatsAppOutboundButtonMessage).Name}");
            var postListUrl = linkGenerator.GetPathByName(httpContext, $"post-{typeof(WhatsAppOutboundListMessage).Name}");

            return Results.Ok(new
            {
                WebhookPath = webhookUrl,
                PostWhatsAppTextMessagePath = postTextUrl,
                PostWhatsAppButtonMessagePath = postButtonUrl,
                PostWhatsAppListMessagePath = postListUrl
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