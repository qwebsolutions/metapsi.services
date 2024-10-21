using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;

namespace Metapsi;

public static partial class ServiceDoc
{
    /// <summary>
    /// Inserts the document. Will throw exception if ID already exists
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="connection"></param>
    /// <param name="document"></param>
    /// <returns></returns>
    public static async Task InsertDocument<T>(this DbConnection connection, T document)
    {
        await new InsertDocumentCommand<T>(connection).ExecuteAsync(document);
    }

    public static async Task<T> InsertReturnDocument<T>(this DbConnection connection, T document)
    {
        return await new InsertReturnDocumentCommand<T>(connection).ExecuteAsync(document);
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
    public static async Task DeleteDocuments<T, TProp>(
        this DbConnection connection,
        System.Linq.Expressions.Expression<Func<T, TProp>> byProperty,
        TProp value)
    {
        await new DeleteDocumentsCommand<T, TProp>(connection, byProperty.PropertyName()).ExecuteAsync(value);
    }

    /// <summary>
    /// Deletes all documents having <paramref name="byProperty"/> equal to <paramref name="value"/>. <paramref name="byProperty"/> must be indexed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TProp"></typeparam>
    /// <param name="connection"></param>
    /// <param name="byProperty"></param>
    /// <param name="value"></param>
    /// <returns>List of deleted documents</returns>
    public static async Task<List<T>> DeleteReturnDocuments<T, TProp>(
        this DbConnection connection,
        System.Linq.Expressions.Expression<Func<T, TProp>> byProperty,
        TProp value)
    {
        var result = await new DeleteReturnDocumentsCommand<T, TProp>(connection, byProperty.PropertyName()).ExecuteAsync(value);
        return result;
    }

    // Single document cannot be deleted outside of a transaction

    /// <summary>
    /// Insert or update document. The match is performed by the registered ID property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="connection"></param>
    /// <param name="document"></param>
    /// <returns>Saved document</returns>
    public static async Task SaveDocument<T>(
        this System.Data.Common.DbConnection connection,
        T document)
    {
        await new SaveDocumentCommand<T>(connection).ExecuteAsync(document);
    }

    /// <summary>
    /// Insert or update document. The match is performed by the registered ID property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="connection"></param>
    /// <param name="document"></param>
    /// <returns>Saved document</returns>
    public static async Task<T> SaveReturnDocument<T>(
        this System.Data.Common.DbConnection connection,
        T document)
    {
        return await new SaveReturnDocumentCommand<T>(connection).ExecuteAsync(document);
    }

    /// <summary>
    /// Get document of type T where <paramref name="id"/> matches the registered ID property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TId"></typeparam>
    /// <param name="connection"></param>
    /// <param name="id"></param>
    /// <returns>Document, if id matches. Otherwise null</returns>
    public static async Task<T> GetDocument<T, TId>(this System.Data.Common.DbConnection connection, TId id)
    {
        return await new GetDocumentCommand<T, TId>(connection).ExecuteAsync(id);
    }

    /// <summary>
    /// Get all documents where <paramref name="byIndexProperty"/> matches <paramref name="value"/>. <paramref name="byIndexProperty"/> must be indexed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TProp"></typeparam>
    /// <param name="connection"></param>
    /// <param name="byIndexProperty"></param>
    /// <param name="value"></param>
    /// <returns>List of matching documents</returns>
    public static async Task<List<T>> GetDocuments<T, TProp>(this DbConnection connection, System.Linq.Expressions.Expression<Func<T, TProp>> byIndexProperty, TProp value)
    {
        var result = await new GetDocumentsCommand<T, TProp>(connection, byIndexProperty.PropertyName()).ExecuteAsync(value);
        return result;
    }

    /// <summary>
    /// Get all documents of type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="connection"></param>
    /// <returns>List of all documents of type T</returns>
    public static async Task<List<T>> ListDocuments<T>(this DbConnection connection)
    {
        var result = await new ListDocumentsCommand<T>(connection).ExecuteAsync();
        return result;
    }
}
