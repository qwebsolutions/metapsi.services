# How to use Metapsi.ServiceDoc

```C#
using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Sqlite;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Metapsi.Dev;

public static class ServiceDocSample
{
    public class User
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class Model
    {
        public List<User> Users { get; set; } = new();
    }

    public static async Task Main()
    {
        var webApp = WebApplication.CreateBuilder().AddMetapsi().Build().UseMetapsi();
        
        // Create db access queue
        var dbQueue = new ServiceDoc.DbQueue(new SqliteQueue(System.IO.Path.GetTempFileName()));
        await webApp.UseDocs(
            dbQueue,
            b =>
            {
                // Register documents with their unique key property
                b.AddDoc<User>(x => x.Id);
            });

        webApp.MapGet("/", async () => Page.Result(
            new Model()
            {
                // Load all documents
                Users = await dbQueue.ListDocuments<User>()
            }));

        // Create sample user
        await dbQueue.SaveDocument(new User()
        {
            Name = "John Doe",
            Email = "john@doe.com"
        });

        webApp.UseRenderer<Model>(
            model =>
            HtmlBuilder.FromDefault(
                b =>
                {
                    b.HeadAppend(b.HtmlTitle("Users"));
                    // Tailwind Play CDN
                    b.AddScript("https://unpkg.com/@tailwindcss/browser@4");
                    b.BodyAppend(
                        b.HtmlA(
                            b =>
                            {
                                b.SetClass("p-8 underline text-blue-800 text-sm");
                                b.SetHref("/docs");
                            },
                            b.Text("Go to backend")));

                    if (model.Users.Any())
                    {
                        b.BodyAppend(
                            b.HtmlDiv(
                                b =>
                                {
                                    b.SetClass("flex flex-col gap-8 m-8");
                                },
                                model.Users.Select(x =>
                                {
                                    return b.HtmlDiv(
                                        b =>
                                        {
                                            b.SetClass("flex flex-row gap-16 p-4 items-baseline rounded border border-blue-200");
                                        },
                                        b.HtmlDiv(b.Text(x.Name)),
                                        b.HtmlDiv(b.Text(x.Email)));
                                }).ToArray()));
                    }
                    else
                    {
                        b.BodyAppend(
                            b.HtmlDiv(
                                b =>
                                {
                                    b.SetClass("p-8 text-gray-600");
                                },
                                b.Text("No users")));
                    }

                }).ToHtml());
        await webApp.RunAsync();
    }
}
```

You can now edit documents in the backend and refresh your page to see the results.
