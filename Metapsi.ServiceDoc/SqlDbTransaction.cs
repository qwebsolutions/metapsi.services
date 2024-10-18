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
        await new InsertDocumentCommand<T>(transaction.Connection).ExecuteAsync(document);
    }

    public static async Task<T> InsertReturnDocument<T>(this DbTransaction transaction, T document)
    {
        return await new InsertReturnDocumentCommand<T>(transaction.Connection).ExecuteAsync(document);
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
    public static async Task DeleteDocuments<T, TProp>(
        this DbTransaction transaction,
        System.Linq.Expressions.Expression<Func<T, TProp>> byProperty,
        TProp value)
    {
        await new DeleteDocumentsCommand<T, TProp>(transaction.Connection, byProperty.PropertyName()).ExecuteAsync(value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TProp"></typeparam>
    /// <param name="transaction"></param>
    /// <param name="byProperty"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static async Task<List<T>> DeleteReturnDocuments<T, TProp>(
        this DbTransaction transaction,
        System.Linq.Expressions.Expression<Func<T, TProp>> byProperty,
        TProp value)
    {
        var result = new DeleteReturnDocumentsCommand<T, TProp>(transaction.Connection, byProperty.PropertyName()).ExecuteAsync(value);
        return result.ToBlockingEnumerable().ToList();
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
        await new DeleteDocumentCommand<T, TId>(transaction.Connection).ExecuteAsync(id);
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
        return await new DeleteReturnDocumentCommand<T, TId>(transaction.Connection).ExecuteAsync(id);
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
        await new SaveDocumentCommand<T>(transaction.Connection).ExecuteAsync(document);
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
        return await new SaveReturnDocumentCommand<T>(transaction.Connection).ExecuteAsync(document);
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
        return await new GetDocumentCommand<T, TId>(transaction.Connection).ExecuteAsync(id);
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
        var result = new GetDocumentsCommand<T, TProp>(transaction.Connection, byIndexProperty.PropertyName()).ExecuteAsync(value);
        return result.ToBlockingEnumerable().ToList();
    }

    /// <summary>
    /// Get all documents of type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="transaction"></param>
    /// <returns>List of all documents of type T</returns>
    public static async Task<List<T>> ListDocuments<T>(this DbTransaction transaction)
    {
        var result = new ListDocumentsCommand<T>(transaction.Connection).ExecuteAsync();
        return result.ToBlockingEnumerable().ToList();
    }
}