using Dapper;
using Metapsi.Sqlite;
using System;
using System.Collections.Generic;
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
    public static Task InsertDocument<T>(
        this SqliteQueue sqliteQueue, 
        T document)
    {
        return sqliteQueue.Enqueue(async c => await c.InsertDocument<T>(document));
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
        this SqliteQueue sqliteQueue,
        System.Linq.Expressions.Expression<Func<T, TProp>> byProperty,
        TProp value)
    {
        return sqliteQueue.Enqueue(async c => await c.DeleteDocuments(byProperty, value));
    }

    /// <summary>
    /// Insert or update document. The match is performed by the registered ID property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="connection"></param>
    /// <param name="document"></param>
    /// <returns></returns>
    public static Task SaveDocument<T>(
        this SqliteQueue sqliteQueue,
        T document)
    {
        return sqliteQueue.Enqueue(async c => await c.SaveDocument(document));
    }

    /// <summary>
    /// Get document of type T where <paramref name="id"/> matches the registered ID property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TId"></typeparam>
    /// <param name="connection"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Task<T> GetDocument<T, TId>(
        this SqliteQueue sqliteQueue,
        TId id)
    {
        return sqliteQueue.Enqueue(async c => await c.GetDocument<T, TId>(id));
    }

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
    /// <param name="connection"></param>
    /// <returns></returns>
    public static Task<List<T>> ListDocuments<T>(
        this SqliteQueue sqliteQueue)
    {
        return sqliteQueue.Enqueue(async c => await c.ListDocuments<T>());
    }
}
