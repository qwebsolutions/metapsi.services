using Microsoft.AspNetCore.Builder;
using System.Threading.Tasks;
using Metapsi;
using Metapsi.Sqlite;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using Dapper;
using System.Data.SQLite;

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

    public static async Task RunServiceDocTest()
    {
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