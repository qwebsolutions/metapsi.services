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
    public static async Task InsertDocument<T>(this DbTransaction transaction, T document)
    {
        await transaction.ChangeDocuments(cb =>
        {
            cb.InsertDocument(document);
        });
    }

    public static async Task<T> InsertReturnDocument<T>(this DbTransaction transaction, T document)
    {
        var results = await transaction.GetDocuments<T>(cb =>
        {
            cb.InsertReturnDocument<T>(document);
        });

        return results.SingleOrDefault();
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
    public static async Task<int> DeleteDocuments<T, TProp>(
        this DbTransaction transaction,
        System.Linq.Expressions.Expression<Func<T, TProp>> byProperty,
        TProp value)
    {
        return await transaction.GetScalar<int>(cb =>
        {
            cb.DeleteDocuments(byProperty, value);
        });
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
        var count = await transaction.GetScalar<int>(cb =>
        {
            cb.DeleteDocuments<T, TId>("Id", id);
        });
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
        var documents = await transaction.GetDocuments<T>(cb =>
        {
            cb.DeleteReturnDocuments<T, TId>("Id", id);
        });
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
    public static async Task SaveDocument<T>(
        this System.Data.Common.DbTransaction transaction,
        T document)
    {
        await transaction.ChangeDocuments(cb =>
        {
            cb.SaveDocument(document);
        });
    }

    /// <summary>
    /// Insert or update document. The match is performed by the registered ID property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="transaction"></param>
    /// <param name="document"></param>
    /// <returns>Saved document</returns>
    public static async Task<T> SaveReturnDocument<T>(
        this System.Data.Common.DbTransaction transaction,
        T document)
    {
        var documents = await transaction.GetDocuments<T>(cb =>
        {
            cb.SaveReturnDocument(document);
        });

        return documents.SingleOrDefault();
    }

    /// <summary>
    /// Gets document of type T where <paramref name="id"/> matches the registered ID property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TId"></typeparam>
    /// <param name="transaction"></param>
    /// <param name="id"></param>
    /// <returns>Document, if id matches. Otherwise null</returns>
    public static async Task<T> GetDocument<T, TId>(this System.Data.Common.DbTransaction transaction, TId id)
    {
        var documents = await transaction.GetDocuments<T>(cb =>
        {
            cb.GetDocuments<T, TId>("Id", id);
        });
        return documents.SingleOrDefault();
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
    public static async Task<List<T>> GetDocuments<T, TProp>(this DbTransaction transaction, System.Linq.Expressions.Expression<Func<T, TProp>> byIndexProperty, TProp value)
    {
        return await transaction.GetDocuments<T>(cb =>
        {
            cb.GetDocuments(byIndexProperty, value);
        });
    }

    /// <summary>
    /// Get all documents of type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="transaction"></param>
    /// <returns>List of all documents of type T</returns>
    public static async Task<List<T>> ListDocuments<T>(this DbTransaction transaction)
    {
        return await transaction.GetDocuments<T>(cb =>
        {
            cb.ListDocuments<T>();
        });
    }
}