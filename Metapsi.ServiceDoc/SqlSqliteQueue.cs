using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Metapsi.Sqlite;

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
    public static Task InsertDocument<T>(this SqliteQueue sqliteQueue, T document)
    {
        return sqliteQueue.Enqueue(async c => await c.InsertDocument<T>(document));
    }

    /// <summary>
    /// Inserts the document. Will throw exception if ID already exists
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sqliteQueue"></param>
    /// <param name="document"></param>
    /// <returns>Inserted document</returns>
    public static Task<T> InsertReturnDocument<T>(this SqliteQueue sqliteQueue, T document)
    {
        return sqliteQueue.Enqueue(async c => await c.InsertReturnDocument<T>(document));
    }

    /// <summary>
    /// Deletes all documents having <paramref name="byIndexProperty"/> equal to <paramref name="value"/>. <paramref name="byIndexProperty"/> must be indexed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TProp"></typeparam>
    /// <param name="connection"></param>
    /// <param name="byIndexProperty"></param>
    /// <param name="value"></param>
    /// <returns>Number of deleted documents</returns>
    public static Task DeleteDocuments<T, TProp>(this SqliteQueue sqliteQueue, System.Linq.Expressions.Expression<Func<T, TProp>> byIndexProperty, TProp value)
    {
        return sqliteQueue.Enqueue(async c => await c.DeleteDocuments(byIndexProperty, value));
    }

    /// <summary>
    /// Deletes all documents having <paramref name="byIndexProperty"/> equal to <paramref name="value"/>. <paramref name="byIndexProperty"/> must be indexed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TProp"></typeparam>
    /// <param name="sqliteQueue"></param>
    /// <param name="byIndexProperty"></param>
    /// <param name="value"></param>
    /// <returns>List of deleted documents</returns>
    public static Task<List<T>> DeleteReturnDocuments<T, TProp>(this SqliteQueue sqliteQueue, System.Linq.Expressions.Expression<Func<T, TProp>> byIndexProperty, TProp value)
    {
        return sqliteQueue.Enqueue(async c => await c.DeleteReturnDocuments(byIndexProperty, value));
    }

    /// <summary>
    /// Deletes document by id. Will throw exception if multiple documents match
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TId"></typeparam>
    /// <param name="sqliteQueue"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Task DeleteDocument<T, TId>(this SqliteQueue sqliteQueue, TId id)
    {
        return sqliteQueue.WithCommit(async t => await t.DeleteDocument<T, TId>(id));
    }

    /// <summary>
    /// Deletes document by id. Will throw exception if multiple documents match
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TId"></typeparam>
    /// <param name="id"></param>
    /// <returns>Deleted document</returns>
    public static Task<T> DeleteReturnDocument<T, TId>(this SqliteQueue sqliteQueue, TId id)
    {
        return sqliteQueue.WithCommit(async t => await t.DeleteReturnDocument<T, TId>(id));
    }

    /// <summary>
    /// Insert or update document. The match is performed by the registered ID property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="connection"></param>
    /// <param name="document"></param>
    /// <returns></returns>
    public static Task SaveDocument<T>(this SqliteQueue sqliteQueue, T document)
    {
        return sqliteQueue.Enqueue(async c => await c.SaveDocument(document));
    }

    /// <summary>
    /// Insert or update document. The match is performed by the registered ID property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sqliteQueue"></param>
    /// <param name="document"></param>
    /// <returns>Saved document</returns>
    public static Task<T> SaveReturnDocument<T>(this SqliteQueue sqliteQueue, T document)
    {
        return sqliteQueue.Enqueue(async c => await c.SaveReturnDocument(document));
    }

    /// <summary>
    /// Get document of type T where <paramref name="id"/> matches the registered ID property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TId"></typeparam>
    /// <param name="connection"></param>
    /// <param name="id"></param>
    /// <returns>Document, if id matches. Otherwise null</returns>
    public static Task<T> GetDocument<T, TId>(this SqliteQueue sqliteQueue, TId id)
    {
        return sqliteQueue.Enqueue(async c => await c.GetDocument<T, TId>(id));
    }

    /// <summary>
    /// Get all documents where <paramref name="byIndexProperty"/> matches <paramref name="value"/>. <paramref name="byIndexProperty"/> must be indexed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TProp"></typeparam>
    /// <param name="sqliteQueue"></param>
    /// <param name="byIndexProperty"></param>
    /// <param name="value"></param>
    /// <returns>List of matching documents</returns>
    public static Task<List<T>> GetDocuments<T, TProp>(
        this SqliteQueue sqliteQueue,
        System.Linq.Expressions.Expression<Func<T, TProp>> byIndexProperty,
        TProp value)
    {
        return sqliteQueue.Enqueue(async c => await c.GetDocuments(byIndexProperty, value));
    }

    /// <summary>
    /// Get all documents of type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>List of all documents of type T</returns>
    public static Task<List<T>> ListDocuments<T>(this SqliteQueue sqliteQueue)
    {
        return sqliteQueue.Enqueue(async c => await c.ListDocuments<T>());
    }
}
