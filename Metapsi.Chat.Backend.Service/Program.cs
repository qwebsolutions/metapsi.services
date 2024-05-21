using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Metapsi.Messaging;


namespace Metapsi.Chat.Backend;

public class Parameters
{
    public int ApiPort { get; set; }
    public string PubRedisUrl { get; set; }
    public string SubRedisUrl { get; set; }
}

public class Program
{
    public const string ChatDb = "chat.db";

    public static async Task SetupService(Mds.ServiceSetup serviceSetup)
    {
        var parameters = serviceSetup.ParametersAs<Parameters>();
        var ig = serviceSetup.ApplicationSetup.AddImplementationGroup();

        var webServerRefs = serviceSetup.ApplicationSetup.AddWebServer(
            ig,
            parameters.ApiPort,
            "");


        var dbPath = serviceSetup.GetServiceDataFile(ChatDb);
        var conversationsUrl = await webServerRefs.RegisterDocsUi<Conversation>(dbPath);
        var endpointsList = await webServerRefs.RegisterDocsUi<ConversationEndpoint>(dbPath);
        var messagesList = await webServerRefs.RegisterDocsUi<ConversationMessage>(dbPath);

        var overviewUrl = webServerRefs.RegisterDocsOverview();

        webServerRefs.WebApplication.MapGet("/", () => Results.Redirect(overviewUrl)).AllowAnonymous().ExcludeFromDescription();

        // TODO: Not all commands should be enqueued on a state. This is quite relevant & changes A LOT
        var noActualState = webServerRefs.ApplicationSetup.AddBusinessState(new ChatService.State());

        var messaging = webServerRefs.ApplicationSetup.AddMessagingTransport(ig, parameters.PubRedisUrl);
        messaging.SubscribeTo(parameters.SubRedisUrl);

        //messaging.OnMessage<ChatMessageEvent>(async (CommandContext commandContext, ChatMessageEvent textMessage) =>
        //{
        //    var alreadySavedMessage = await commandContext.Do(NoSqlDoc.GetDocApi<ChatMessageEvent>().Get, textMessage.Id);
        //    if (alreadySavedMessage == null)
        //    {
        //        await commandContext.Do(NoSqlDoc.GetDocApi<ChatMessageEvent>().Save, textMessage);
        //    }
        //});

        webServerRefs.ApplicationSetup.MapEvent<ServiceDoc.NewDoc<ConversationMessage>>(e =>
        {
            e.Using(noActualState, ig).EnqueueCommand(async (CommandContext commandContext, ChatService.State state) =>
            {
                commandContext.RaiseNotification(new ChatUpdatedEvent()
                {
                    MessagingGroupId = e.EventData.Doc.ChatGroupId,
                    ChangeType = ChatUpdateType.NewMessage,
                    RelatedId = e.EventData.Doc.Id
                });
            });
        });

        var apiRoute = webServerRefs.WebApplication.MapGroup("api");

        webServerRefs.RegisterStaticFiles(typeof(Program).Assembly);
    }
}

public static class ChatService
{
    public class State
    {
    }
}
