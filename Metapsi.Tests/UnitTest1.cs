﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Metapsi.Sqlite;
using System.Net;
using System.Text.RegularExpressions;
using Metapsi.Html;

namespace Metapsi.Services.Tests;

[DocDescription("Description here")]
public class TestEntity
{
    public string Key { get; set; }
    public string Notes { get; set; }
    public string Created { get; set; } = string.Empty;
}

public class ConfigurationParameter
{
    public string Key { get; set; }
    public string Value { get; set; }
}

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public async Task StartServiceDocTestEntity()
    {
        var parameters = new Dictionary<string, string>();
        parameters["ServiceName"] = "TestService";
        parameters["InfrastructureName"] = "TestInfrastructure";

        var serviceSetup = Mds.ServiceSetup.New(parameters);
        await SetupService(serviceSetup);

        var app = serviceSetup.Revive();
        await app.SuspendComplete;
    }

    public static async Task SetupService(Mds.ServiceSetup serviceSetup)
    {
        var ig = serviceSetup.ApplicationSetup.AddImplementationGroup();
        var configurationDb = serviceSetup.GetServiceDataFile("test.db");

        var appBuilder = WebApplication.CreateBuilder().AddMetapsi(serviceSetup.ApplicationSetup, ig);
        var app = appBuilder.Build().UseMetapsi(serviceSetup.ApplicationSetup);

        var configurationUrl = await app.MapGroup("docs").UseDocs(
            new ServiceDoc.DbQueue(new SqliteQueue(configurationDb)),
            b =>
            {
                b.AddDoc<TestEntity>(
                    x => x.Key,
                    b =>
                    {
                        b.SetFrontendNew(async () => new TestEntity()
                        {
                            Key = Guid.NewGuid().ToString(),
                            Notes = "Manually edited in " + serviceSetup.ServiceName
                        });
                    });
            });

        //app.MapGet("/", () => Results.Redirect(configurationUrl)).AllowAnonymous().ExcludeFromDescription();
    }

    //[TestMethod]
    //public async Task StartChatService()
    //{
    //    var applicationSetup = ApplicationSetup.New();
    //    var ig = applicationSetup.AddImplementationGroup();
    //    var testsPath = Metapsi.RelativePath.SearchUpfolder(RelativePath.From.EntryPath, "Metapsi.Tests");
    //    var testDataFolder = System.IO.Path.Combine(testsPath, "TestData");
    //    System.IO.Directory.CreateDirectory(testDataFolder);
    //    var chatDbPath = System.IO.Path.Combine(testDataFolder, "chat.db");

    //    var appBuilder = WebApplication.CreateBuilder().AddMetapsi(applicationSetup, ig);
    //    var webApp = appBuilder.Build();
    //    webApp.UseMetapsi(applicationSetup);
    //    var chatEndpoint = webApp.MapGroup("chat");
    //    var dbQueue = new ServiceDoc.DbQueue(new SqliteQueue(chatDbPath));
    //    var chatOverview = await chatEndpoint.UseMetapsiChat(applicationSetup, ig, dbQueue);
    //    chatOverview.WithMetadata(new EndpointNameMetadata("chat-overview"));

    //    var configEndpoint = webApp.MapGroup("config");
    //    await configEndpoint.UseDocs(
    //        dbQueue,
    //        b =>
    //        {
    //            b.AddDoc<ConfigurationParameter>(
    //                x => x.Key,
    //                b =>
    //                {
    //                    b.SetFrontendNew(async () =>
    //                    {
    //                        var list = await dbQueue.ListDocuments<ConfigurationParameter>();
    //                        return new ConfigurationParameter()
    //                        {
    //                            Key = list.Count.ToString()
    //                        };
    //                    });
    //                });
    //        });

    //    webApp.MapGet("/", (HttpContext httpContext) =>
    //    {
    //        var linkGenerator = httpContext.RequestServices.GetRequiredService<LinkGenerator>();
    //        return Results.Redirect(linkGenerator.GetPathByName(httpContext, "chat-overview"));
    //    });

    //    var app = applicationSetup.Revive();

    //    var httpClient = new HttpClient();

    //    await CheckUntilUrlAvailable(httpClient, "http://localhost:5000/chat/Conversation/api");

    //    var conversationClient = new ServiceDoc.Client<Chat.Conversation>(httpClient, "http://localhost:5000/chat/Conversation/api");
    //    await conversationClient.Save(new Chat.Conversation()
    //    {
    //        Id = "Salvat din test",
    //    });

    //    var chatClient = new ChatClient(httpClient, "http://localhost:5000/chat/api");
    //    var createConversationResponse = await chatClient.CreateConversation("Ionică", "Mirel");
    //    var message = await chatClient.PostMessage(createConversationResponse.EndpointMappings.First().EndpointId, "Primul mesaj!");

    //    await app.SuspendComplete;
    //}

    private static async Task CheckUntil(Func<Task<bool>> check, int attempts = 10)
    {
        for (int i = 0; i < attempts; i++)
        {
            if (await check())
            {
                return;
            }
            else
            {
                await Task.Delay(1000);
            }
        }

        throw new Exception($"{attempts} attempts exceeded!");
    }

    private static async Task CheckUntilUrlAvailable(HttpClient httpClient, string url, int attempts = 5)
    {
        await CheckUntil(async () =>
        {
            try
            {
                var uiResult = await httpClient.GetAsync(url);
                if (uiResult.StatusCode == System.Net.HttpStatusCode.Found)
                {
                    // Why is 302 not succes?!
                    return true;
                }
                uiResult.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }, attempts);
    }
}