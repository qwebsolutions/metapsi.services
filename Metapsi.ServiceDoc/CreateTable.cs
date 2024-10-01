using Dapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using Metapsi.Sqlite;

namespace Metapsi;

public static partial class ServiceDoc
{
    private class PragmaXTableRow
    {
        public string name { get; set; }
    }

    internal class IndexColumn
    {
        public string SourceProperty { get; set; }
        public Type CSharpType { get; set; }
    }

    public static string GetTableName(Type t)
    {
        return t.CSharpTypeName(TypeQualifier.Root).Replace(".", "_").Replace("`", "_").Replace("<", "_").Replace(">", "_").Replace("+", "_");
    }
    
    private static async Task CreateDocumentTableAsync<T>(
        SqliteQueue sqliteQueue,
        string tableName,
        IndexColumn idColumn,
        List<IndexColumn> indexColumns)
    {
        var tableColumnDeclarations = new List<string>();
        tableColumnDeclarations.Add($"ID {idColumn.CSharpType.GetSqliteTypeAffinity()} NOT NULL UNIQUE GENERATED ALWAYS AS (json_extract(json, '$.{idColumn.SourceProperty}')) VIRTUAL");
        foreach (var indexColumn in indexColumns)
        {
            tableColumnDeclarations.Add($"{indexColumn.SourceProperty} {indexColumn.CSharpType.GetSqliteTypeAffinity()} NOT NULL GENERATED ALWAYS AS (json_extract(json, '$.{indexColumn.SourceProperty}')) VIRTUAL");
        }
        tableColumnDeclarations.Add("json TEXT");

        var createDocCommand = $"CREATE TABLE IF NOT EXISTS {tableName} ({string.Join(",", tableColumnDeclarations)});";

        await sqliteQueue.Enqueue(async (conn) => await conn.ExecuteAsync(createDocCommand));

        // If table already existed, it was not recreated. 
        // Check if all index columns are created, as some might have been added in the latest update

        var allColumns = await sqliteQueue.Enqueue(async (conn) => await conn.QueryAsync<PragmaXTableRow>($"PRAGMA table_xinfo({tableName});"));

        if (!allColumns.Any(x => x.name.ToUpper() == "ID"))
        {
            await sqliteQueue.Enqueue(async (conn) => await conn.ExecuteAsync($"ALTER TABLE {tableName} ADD COLUMN Id {idColumn.CSharpType.GetSqliteTypeAffinity()} NOT NULL GENERATED ALWAYS AS (json_extract(json, '$.{idColumn.SourceProperty}')) VIRTUAL"));
        }

        foreach (var indexColumn in indexColumns)
        {
            if (!allColumns.Any(x => x.name.ToUpper() == indexColumn.SourceProperty.ToUpper()))
            {
                await sqliteQueue.Enqueue(async (conn) => await conn.ExecuteAsync($"ALTER TABLE {tableName} ADD COLUMN {indexColumn.SourceProperty} {indexColumn.CSharpType.GetSqliteTypeAffinity()} NOT NULL GENERATED ALWAYS AS (json_extract(json, '$.{indexColumn.SourceProperty}')) VIRTUAL"));
            }
        }

        // Create UNIQUE index on ID property
        await sqliteQueue.Enqueue(async (conn) => await conn.ExecuteAsync($"CREATE UNIQUE INDEX IF NOT EXISTS {tableName}_Id on {tableName}(Id);"));

        foreach (var indexColumn in indexColumns)
        {
            await sqliteQueue.Enqueue(async (conn) => await conn.ExecuteAsync($"CREATE INDEX IF NOT EXISTS {tableName}_{indexColumn.SourceProperty} on {tableName}({indexColumn.SourceProperty});"));
        }
    }
}
