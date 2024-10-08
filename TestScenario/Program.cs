﻿using Microsoft.AspNetCore.Builder;
using System.Threading.Tasks;
using Metapsi;
using Metapsi.Sqlite;
using System;
using System.Linq;
using System.Runtime.InteropServices;

public static class Program
{
    public class TestEntity
    {
        public class NestedData
        {
            public Guid GuidProperty { get; set; }
        }

        public int Id { get; set; }
        public string StringProperty { get; set; }
        public NestedData Data { get; set; } = new NestedData();
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
                    b=>
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
                    b=>
                    {
                        b.AddIndex(x => x.Enabled);
                    });
            });
        await app.RunAsync();
    }
}