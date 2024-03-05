using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Metapsi.ActiveTable.Tests;

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
        //var parameters = serviceSetup.ParametersAs<Parameters>();
        var ig = serviceSetup.ApplicationSetup.AddImplementationGroup();

        var webServerRefs = serviceSetup.ApplicationSetup.AddWebServer(ig, 5000);
        webServerRefs.RegisterStaticFiles(typeof(Metapsi.HtmlControls.Control).Assembly);

        var configurationDb = serviceSetup.GetServiceDataFile("test.db");
        var configurationUrl = await webServerRefs.RegisterDocsUi<TestEntity>(
            configurationDb,
            (x) => x.Key,
            createDocument: async (cc) => new TestEntity()
            {
                Notes = "Manually edited in " + serviceSetup.ServiceName
            });

        webServerRefs.WebApplication.MapGet("/", () => Results.Redirect(configurationUrl)).AllowAnonymous().ExcludeFromDescription();
    }
}