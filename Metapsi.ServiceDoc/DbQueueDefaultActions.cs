using Dapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Metapsi
{
    public static partial class ServiceDoc
    {
        internal class DbQueueDefaultActions : IDbDefaultActions
        {
            private readonly DbQueue dbQueue;

            public DbQueueDefaultActions(DbQueue dbQueue)
            {
                this.dbQueue = dbQueue;
            }

            Func<TableMetadata, Task> defaultMigrate { get; set; }
            Func<Type, Task<int>> defaultCount { get; set; }
            Action<Type> fillList { get; set; }

            //public void SetDefaultMigrate(Func<TableMetadata, Task> defaultMigrate)
            //{
            //    this.defaultMigrate = defaultMigrate;
            //}

            public System.Func<Task> GetDefaultMigrate(TableMetadata tableMetadata)
            {
                return async () => await CreateDocumentTableAsync(dbQueue.SqliteQueue, tableMetadata);
            }

            public System.Func<Task<int>> GetDefaultCount<T>()
            {
                return async () =>
                {
                    var count = await dbQueue.SqliteQueue.Enqueue(async (c) =>
                    {
                        return await c.ExecuteScalarAsync<int>($"select count(1) from {GetTableName(typeof(T))}");
                    });
                    return count;
                };
            }

            public Func<Task<List<T>>> GetDefaultList<T>()
            {
                return async () =>
                {
                    return await dbQueue.ListDocuments<T>();
                };
            }

            public Func<T, Task<SaveResult>> GetDefaultSave<T>()
            {
                return async (T doc) =>
                {
                    await dbQueue.SaveDocument(doc);
                    return new SaveResult()
                    {
                        Message = "Document saved",
                        Success = true
                    };
                };
            }


            public Func<T, Task<DeleteResult>> GetDefaultDelete<T,TId>(System.Linq.Expressions.Expression<Func<T,TId>> getId)
            {
                return async (T doc) =>
                {
                    await dbQueue.DeleteDocument<T, TId>(getId.Compile()(doc));
                    return new DeleteResult()
                    {
                        Message = "Document deleted",
                        Success = true
                    };
                };
            }
        }
    }
}
