using Dapper;

using System;
using System.Collections.Generic;
using System.Linq;
using Metapsi.Sqlite;
using System.Text;
using System.Threading.Tasks;

namespace Metapsi;

//public static partial class ServiceDoc
//{
    //public class NewDoc<T> : IData
    //{
    //    public T Doc { get; set; }
    //}

    //public class ChangedDoc<T> : IData
    //{
    //    public T Old { get; set; }
    //    public T New { get; set; }
    //}

    //public class DeletedDoc<T> : IData
    //{
    //    public T Doc { get; set; }
    //}

    //public class UnchangedDoc<T> : IData
    //{
    //    public T Doc { get; set; }
    //}

    //public class SaveResult<T>
    //{
    //    public NewDoc<T> New { get; set; }
    //    public ChangedDoc<T> Changed { get; set; }
    //    public UnchangedDoc<T> Unchanged { get; set; }
    //}

    //public class DeleteResult<T>
    //{
    //    public T Doc { get; set; }
    //}

    //private static async Task RegisterDocBackendApi<T>(
    //    this ApplicationSetup applicationSetup,
    //    ImplementationGroup ig,
    //    Metapsi.Sqlite.SqliteQueue sqliteQueue,
    //    string idProperty,
    //    List<string> indexProperties)
    //{
    //    await CreateDocumentTableAsync<T>(sqliteQueue, idProperty, indexProperties);

    //    var notARealState = applicationSetup.AddBusinessState(new object());

    //    applicationSetup.MapEvent<ApplicationRevived>(e =>

    //    e.Using(notARealState, ig).EnqueueCommand(
    //        async (CommandContext commandContext, object _) =>
    //        {
    //            await CreateDocumentTableAsync<T>(sqliteQueue, idProperty, indexProperties);
    //        }));

    //    ig.MapRequest(GetDocApi<T>().Save,
    //        async (RequestRoutingContext rc, T input) =>
    //        {
    //            var saveResult = await sqliteQueue.WithCommit(
    //                async (transaction) =>
    //                {
    //                    var current = await transaction.GetDocument(idProperty, id);
    //                    if (object.Equals(current, default(T)))
    //                    {
    //                        // The document did not exist before, it is new
    //                        await transaction.SaveDocument(input, idProperty);

    //                        return new SaveResult<T>() { New = new NewDoc<T>() { Doc = input } };
    //                    }
    //                    else
    //                    {
    //                        var areEqual = Metapsi.Serialize.ToJson(current) == Metapsi.Serialize.ToJson(input);

    //                        if (areEqual)
    //                        {
    //                            return new SaveResult<T> { Unchanged = new UnchangedDoc<T> { Doc = input } };
    //                        }
    //                        else
    //                        {
    //                            await transaction.SaveDocument(input, idProperty);
    //                            return new SaveResult<T> { Changed = new ChangedDoc<T>() { New = input, Old = current } };
    //                        }
    //                    }
    //                });

    //            await rc.Using(notARealState, ig).EnqueueCommand(async (CommandContext commandContext, object _) =>
    //            {
    //                if (saveResult.New != null)
    //                {
    //                    commandContext.PostEvent(saveResult.New);
    //                }
    //                if (saveResult.Changed != null)
    //                {
    //                    commandContext.PostEvent(saveResult.Changed);
    //                }
    //                if (saveResult.Unchanged != null)
    //                {
    //                    commandContext.PostEvent(saveResult.Unchanged);
    //                }
    //            });

    //            return saveResult;
    //        });

    //    ig.MapRequest(GetDocApi<T>().List, async (rc) =>
    //    {
    //        if (!TableCreated<T>())
    //            return new List<T>();

    //        return await sqliteQueue.WithRollback(
    //            async (transaction) =>
    //            {
    //                return await transaction.GetDocuments<T>();
    //            });
    //    });

    //    ig.MapRequest(GetDocApi<T>().Get, async (rc, id) =>
    //    {
    //        if (!TableCreated<T>())
    //            return default(T);

    //        return await sqliteQueue.WithRollback(
    //            async (transaction) =>
    //            {
    //                return await transaction.GetDocument<T, string>(idProperty, id);
    //            });
    //    });

    //    ig.MapRequest(GetDocApi<T>().Delete, async (rc, id) =>
    //    {
    //        if (!TableCreated<T>())
    //        {
    //            return new DeleteResult<T>();
    //        }

    //        var deleteResult = await sqliteQueue.WithCommit(
    //            async (transaction) =>
    //            {
    //                var doc = await transaction.GetDocument(idProperty, id);
    //                await transaction.DeleteDocuments(idProperty, id);
    //                return new DeleteResult<T>() { Doc = doc };
    //            });

    //        await rc.Using(notARealState, ig).EnqueueCommand(async (CommandContext commandContext, object _) =>
    //        {
    //            commandContext.PostEvent(new DeletedDoc<T>() { Doc = deleteResult.Doc });
    //        });

    //        return deleteResult;
    //    });

    //    ig.MapRequest(GetDocApi<T>().Count, async (rc) =>
    //    {
    //        return await sqliteQueue.WithRollback(
    //            async (transaction) =>
    //            {
    //                return await transaction.Connection.ExecuteScalarAsync<int>($"select count (1) from {GetTableName(typeof(T))}");
    //            });
    //    });

    //    //ig.MapRequest(GetDocApi<T>().GetDbPath(), async (rc) =>
    //    //{
    //    //    return sqliteQueue.DbPath;
    //    //});
    //}

    
    
//}
