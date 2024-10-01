using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using System.Net.Http;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Linq;
using Metapsi.Sqlite;
using static Metapsi.ServiceDoc;


namespace Metapsi.Chat;

public static class ChatServiceExtensions
{
    public static async Task<RouteHandlerBuilder> UseMetapsiChat(
        this IEndpointRouteBuilder endpoint,
        ApplicationSetup applicationSetup,
        ImplementationGroup ig,
        ServiceDoc.DbQueue dbQueue)
    {
        var overviewRoute = await endpoint.UseDocs(
            dbQueue,
            b =>
            {
                b.AddDoc<Conversation>(x => x.Id, async () => new Conversation());
                b.AddDoc<UserConversationEndpoint>(x => x.Id, async (cc) => new UserConversationEndpoint());
                b.AddDoc<Message>(x => x.Id, async (cc) => new Message());
            });

        var api = endpoint.MapGroup("api");
        api.MapPost(nameof(ChatClientApi.CreateConversation), async (CommandContext commandContext, CreateConversationRequest input) =>
        {
            var conversation = new Conversation();
            await dbQueue.SaveDocument(conversation);
            
            CreateConversationResponse response = new CreateConversationResponse()
            {
                ConversationId = conversation.Id
            };

            foreach (var userId in input.UserIds)
            {
                var endpoint = new UserConversationEndpoint()
                {
                    ConversationId = conversation.Id,
                    UserId = userId
                };
                await dbQueue.SaveDocument(endpoint);

                response.EndpointMappings.Add(new EndpointMapping()
                {
                    UserId = userId,
                    EndpointId = endpoint.Id
                });
            }

            return Results.Ok(response);
        });

        api.MapPost(nameof(ChatClientApi.PostMessage), async (CommandContext commandContext, PostMessageRequest request) =>
        {
            var newMessage = new Message()
            {
                FromEndpointId = request.FromEndpointId,
                MessageText = request.Message,
            };
            await dbQueue.SaveDocument(newMessage);

            return new PostMessageResponse()
            {
                MessageId = newMessage.Id
            };
        });

        return overviewRoute;
    }
}

public class ChatClient
{
    public ChatClient(HttpClient httpClient, string apiUrl)
    {
        this.HttpClient = httpClient;
        this.ApiUrl = apiUrl;
    }

    public HttpClient HttpClient { get; set; }
    public string ApiUrl { get; set; }

    public ServiceDoc.Client<Conversation> Conversations =>
        new ServiceDoc.Client<Conversation>()
        {
            HttpClient = this.HttpClient,
            ApiUrl = this.ApiUrl.TrimEnd('/').Replace("/api", string.Empty) + "/" + nameof(Conversations) + "/api/",
        };

    public ServiceDoc.Client<UserConversationEndpoint> Endpoints =>
        new ServiceDoc.Client<UserConversationEndpoint>()
        {
            HttpClient = this.HttpClient,
            ApiUrl = this.ApiUrl.TrimEnd('/').Replace("/api", string.Empty) + "/" + nameof(UserConversationEndpoint) + "/api/",
        };

    public ServiceDoc.Client<Metapsi.Chat.Message> Messages =>
        new ServiceDoc.Client<Message>()
        {
            HttpClient = this.HttpClient,
            ApiUrl = this.ApiUrl.TrimEnd('/').Replace("/api", string.Empty) + "/" + nameof(Message) + "/api/",
        };
}

public class CreateConversationRequest
{
    public List<string> UserIds { get; set; } = new List<string>();
}

public class EndpointMapping
{
    public string UserId { get; set; }
    public string EndpointId { get; set; }
}

public class CreateConversationResponse
{
    public string ConversationId { get; set; }
    public List<EndpointMapping> EndpointMappings { get; set; } = new();
}

public class PostMessageRequest
{
    public string FromEndpointId { get; set; }
    public string Message { get; set; }
}

public class PostMessageResponse
{
    public string MessageId { get; set; }
}

public static class ChatClientApi
{
    public static async Task<CreateConversationResponse> CreateConversation(this ChatClient chatClient, params string[] userIds)
    {
        var response = await chatClient.HttpClient.PostAsJsonAsync(
            chatClient.ApiUrl + "/" + nameof(CreateConversation),
            new CreateConversationRequest()
            {
                UserIds = userIds.ToList()
            });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CreateConversationResponse>();
    }

    public static async Task<PostMessageResponse> PostMessage(this ChatClient chatClient, string fromEndpointId, string message)
    {
        var response = await chatClient.HttpClient.PostAsJsonAsync(
            chatClient.ApiUrl + "/" + nameof(PostMessage),
            new PostMessageRequest()
            {
                Message = message,
                FromEndpointId = fromEndpointId
            });
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<PostMessageResponse>();
    }
}