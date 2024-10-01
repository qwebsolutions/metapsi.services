using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Metapsi;

public static partial class ServiceDoc
{
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

    public static Task<T> InsertReturnDocument<T>(this DbTransaction transaction, T document)
    {
        // Use .GetType() to account for inheritance
        var tableName = GetTableName(document.GetType());
        return InsertReturnDocument(tableName, document, transaction.Connection, transaction);
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

    /// <summary>
    /// Deletes document by id. Will throw exception if multiple documents match
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TId"></typeparam>
    /// <param name="transaction"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
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
    /// Deletes document by id. Will throw exception if multiple documents match
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TId"></typeparam>
    /// <param name="transaction"></param>
    /// <param name="id"></param>
    /// <returns>Deleted document</returns>
    /// <exception cref="Exception"></exception>
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
    /// <param name="transaction"></param>
    /// <param name="document"></param>
    /// <returns>Saved document</returns>
    public static Task SaveDocument<T>(
        this System.Data.Common.DbTransaction transaction,
        T document)
    {
        var tableName = GetTableName(typeof(T));
        return SaveDocument<T>(tableName, document, transaction.Connection, transaction);
    }

    /// <summary>
    /// Insert or update document. The match is performed by the registered ID property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="transaction"></param>
    /// <param name="document"></param>
    /// <returns>Saved document</returns>
    public static Task<T> SaveReturnDocument<T>(
        this System.Data.Common.DbTransaction transaction,
        T document)
    {
        var tableName = GetTableName(typeof(T));
        return SaveReturnDocument<T>(tableName, document, transaction.Connection, transaction);
    }

    /// <summary>
    /// Gets document of type T where <paramref name="id"/> matches the registered ID property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TId"></typeparam>
    /// <param name="transaction"></param>
    /// <param name="id"></param>
    /// <returns>Document, if id matches. Otherwise null</returns>
    public static Task<T> GetDocument<T, TId>(this System.Data.Common.DbTransaction transaction, TId id)
    {
        var tableName = GetTableName(typeof(T));
        return GetDocument<T, TId>(tableName, id, transaction.Connection, transaction);
    }

    /// <summary>
    /// Get all documents where <paramref name="byIndexProperty"/> matches <paramref name="value"/>. <paramref name="byIndexProperty"/> must be indexed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TProp"></typeparam>
    /// <param name="transaction"></param>
    /// <param name="byIndexProperty"></param>
    /// <param name="value"></param>
    /// <returns>List of matching documents</returns>
    public static Task<List<T>> GetDocuments<T, TProp>(this DbTransaction transaction, System.Linq.Expressions.Expression<Func<T, TProp>> byIndexProperty, TProp value)
    {
        var tableName = GetTableName(typeof(T));
        var byPropertyName = byIndexProperty.PropertyName();
        return GetDocuments<T, TProp>(tableName, byPropertyName, value, transaction.Connection, transaction);
    }

    /// <summary>
    /// Get all documents of type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="transaction"></param>
    /// <returns>List of all documents of type T</returns>
    public static Task<List<T>> ListDocuments<T>(this DbTransaction transaction)
    {
        var tableName = GetTableName(typeof(T));
        return ListDocuments<T>(tableName, transaction.Connection, transaction);
    }
}