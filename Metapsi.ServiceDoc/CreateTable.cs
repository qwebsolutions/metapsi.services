using Dapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace Metapsi;

public static partial class ServiceDoc
{
    private class PragmaXTableRow
    {
        public string name { get; set; }
    }

    public static string GetTableName(Type t)
    {
        return t.CSharpTypeName(TypeQualifier.Root).Replace(".", "_").Replace("`", "_").Replace("<", "_").Replace(">", "_").Replace("+", "_");
    }
    
    private static async Task CreateDocumentTableAsync<T>(
        Metapsi.Sqlite.SqliteQueue sqliteQueue,
        string idProperty,
        List<string> indexProperties)
    {
        var tableName = GetTableName(typeof(T));

        var tableColumnDeclarations = new List<string>();
        tableColumnDeclarations.Add($"ID TEXT NOT NULL UNIQUE GENERATED ALWAYS AS (json_extract(json, '$.{idProperty}')) VIRTUAL");
        foreach (var indexProperty in indexProperties)
        {
            tableColumnDeclarations.Add($"{indexProperty} TEXT NOT NULL GENERATED ALWAYS AS (json_extract(json, '$.{indexProperty}')) VIRTUAL");
        }
        tableColumnDeclarations.Add("json TEXT");

        var createDocCommand = $"CREATE TABLE IF NOT EXISTS {tableName} ({string.Join(",", tableColumnDeclarations)});";

        await sqliteQueue.Enqueue(async (conn) => await conn.ExecuteAsync(createDocCommand));

        // If table already existed, it was not recreated. 
        // Check if all index columns are created, as some might have been added in the latest update

        var allColumns = await sqliteQueue.Enqueue(async (conn) => await conn.QueryAsync<PragmaXTableRow>($"PRAGMA table_xinfo({tableName});"));

        foreach (var indexProperty in indexProperties)
        {
            if (!allColumns.Any(x => x.name == indexProperty))
            {
                await sqliteQueue.Enqueue(async (conn) => await conn.ExecuteAsync($"ALTER TABLE {tableName} ADD COLUMN {indexProperty} TEXT NOT NULL GENERATED ALWAYS AS (json_extract(json, '$.{indexProperty}')) VIRTUAL"));
            }
        }

        // Create UNIQUE index on ID property
        await sqliteQueue.Enqueue(async (conn) => await conn.ExecuteAsync($"CREATE UNIQUE INDEX IF NOT EXISTS {tableName}_Id on {tableName}(Id);"));

        foreach (var indexProperty in indexProperties)
        {
            await sqliteQueue.Enqueue(async (conn) => await conn.ExecuteAsync($"CREATE INDEX IF NOT EXISTS {tableName}_{indexProperty} on {tableName}({indexProperty});"));
        }
    }
}
