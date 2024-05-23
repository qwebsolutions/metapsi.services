using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using System.Net.Http;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Linq;


namespace Metapsi.Chat;

public static class ChatServiceExtensions
{
    public static async Task<RouteHandlerBuilder> AddChatBackend(
        this ApplicationSetup applicationSetup,
        ImplementationGroup ig,
        IEndpointRouteBuilder endpoint,
        string dbPath)
    {
        var overviewRoute = await endpoint.UseDocsGroup(applicationSetup, ig, dbPath,
            b =>
            {
                b.AddDoc<Conversation>(x => x.Id, async (cc) => new Conversation());
                b.AddDoc<UserConversationEndpoint>(x => x.Id, async (cc) => new UserConversationEndpoint());
                b.AddDoc<Message>(x => x.Id, async (cc) => new Message());
            });

        var conversationDocs = ServiceDoc.GetDocApi<Conversation>();
        var endpointDocs = ServiceDoc.GetDocApi<UserConversationEndpoint>();
        var messageDocs = ServiceDoc.GetDocApi<Message>();

        var api = endpoint.MapGroup("api");
        api.MapPost(nameof(ChatClientApi.CreateConversation), async (CommandContext commandContext, CreateConversationRequest input) =>
        {
            var saveResult = await commandContext.Do(conversationDocs.Save, new Conversation());
            if (saveResult.New == null)
                return Results.Text("Conversation already exists", statusCode: StatusCodes.Status412PreconditionFailed);

            CreateConversationResponse response = new CreateConversationResponse()
            {
                ConversationId = saveResult.New.Doc.Id
            };

            foreach (var userId in input.UserIds)
            {
                var newEndpointResult = await commandContext.Do(endpointDocs.Save, new UserConversationEndpoint()
                {
                    ConversationId = saveResult.New.Doc.Id,
                    UserId = userId
                });

                response.EndpointMappings.Add(new EndpointMapping()
                {
                    UserId = userId,
                    EndpointId = newEndpointResult.New.Doc.Id
                });
            }

            return Results.Ok(response);
        });

        api.MapPost(nameof(ChatClientApi.PostMessage), async (CommandContext commandContext, PostMessageRequest request) =>
        {
            var saveMessageResult = await commandContext.Do(messageDocs.Save, new Message()
            {
                FromEndpointId = request.FromEndpointId,
                MessageText = request.Message,
            });

            return new PostMessageResponse()
            {
                MessageId = saveMessageResult.New.Doc.Id
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