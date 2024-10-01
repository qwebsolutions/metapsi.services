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
    public static Task<List<T>> GetDocuments<T>(
        this SqliteQueue sqliteQueue,
        System.Linq.Expressions.Expression<Func<T, string>> byIndexProperty,
        string value)
    {
        return sqliteQueue.GetDocuments<T, string>(byIndexProperty, value);
    }

    public static Task DeleteDocument<T>(
        this DbQueue dbQueue,
        string id)
    {
        return dbQueue.DeleteDocument<T, string>(id);
    }

    /// <summary>
    /// Get document of type T where <paramref name="id"/> matches the registered ID property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TId"></typeparam>
    /// <param name="connection"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Task<T> GetDocument<T>(this DbQueue dbQueue,  string id)
    {
        return dbQueue.GetDocument<T, string>(id);
    }

    // Int

    public static Task DeleteDocument<T>(this DbTransaction transaction, int id)
    {
        return transaction.DeleteDocument<T, int>(id);
    }

    public static Task DeleteDocument<T>(this DbQueue dbQueue, int id)
    {
        return dbQueue.DeleteDocument<T, int>(id);
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
    /// Get document of type T where <paramref name="id"/> matches the registered ID property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TId"></typeparam>
    /// <param name="connection"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Task<T> GetDocument<T>(this DbQueue dbQueue, int id)
    {
        return dbQueue.GetDocument<T, int>(id);
    }

    // Guid

    public static Task DeleteDocument<T>(this DbTransaction transaction, Guid id)
    {
        return transaction.DeleteDocument<T, Guid>(id);
    }

    public static Task DeleteDocument<T>(this DbQueue dbQueue, Guid id)
    {
        return dbQueue.DeleteDocument<T, Guid>(id);
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
    /// Get document of type T where <paramref name="id"/> matches the registered ID property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TId"></typeparam>
    /// <param name="connection"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Task<T> GetDocument<T>(this DbQueue dbQueue, Guid id)
    {
        return dbQueue.GetDocument<T, Guid>(id);
    }
}
