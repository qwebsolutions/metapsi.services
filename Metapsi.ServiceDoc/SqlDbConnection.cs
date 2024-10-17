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
        await connection.ChangeDocuments(cb =>
        {
            cb.InsertDocument(document);
        });
    }

    public static async Task<T> InsertReturnDocument<T>(this DbConnection connection, T document)
    {
        var documents = await connection.GetDocuments<T>(cb =>
        {
            cb.InsertReturnDocument(document);
        });

        return documents.SingleOrDefault();
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
    public static async Task<int> DeleteDocuments<T, TProp>(
        this DbConnection connection,
        System.Linq.Expressions.Expression<Func<T, TProp>> byProperty,
        TProp value)
    {
        return await connection.GetScalar<int>(cb =>
        {
            cb.DeleteDocuments(byProperty, value);
        });
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
        return await connection.GetDocuments<T>(cb =>
        {
            cb.DeleteReturnDocuments(byProperty, value);
        });
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
        await connection.ChangeDocuments(cb =>
        {
            cb.SaveDocument(document);
        });
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
        var results = await connection.GetDocuments<T>(cb =>
        {
            cb.SaveReturnDocument(document);
        });

        return results.SingleOrDefault();
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
        var results = await connection.GetDocuments<T>(cb =>
        {
            cb.GetDocuments<T, TId>("Id", id);
        });
        return results.SingleOrDefault();
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
        return await connection.GetDocuments<T>(cb =>
        {
            cb.GetDocuments(byIndexProperty, value);
        });
    }

    /// <summary>
    /// Get all documents of type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="connection"></param>
    /// <returns>List of all documents of type T</returns>
    public static async Task<List<T>> ListDocuments<T>(this DbConnection connection)
    {
        return await connection.GetDocuments<T>(cb=>
        {
            cb.ListDocuments<T>();
        });
    }
}
