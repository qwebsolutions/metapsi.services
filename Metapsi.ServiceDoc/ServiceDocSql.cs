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
    /// <param name="connection"></param>
    /// <param name="document"></param>
    /// <returns></returns>
    public static Task InsertDocument<T>(this DbConnection connection, T document)
    {
        // Use .GetType() to account for inheritance
        var tableName = GetTableName(document.GetType());
        return InsertDocument(tableName, document, connection, null);
    }

    /// <summary>
    /// Inserts the document. Will throw exception if ID already exists
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="transaction"></param>
    /// <param name="document"></param>
    /// <returns></returns>
    public static Task InsertDocument<T>(this DbTransaction transaction, T document)
    {
        // Use .GetType() to account for inheritance
        var tableName = GetTableName(document.GetType());
        return InsertDocument(tableName, document, transaction.Connection, transaction);
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
        var deletedCount = await transaction.Connection.ExecuteAsync($"delete from {tableName} where {byProperty}=@value", new { value }, transaction);
        return deletedCount;
    }

    /// <summary>
    /// Deletes all documents having <paramref name="byProperty"/> equal to <paramref name="value"/>. <paramref name="byProperty"/> must be indexed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TProp"></typeparam>
    /// <param name="connection"></param>
    /// <param name="byProperty"></param>
    /// <param name="value"></param>
    /// <returns>Number of deleted documents</returns>
    public static Task<int> DeleteDocuments<T, TProp>(
        this DbConnection connection,
        System.Linq.Expressions.Expression<Func<T, TProp>> byProperty,
        TProp value)
    {
        // We don't have the actual document here, use typeof(T)
        var tableName = GetTableName(typeof(T));
        var byPropertyName = byProperty.PropertyName();
        return DeleteDocuments<T, TProp>(tableName, byPropertyName, value, connection, null);
    }

    /// <summary>
    /// Deletes all documents having <paramref name="byProperty"/> equal to <paramref name="value"/>. <paramref name="byProperty"/> must be indexed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TProp"></typeparam>
    /// <param name="transaction"></param>
    /// <param name="byProperty"></param>
    /// <param name="value"></param>
    /// <returns>Number of deleted documents</returns>
    public static Task<int> DeleteDocuments<T, TProp>(
        this DbTransaction transaction,
        System.Linq.Expressions.Expression<Func<T, TProp>> byProperty,
        TProp value)
    {
        // We don't have the actual document here, use typeof(T)
        var tableName = GetTableName(typeof(T));
        var byPropertyName = byProperty.PropertyName();
        return DeleteDocuments<T, TProp>(tableName, byPropertyName, value, transaction.Connection, transaction);
    }

    public static async Task DeleteDocument<T, TId>(
        this DbTransaction transaction,
        TId id)
    {
        var tableName = GetTableName(typeof(T));
        var count = await DeleteDocuments<T, TId>(tableName, "Id", id, transaction.Connection, transaction);
        if (count > 1)
        {
            throw new Exception($"{typeof(T).Name} id {id} matches multiple documents");
        }
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
        var deletedDocs = await transaction.Connection.QueryAsync<JustJson>($"delete from {tableName} where {byProperty}=@value returning json", new { value }, transaction);
        return deletedDocs.Select(x => Metapsi.Serialize.FromJson<T>(x.json)).ToList();
    }

    public static async Task<T> DeleteReturnDocument<T, TId>(
        this DbTransaction transaction,
        TId id)
    {
        var tableName = GetTableName(typeof(T));
        var documents = await DeleteReturnDocuments<T, TId>(tableName, "Id", id, transaction.Connection, transaction);
        if (documents.Count > 1)
        {
            throw new Exception($"{typeof(T).Name} id {id} matches multiple documents");
        }
        return documents.SingleOrDefault();
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
    /// <param name="connection"></param>
    /// <param name="document"></param>
    /// <returns></returns>
    public static Task SaveDocument<T>(
        this System.Data.Common.DbConnection connection,
        T document)
    {
        var tableName = GetTableName(typeof(T));
        return SaveDocument<T>(tableName, document, connection, null);
    }

    /// <summary>
    /// Insert or update document. The match is performed by the registered ID property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="transaction"></param>
    /// <param name="document"></param>
    /// <returns></returns>
    public static Task SaveDocument<T>(
        this System.Data.Common.DbTransaction transaction,
        T document)
    {
        var tableName = GetTableName(typeof(T));
        return SaveDocument<T>(tableName, document, transaction.Connection, transaction);
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
        var jsonRow = await transaction.Connection.QuerySingleOrDefaultAsync<JustJson>($"select json from {tableName} where Id = @id", new { id }, transaction);
        if (jsonRow != null)
        {
            return Metapsi.Serialize.FromJson<T>(jsonRow.json);
        }
        return default(T);
    }

    /// <summary>
    /// Get document of type T where <paramref name="id"/> matches the registered ID property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TId"></typeparam>
    /// <param name="connection"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Task<T> GetDocument<T, TId>(this System.Data.Common.DbConnection connection, TId id)
    {
        var tableName = GetTableName(typeof(T));
        return GetDocument<T, TId>(tableName, id, connection, null);
    }

    /// <summary>
    /// Gets document of type T where <paramref name="id"/> matches the registered ID property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TId"></typeparam>
    /// <param name="transaction"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Task<T> GetDocument<T, TId>(this System.Data.Common.DbTransaction transaction, TId id)
    {
        var tableName = GetTableName(typeof(T));
        return GetDocument<T, TId>(tableName, id, transaction.Connection, transaction);
    }

    /// <summary>
    /// Get all documents where <paramref name="byProperty"/> matches <paramref name="value"/>. <paramref name="byProperty"/> must be indexed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TProp"></typeparam>
    /// <param name="tableName"></param>
    /// <param name="prop"></param>
    /// <param name="connection"></param>
    /// <param name="transaction"></param>
    /// <returns></returns>
    public static async Task<List<T>> GetDocuments<T, TProp>(string tableName, string byProperty, TProp value, DbConnection connection, DbTransaction transaction)
    {
        var jsonDocuments = await transaction.Connection.QueryAsync<JustJson>($"select json from {tableName} where {byProperty} = @value", new { value }, transaction);
        return jsonDocuments.Select(x => Metapsi.Serialize.FromJson<T>(x.json)).ToList();
    }

    public static Task<List<T>> GetDocuments<T, TProp>(this DbConnection connection, System.Linq.Expressions.Expression<Func<T, TProp>> byIndexProperty, TProp value)
    {
        var tableName = GetTableName(typeof(T));
        var byPropertyName = byIndexProperty.PropertyName();
        return GetDocuments<T, TProp>(tableName, byPropertyName, value, connection, null);
    }

    public static Task<List<T>> GetDocuments<T, TProp>(this DbTransaction transaction, System.Linq.Expressions.Expression<Func<T, TProp>> byIndexProperty, TProp value)
    {
        var tableName = GetTableName(typeof(T));
        var byPropertyName = byIndexProperty.PropertyName();
        return GetDocuments<T, TProp>(tableName, byPropertyName, value, transaction.Connection, transaction);
    }

    /// <summary>
    /// Get all documents from table <paramref name="tableName"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tableName"></param>
    /// <param name="connection"></param>
    /// <param name="transaction"></param>
    /// <returns></returns>
    public static async Task<List<T>> ListDocuments<T>(string tableName, DbConnection connection, DbTransaction transaction)
    {
        var jsonDocuments = await connection.QueryAsync<JustJson>($"select json from {tableName}", transaction: transaction);
        return jsonDocuments.Select(x => Metapsi.Serialize.FromJson<T>(x.json)).ToList();
    }

    /// <summary>
    /// Get all documents of type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="connection"></param>
    /// <returns></returns>
    public static Task<List<T>> ListDocuments<T>(this DbConnection connection)
    {
        var tableName = GetTableName(typeof(T));
        return ListDocuments<T>(tableName, connection, null);
    }

    /// <summary>
    /// Get all documents of type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="transaction"></param>
    /// <returns></returns>
    public static Task<List<T>> ListDocuments<T>(this DbTransaction transaction)
    {
        var tableName = GetTableName(typeof(T));
        return ListDocuments<T>(tableName, transaction.Connection, transaction);
    }
}
