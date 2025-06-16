using Microsoft.AspNetCore.Builder;
using System.Threading.Tasks;
using Metapsi;
using Metapsi.Sqlite;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using Dapper;
using System.Data.SQLite;
using Metapsi.Html;
using System.Collections.Generic;

public static class Program
{
    public enum TestEnum
    {
        Value1,
        Value2
    }

    public class TestEntity
    {
        public class NestedData
        {
            public Guid GuidProperty { get; set; }
        }

        public bool BoolProperty { get; set; }

        public int Id { get; set; }
        public string StringProperty { get; set; }
        public NestedData Data { get; set; } = new NestedData();
        public TestEnum TestEnum { get; set; }
    }

    public class TestEntityStringKey
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public bool Enabled { get; set; }
    }

    public static async Task Main()
    {
        await RunServiceDocTest();
    }

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


    public static async Task ServiceDocNotWorkingTest()
    {
        var webApp = WebApplication.CreateBuilder().AddMetapsi().Build().UseMetapsi();
        webApp.Urls.Add("http://localhost:5000");

        string dbPath =
    System.IO.Path.Combine(
        RelativePath.SearchUpfolder(RelativePath.From.EntryPath, System.IO.Path.Combine("Metapsi.Tests")),
        "TestData", "test.db");

        // Create db access queue
        var dbQueue = new ServiceDoc.DbQueue(new SqliteQueue(dbPath));
        await webApp.MapGroup("docs").UseDocs(
            dbQueue,
            b =>
            {
                // Register documents with their unique key property
                b.AddDoc<User>(x => x.Id);
                b.AddDoc<TestEntity>(x => x.Id);
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

        string extraDbPath =
            System.IO.Path.Combine(
                RelativePath.SearchUpfolder(RelativePath.From.EntryPath, System.IO.Path.Combine("Metapsi.Tests")),
                "TestData", "extra.db");

        var extraDbQueue = new ServiceDoc.DbQueue(new SqliteQueue(extraDbPath));

        await webApp.MapGroup("extra").UseDocs(
            extraDbQueue,
            b =>
            {
                b.AddDoc<ExtraClass>(x => x.Id);
            });

        await webApp.RunAsync();
    }

    public class ExtraClass
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string SomeExtraData { get; set; }
    }

    public static async Task RunServiceDocTest()
    {

        await ServiceDocNotWorkingTest();

        Environment.Exit(0);


        var builder = WebApplication.CreateBuilder().AddMetapsi();
        var app = builder.Build().UseMetapsi();
        string dbPath =
            System.IO.Path.Combine(
                RelativePath.SearchUpfolder(RelativePath.From.EntryPath, System.IO.Path.Combine("Metapsi.Tests")),
                "TestData", "test.db");
        ServiceDoc.DbQueue dbQueue = new ServiceDoc.DbQueue(new SqliteQueue(dbPath));
        await app.MapGroup("config").UseDocs(
            dbQueue,
            b =>
            {
                b.SetOverviewUrl("all");
                b.AddDoc<TestEntity>(
                    x => x.Id,
                    b =>
                    {
                        b.SetFrontendNew(async () =>
                        {
                            return new TestEntity()
                            {
                                Id = 1000,
                                StringProperty = "Created server-side"
                            };
                        });
                        b.SetFrontendList(async () =>
                        {
                            var docs = await dbQueue.SqliteQueue.ListDocuments<TestEntity>();
                            return docs.OrderByDescending(x => x.Id).ToList();
                        });

                        b.SetFrontendDelete(async (TestEntity t) =>
                        {
                            return new ServiceDoc.DeleteResult()
                            {
                                Message = "Cannot delete"
                            };
                        });
                    });
                b.AddDoc<TestEntityStringKey>(
                    x => x.Key,
                    b =>
                    {
                        b.AddIndex(x => x.Enabled);
                    });
            });
        app.Urls.Add("http://localhost:5000");
        app.RunAsync();


        //Task.Run(async () =>
        //{
        //    await dbQueue.SqliteQueue.Enqueue(async c => await c.ExecuteAsync($"delete from {ServiceDoc.GetTableName(typeof(TestEntity))}"));
        //    int count = 1000;
        //    var sw = System.Diagnostics.Stopwatch.StartNew();
        //    for (int i = 0; i < count; i++)
        //    {
        //        //new SQLiteConnectionStringBuilder($"Max Pool Size=25;Pooling=true")
        //        //{
        //        //    DataSource = ":memory:",
        //        //    FailIfMissing = true, // This setting causes SQLiteConnection.PoolCount not to return its expected value.
        //        //    Pooling = true,
        //        //    ToFullPath = false,
        //        //};
        //        {
        //            var connectionString = Db.ToConnection(dbPath + ";Max Pool Size=3;Pooling=True");

        //            using (var newConnection = new SQLiteConnection(connectionString))
        //            {
        //                await newConnection.OpenAsync();

        //                await newConnection.SaveDocument(new TestEntity()
        //                {
        //                    Data = new TestEntity.NestedData()
        //                    {
        //                        GuidProperty = Guid.NewGuid()
        //                    },
        //                    Id = i,
        //                    StringProperty = "string_value_" + i
        //                });
        //            }
        //        }
        //    }

        //    sw.Stop();
        //    Console.WriteLine($"{count} entities with dbQueue.SaveDocument {sw.ElapsedMilliseconds} ms");
        //    Console.WriteLine($"OpenCount {SQLiteConnection.OpenCount}");
        //    Console.WriteLine($"CreateCount {SQLiteConnection.CreateCount}");
        //});

        //Task.Run(async () =>
        //{
        //    while (true)
        //    {
        //        var plm = await dbQueue.SqliteQueue.Read(async c =>
        //        {
        //            return await c.ListDocuments<TestEntity>();
        //        });

        //        Console.WriteLine(plm.Count);
        //    }
        //});

        //Task.Run(async () =>
        //{
        //    await dbQueue.SqliteQueue.Enqueue(async c => await c.ExecuteAsync($"delete from {ServiceDoc.GetTableName(typeof(TestEntity))}"));
        //    int count = 1000;
        //    var sw = System.Diagnostics.Stopwatch.StartNew();
        //    for (int i = 0; i < count; i++)
        //    {
        //        await dbQueue.SaveDocument(new TestEntity()
        //        {
        //            Data = new TestEntity.NestedData()
        //            {
        //                GuidProperty = Guid.NewGuid()
        //            },
        //            Id = i,
        //            StringProperty = "string_value_" + i
        //        });
        //    }

        //    sw.Stop();
        //    Console.WriteLine($"{count} entities with dbQueue.SaveDocument {sw.ElapsedMilliseconds} ms");
        //});

        // 16427 ms

        await Task.Run(async () =>
        {
            await dbQueue.SqliteQueue.Enqueue(async c => await c.ExecuteAsync($"delete from {ServiceDoc.GetTableName(typeof(TestEntity))}"));
            int count = 1000;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            await dbQueue.SqliteQueue.WithCommit(async t =>
            {
                for (int i = 0; i < count; i++)
                {
                    var d = await t.SaveReturnDocument(new TestEntity()
                    {
                        Data = new TestEntity.NestedData()
                        {
                            GuidProperty = Guid.NewGuid()
                        },
                        Id = 2000 + i,
                        StringProperty = "string_value_" + i
                    });
                }

            });
            sw.Stop();
            Console.WriteLine($"{count} entities with SqliteTransaction.SaveDocument {sw.ElapsedMilliseconds} ms");
        });

        var del1 = await dbQueue.DeleteDocument<TestEntity>(2001);

        await dbQueue.SqliteQueue.DeleteDocument<TestEntity, int>(2002);

        //Task.Run(async () =>
        //{
        //    await dbQueue.SqliteQueue.Enqueue(async c => await c.ExecuteAsync($"delete from {ServiceDoc.GetTableName(typeof(TestEntity))}"));
        //    int count = 1000;
        //    var sw = System.Diagnostics.Stopwatch.StartNew();
        //    await dbQueue.SqliteQueue.WithCommit(async t =>
        //    {

        //        t.Connection.CreateCommand();
        //        for (int i = 0; i < count; i++)
        //        {

        //            await t.SaveDocument(new TestEntity()
        //            {
        //                Data = new TestEntity.NestedData()
        //                {
        //                    GuidProperty = Guid.NewGuid()
        //                },
        //                Id = 2000 + i,
        //                StringProperty = "string_value_" + i
        //            });
        //        }

        //    });
        //    sw.Stop();
        //    Console.WriteLine($"{count} entities with SqliteTransaction.SaveDocument {sw.ElapsedMilliseconds} ms");
        //});

        await Task.Delay(TimeSpan.FromMinutes(10));

        Environment.Exit(0);

        Task.Run(async () =>
        {
            await dbQueue.SqliteQueue.Enqueue(async c => await c.ExecuteAsync($"delete from {ServiceDoc.GetTableName(typeof(TestEntity))}"));
            int count = 1000;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < count; i++)
            {
                await dbQueue.SqliteQueue.SaveDocument(new TestEntity()
                {
                    Data = new TestEntity.NestedData()
                    {
                        GuidProperty = Guid.NewGuid()
                    },
                    Id = i,
                    StringProperty = "string_value_" + i
                });
            }

            sw.Stop();
            Console.WriteLine($"{count} entities with dbQueue.SqliteQueue.SaveDocument {sw.ElapsedMilliseconds} ms");
        });

        Task.Run(async () =>
        {
            await dbQueue.SqliteQueue.Enqueue(async c => await c.ExecuteAsync($"delete from {ServiceDoc.GetTableName(typeof(TestEntity))}"));
            int count = 1000;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < count; i++)
            {
                await dbQueue.SqliteQueue.InsertDocument(new TestEntity()
                {
                    Data = new TestEntity.NestedData()
                    {
                        GuidProperty = Guid.NewGuid()
                    },
                    Id = 1000 + i,
                    StringProperty = "string_value_" + i
                });
            }

            sw.Stop();
            Console.WriteLine($"{count} entities with dbQueue.SqliteQueue.InsertDocument {sw.ElapsedMilliseconds} ms");
        });



        Task.Run(async () =>
        {
            await dbQueue.SqliteQueue.Enqueue(async c => await c.ExecuteAsync($"delete from {ServiceDoc.GetTableName(typeof(TestEntity))}"));
            int count = 1000;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            await dbQueue.SqliteQueue.WithCommit(async t =>
            {
                for (int i = 0; i < count; i++)
                {
                    await t.InsertDocument(new TestEntity()
                    {
                        Data = new TestEntity.NestedData()
                        {
                            GuidProperty = Guid.NewGuid()
                        },
                        Id = 3000 + i,
                        StringProperty = "string_value_" + i
                    });
                }
                await Task.Delay(30000);
            });
            sw.Stop();
            Console.WriteLine($"{count} entities with SqliteTransaction.InsertDocument {sw.ElapsedMilliseconds} ms");
        });

        await Task.Delay(TimeSpan.FromMinutes(15));
    }
}