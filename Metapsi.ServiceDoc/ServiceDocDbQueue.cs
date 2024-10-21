using Dapper;
using Metapsi.Sqlite;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Metapsi;

public static partial class ServiceDoc
{
    /// <summary>
    /// Exposes a small set of operations that are common to the frontend, REST API and your code.
    /// Allows custom behavior by method override
    /// </summary>
    public class DbQueue
    {
        public SqliteQueue SqliteQueue { get; set; }

        public DbQueue(SqliteQueue sqliteQueue)
        {
            this.SqliteQueue = sqliteQueue;
        }

        /// <summary>
        /// Insert or update document. The match is performed by the registered ID property
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="document"></param>
        /// <returns>Saved document</returns>
        public virtual Task<T> SaveDocument<T>(T document)
        {
            return this.SqliteQueue.SaveReturnDocument(document);
        }

        /// <summary>
        /// Deletes document by id. Will throw exception if multiple documents match
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TId"></typeparam>
        /// <param name="id"></param>
        /// <returns>Deleted document</returns>
        public virtual Task<T> DeleteDocument<T, TId>(TId id)
        {
            return this.SqliteQueue.DeleteReturnDocument<T, TId>(id);
        }

        /// <summary>
        /// Deletes all documents having <paramref name="byIndexProperty"/> equal to <paramref name="value"/>. <paramref name="byIndexProperty"/> must be indexed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="byIndexProperty"></param>
        /// <param name="value"></param>
        /// <returns>List of deleted documents</returns>
        public virtual Task<List<T>> DeleteDocuments<T, TProp>(System.Linq.Expressions.Expression<Func<T, TProp>> byIndexProperty, TProp value)
        {
            return this.SqliteQueue.DeleteReturnDocuments(byIndexProperty, value);
        }

        /// <summary>
        /// Get document of type T where <paramref name="id"/> matches the registered ID property
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TId"></typeparam>
        /// <param name="connection"></param>
        /// <param name="id"></param>
        /// <returns>Document, if id matches. Otherwise null</returns>
        public virtual Task<T> GetDocument<T, TId>(TId id)
        {
            return this.SqliteQueue.Read(async t => await t.GetDocument<T, TId>(id));
        }

        /// <summary>
        /// Get all documents of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>List of all documents of type T</returns>
        public virtual Task<List<T>> ListDocuments<T>()
        {
            return this.SqliteQueue.Read(async t => await t.ListDocuments<T>());
        }
    }
}
