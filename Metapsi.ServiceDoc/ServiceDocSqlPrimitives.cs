using Metapsi.Sqlite;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace Metapsi;

public static partial class ServiceDoc
{
    // String

    public static Task DeleteDocument<T>(this DbTransaction transaction, string id)
    {
        return transaction.DeleteDocument<T, string>(id);
    }

    public static Task<T> DeleteReturnDocument<T>(this DbTransaction transaction, string id)
    {
        return transaction.DeleteReturnDocument<T, string>(id);
    }

    public static Task<T> GetDocument<T>(this System.Data.Common.DbConnection connection, string id)
    {
        return connection.GetDocument<T, string>(id);
    }

    public static Task<T> GetDocument<T>(this System.Data.Common.DbTransaction transaction, string id)
    {
        return transaction.GetDocument<T, string>(id);
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
    public static Task<int> DeleteDocuments<T>(
        this SqliteQueue sqliteQueue,
        System.Linq.Expressions.Expression<Func<T, string>> byProperty,
        string value)
    {
        return sqliteQueue.DeleteDocuments<T, string>(byProperty, value);
    }

    /// <summary>
    /// Get document of type T where <paramref name="id"/> matches the registered ID property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TId"></typeparam>
    /// <param name="connection"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Task<T> GetDocument<T>(this SqliteQueue sqliteQueue,  string id)
    {
        return sqliteQueue.GetDocument<T, string>(id);
    }

    public static Task<List<T>> GetDocuments<T>(
        this SqliteQueue sqliteQueue,
        System.Linq.Expressions.Expression<Func<T, string>> byIndexProperty,
        string value)
    {
        return sqliteQueue.GetDocuments<T, string>(byIndexProperty, value);
    }


    // Int

    public static Task DeleteDocument<T>(this DbTransaction transaction, int id)
    {
        return transaction.DeleteDocument<T, int>(id);
    }

    public static Task<T> DeleteReturnDocument<T>(this DbTransaction transaction, int id)
    {
        return transaction.DeleteReturnDocument<T, int>(id);
    }

    public static Task<T> GetDocument<T>(this System.Data.Common.DbConnection connection, int id)
    {
        return connection.GetDocument<T, int>(id);
    }

    public static Task<T> GetDocument<T>(this System.Data.Common.DbTransaction transaction, int id)
    {
        return transaction.GetDocument<T, int>(id);
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
    public static Task<int> DeleteDocuments<T>(
        this SqliteQueue sqliteQueue,
        System.Linq.Expressions.Expression<Func<T, int>> byProperty,
        int value)
    {
        return sqliteQueue.DeleteDocuments<T, int>(byProperty, value);
    }

    /// <summary>
    /// Get document of type T where <paramref name="id"/> matches the registered ID property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TId"></typeparam>
    /// <param name="connection"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Task<T> GetDocument<T>(this SqliteQueue sqliteQueue, int id)
    {
        return sqliteQueue.GetDocument<T, int>(id);
    }

    public static Task<List<T>> GetDocuments<T>(
        this SqliteQueue sqliteQueue,
        System.Linq.Expressions.Expression<Func<T, int>> byIndexProperty,
        int value)
    {
        return sqliteQueue.GetDocuments<T, int>(byIndexProperty, value);
    }

    // Guid

    public static Task DeleteDocument<T>(this DbTransaction transaction, Guid id)
    {
        return transaction.DeleteDocument<T, Guid>(id);
    }

    public static Task<T> DeleteReturnDocument<T>(this DbTransaction transaction, Guid id)
    {
        return transaction.DeleteReturnDocument<T, Guid>(id);
    }

    public static Task<T> GetDocument<T>(this System.Data.Common.DbConnection connection, Guid id)
    {
        return connection.GetDocument<T, Guid>(id);
    }

    public static Task<T> GetDocument<T>(this System.Data.Common.DbTransaction transaction, Guid id)
    {
        return transaction.GetDocument<T, Guid>(id);
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
    public static Task<int> DeleteDocuments<T>(
        this SqliteQueue sqliteQueue,
        System.Linq.Expressions.Expression<Func<T, Guid>> byProperty,
        Guid value)
    {
        return sqliteQueue.DeleteDocuments<T, Guid>(byProperty, value);
    }

    /// <summary>
    /// Get document of type T where <paramref name="id"/> matches the registered ID property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TId"></typeparam>
    /// <param name="connection"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Task<T> GetDocument<T>(this SqliteQueue sqliteQueue, Guid id)
    {
        return sqliteQueue.GetDocument<T, Guid>(id);
    }

    public static Task<List<T>> GetDocuments<T>(
        this SqliteQueue sqliteQueue,
        System.Linq.Expressions.Expression<Func<T, Guid>> byIndexProperty,
        Guid value)
    {
        return sqliteQueue.GetDocuments<T, Guid>(byIndexProperty, value);
    }
}
