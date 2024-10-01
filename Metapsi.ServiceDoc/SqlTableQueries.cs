using Dapper;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;

namespace Metapsi;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class DocDescriptionAttribute : Attribute
{
    public DocDescriptionAttribute(string summary)
    {
    }

    public DocDescriptionAttribute(string summary, int order)
    {
    }
}

public static partial class ServiceDoc
{
    internal class JustJson
    {
        public string json { get; set; }
    }

    /// <summary>
    /// Inserts the document. Will throw exception if ID already exists
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="document"></param>
    /// <param name="connection"></param>
    /// <param name="transaction"></param>
    /// <returns></returns>
    public static async Task InsertDocument<T>(string tableName, T document, DbConnection connection, DbTransaction transaction)
    {
        await connection.ExecuteAsync($"insert into {tableName} (json) values(@json)", new { json = Metapsi.Serialize.ToJson(document) }, transaction);
    }

    /// <summary>
    /// Inserts the document. Will throw exception if ID already exists
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tableName"></param>
    /// <param name="document"></param>
    /// <param name="connection"></param>
    /// <param name="transaction"></param>
    /// <returns>Inserted document</returns>
    public static async Task<T> InsertReturnDocument<T>(string tableName, T document, DbConnection connection, DbTransaction transaction)
    {
        var r = await connection.QuerySingleAsync<JustJson>($"insert into {tableName} (json) values(@json) returning json", new { json = Metapsi.Serialize.ToJson(document) }, transaction);
        return Serialize.FromJson<T>(r.json);
    }

    /// <summary>
    /// Deletes all documents having <paramref name="byProperty"/> equal to <paramref name="value"/>. <paramref name="byProperty"/> must be indexed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TProp"></typeparam>
    /// <param name="byProperty"></param>
    /// <param name="value"></param>
    /// <param name="connection"></param>
    /// <param name="transaction"></param>
    /// <returns>Number of deleted documents</returns>
    public static async Task<int> DeleteDocuments<T, TProp>(
        string tableName,
        string byProperty,
        TProp value,
        DbConnection connection,
        DbTransaction transaction)
    {
        var deletedCount = await connection.ExecuteAsync($"delete from {tableName} where {byProperty}=@value", new { value }, transaction);
        return deletedCount;
    }

    /// <summary>
    /// Deletes all documents having <paramref name="byProperty"/> equal to <paramref name="value"/>. <paramref name="byProperty"/> must be indexed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TProp"></typeparam>
    /// <param name="tableName"></param>
    /// <param name="byProperty"></param>
    /// <param name="value"></param>
    /// <param name="connection"></param>
    /// <param name="transaction"></param>
    /// <returns>List of deleted documents</returns>
    public static async Task<List<T>> DeleteReturnDocument<T, TProp>(
        string tableName,
        string byProperty,
        TProp value,
        DbConnection connection,
        DbTransaction transaction)
    {
        var deletedDocuments = await connection.QueryAsync<JustJson>($"delete from {tableName} where {byProperty}=@value returning *", new { value }, transaction);
        return deletedDocuments.Select(x => Serialize.FromJson<T>(x.json)).ToList();
    }

    /// <summary>
    /// Deletes all documents having <paramref name="byProperty"/> equal to <paramref name="value"/>. <paramref name="byProperty"/> must be indexed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TProp"></typeparam>
    /// <param name="byProperty"></param>
    /// <param name="value"></param>
    /// <param name="connection"></param>
    /// <param name="transaction"></param>
    /// <returns>Number of deleted documents</returns>
    public static async Task<List<T>> DeleteReturnDocuments<T, TProp>(
        string tableName,
        string byProperty,
        TProp value,
        DbConnection connection,
        DbTransaction transaction)
    {
        var deletedDocs = await connection.QueryAsync<JustJson>($"delete from {tableName} where {byProperty}=@value returning json", new { value }, transaction);
        return deletedDocs.Select(x => Metapsi.Serialize.FromJson<T>(x.json)).ToList();
    }


    /// <summary>
    /// Insert or update document. The match is performed by the registered ID property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tableName"></param>
    /// <param name="document"></param>
    /// <param name="connection"></param>
    /// <param name="transaction"></param>
    /// <returns></returns>
    public static async Task SaveDocument<T>(string tableName, T document, DbConnection connection, DbTransaction transaction)
    {
        await connection.ExecuteAsync($"insert into {tableName} (json) values(@json) ON CONFLICT (Id) DO UPDATE SET json=@json", new { json = Metapsi.Serialize.ToJson(document) }, transaction);
    }

    /// <summary>
    /// Insert or update document. The match is performed by the registered ID property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tableName"></param>
    /// <param name="document"></param>
    /// <param name="connection"></param>
    /// <param name="transaction"></param>
    /// <returns>Saved document</returns>
    public static async Task<T> SaveReturnDocument<T>(string tableName, T document, DbConnection connection, DbTransaction transaction)
    {
        var r = await connection.QuerySingleAsync<JustJson>($"insert into {tableName} (json) values(@json) ON CONFLICT (Id) DO UPDATE SET json=@json returning json", new { json = Metapsi.Serialize.ToJson(document) }, transaction);
        return Serialize.FromJson<T>(r.json);
    }

    /// <summary>
    /// Get document of type T where <paramref name="id"/> matches the registered ID property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TId"></typeparam>
    /// <param name="tableName"></param>
    /// <param name="id"></param>
    /// <param name="connection"></param>
    /// <param name="transaction"></param>
    /// <returns></returns>
    public static async Task<T> GetDocument<T, TId>(string tableName, TId id, DbConnection connection, DbTransaction transaction)
    {
        var jsonRow = await connection.QuerySingleOrDefaultAsync<JustJson>($"select json from {tableName} where Id = @id", new { id }, transaction);
        if (jsonRow != null)
        {
            return Metapsi.Serialize.FromJson<T>(jsonRow.json);
        }
        return default(T);
    }

    /// <summary>
    /// Get all documents where <paramref name="byIndexProperty"/> matches <paramref name="value"/>. <paramref name="byIndexProperty"/> must be indexed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TProp"></typeparam>
    /// <param name="tableName"></param>
    /// <param name="prop"></param>
    /// <param name="connection"></param>
    /// <param name="transaction"></param>
    /// <returns>List of matching documents</returns>
    public static async Task<List<T>> GetDocuments<T, TProp>(string tableName, string byIndexProperty, TProp value, DbConnection connection, DbTransaction transaction)
    {
        var jsonDocuments = await connection.QueryAsync<JustJson>($"select json from {tableName} where {byIndexProperty} = @value", new { value }, transaction);
        return jsonDocuments.Select(x => Metapsi.Serialize.FromJson<T>(x.json)).ToList();
    }

    /// <summary>
    /// Get all documents from table <paramref name="tableName"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tableName"></param>
    /// <param name="connection"></param>
    /// <param name="transaction"></param>
    /// <returns>List of all documents of type T</returns>
    public static async Task<List<T>> ListDocuments<T>(string tableName, DbConnection connection, DbTransaction transaction)
    {
        var jsonDocuments = await connection.QueryAsync<JustJson>($"select json from {tableName}", transaction: transaction);
        return jsonDocuments.Select(x => Metapsi.Serialize.FromJson<T>(x.json)).ToList();
    }
}
