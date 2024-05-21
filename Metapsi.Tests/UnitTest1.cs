using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace Metapsi.ActiveTable.Tests;

[DocDescription("Description here")]
public class TestEntity
{
    [DocIndex]
    public string Key { get; set; }
    public string Notes { get; set; }
    public string Created { get; set; } = string.Empty;
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

        var appBuilder = WebApplication.CreateBuilder().AddMetapsiWebServices(serviceSetup.ApplicationSetup, ig);
        var app = appBuilder.Build().UseMetapsi(serviceSetup.ApplicationSetup);

        var configurationUrl = await serviceSetup.ApplicationSetup.RegisterDocsUi<TestEntity>(
            ig,
            app,
            configurationDb,
            (x) => x.Key,
            createDocument: async (cc) => new TestEntity()
            {
                Key = Guid.NewGuid().ToString(),
                Notes = "Manually edited in " + serviceSetup.ServiceName
            });

        app.MapGet("/", () => Results.Redirect(configurationUrl)).AllowAnonymous().ExcludeFromDescription();
    }

    [TestMethod]
    public async Task StartChatService()
    {
        var serviceSetup = Mds.ServiceSetup.New();
        await SetupService(serviceSetup);

        var app = serviceSetup.Revive();
        await app.SuspendComplete;
    }
}