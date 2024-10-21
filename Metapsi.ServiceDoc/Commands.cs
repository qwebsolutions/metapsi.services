using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Transactions;

namespace Metapsi;

public static partial class ServiceDoc
{
    public class InsertDocumentCommand<T> : IDisposable, IAsyncDisposable
    {
        public DbCommand DbCommand { get; private set; }

        public InsertDocumentCommand(DbConnection dbConnection)
        {
            DbCommand = dbConnection.CreateCommand();
            DbCommand.CommandText = CommandBuilder.GetInsertStatement(typeof(T), "json");
            var p1 = DbCommand.CreateParameter();
            p1.ParameterName = "@json";
            DbCommand.Parameters.Add(p1);
        }

        public async Task ExecuteAsync(T document)
        {
            this.DbCommand.Parameters[0].Value = Metapsi.Serialize.ToJson(document);
            await this.DbCommand.ExecuteNonQueryAsync();
        }

        public ValueTask DisposeAsync()
        {
            return DbCommand.DisposeAsync();
        }

        public void Dispose()
        {
            DbCommand.Dispose();
        }
    }

    public class InsertReturnDocumentCommand<T> : IDisposable, IAsyncDisposable
    {
        public DbCommand DbCommand { get; private set; }

        public InsertReturnDocumentCommand(DbConnection dbConnection)
        {
            DbCommand = dbConnection.CreateCommand();
            DbCommand.CommandText = CommandBuilder.GetInsertReturnDocumentStatement(typeof(T), "json");
            var p1 = DbCommand.CreateParameter();
            p1.ParameterName = "@json";
            DbCommand.Parameters.Add(p1);
        }

        public async Task<T> ExecuteAsync(T document)
        {
            this.DbCommand.Parameters[0].Value = Metapsi.Serialize.ToJson(document);
            var result = await this.DbCommand.ExecuteScalarAsync();
            if(result is string)
                return Metapsi.Serialize.FromJson<T>(result as string);
            return default(T);
        }

        public ValueTask DisposeAsync()
        {
            return DbCommand.DisposeAsync();
        }

        public void Dispose()
        {
            DbCommand.Dispose();
        }
    }

    public class DeleteDocumentCommand<T, TId> : IDisposable, IAsyncDisposable
    {
        public DbCommand DbCommand { get; private set; }

        public DeleteDocumentCommand(DbConnection dbConnection)
        {
            DbCommand = dbConnection.CreateCommand();
            DbCommand.CommandText = CommandBuilder.GetDeleteDocumentsStatement(typeof(T), "Id", "id");
            var p1 = DbCommand.CreateParameter();
            p1.ParameterName = "@id";
            DbCommand.Parameters.Add(p1);
        }

        public async Task ExecuteAsync(TId id)
        {
            this.DbCommand.Parameters[0].Value = id;
            var count = await this.DbCommand.ExecuteNonQueryAsync();

            if (count > 1)
            {
                throw new Exception($"{typeof(T).Name} id {id} matches multiple documents");
            }
        }

        public ValueTask DisposeAsync()
        {
            return DbCommand.DisposeAsync();
        }

        public void Dispose()
        {
            DbCommand.Dispose();
        }
    }

    public class DeleteReturnDocumentCommand<T, TId> : IDisposable, IAsyncDisposable
    {
        public DbCommand DbCommand { get; private set; }

        public DeleteReturnDocumentCommand(DbConnection dbConnection)
        {
            DbCommand = dbConnection.CreateCommand();
            DbCommand.CommandText = CommandBuilder.GetDeleteReturnDocumentsStatement(typeof(T), "Id", "id");
            var p1 = DbCommand.CreateParameter();
            p1.ParameterName = "@id";
            DbCommand.Parameters.Add(p1);
        }

        public async Task<T> ExecuteAsync(TId id)
        {
            this.DbCommand.Parameters[0].Value = id;
            using var dbReader = await this.DbCommand.ExecuteReaderAsync();

            T result = default(T);

            while (await dbReader.ReadAsync())
            {
                if (!object.Equals(result, default(T)))
                {
                    // We 'iterated' more than once
                    throw new Exception($"{typeof(T).Name} id {id} matches multiple documents");
                }

                var json = dbReader.GetString(0);
                result = Metapsi.Serialize.FromJson<T>(json);
            }
            return result;
        }

        public ValueTask DisposeAsync()
        {
            return DbCommand.DisposeAsync();
        }

        public void Dispose()
        {
            DbCommand.Dispose();
        }
    }

    public class DeleteDocumentsCommand<T, TProp> : IDisposable, IAsyncDisposable
    {
        public DbCommand DbCommand { get; private set; }

        public DeleteDocumentsCommand(DbConnection dbConnection, string propertyName)
        {
            DbCommand = dbConnection.CreateCommand();
            DbCommand.CommandText = CommandBuilder.GetDeleteDocumentsStatement(typeof(T), propertyName, "value");
            var p1 = DbCommand.CreateParameter();
            p1.ParameterName = "@value";
            DbCommand.Parameters.Add(p1);
        }

        public async Task ExecuteAsync(TProp value)
        {
            this.DbCommand.Parameters[0].Value = value;
            await this.DbCommand.ExecuteNonQueryAsync();
        }

        public ValueTask DisposeAsync()
        {
            return DbCommand.DisposeAsync();
        }

        public void Dispose()
        {
            DbCommand.Dispose();
        }
    }

    public class DeleteReturnDocumentsCommand<T, TProp> : IDisposable, IAsyncDisposable
    {
        public DbCommand DbCommand { get; private set; }

        public DeleteReturnDocumentsCommand(DbConnection dbConnection, string propertyName)
        {
            DbCommand = dbConnection.CreateCommand();
            DbCommand.CommandText = CommandBuilder.GetDeleteReturnDocumentsStatement(typeof(T), propertyName, "value");
            var p1 = DbCommand.CreateParameter();
            p1.ParameterName = "@value";
            DbCommand.Parameters.Add(p1);
        }

        public async IAsyncEnumerable<T> IterateAsync(TProp value)
        {
            this.DbCommand.Parameters[0].Value = value;
            using var dbReader = await this.DbCommand.ExecuteReaderAsync();
            while (await dbReader.ReadAsync())
            {
                var json = dbReader.GetString(0);
                var document = Metapsi.Serialize.FromJson<T>(json);
                yield return document;
            }
        }

        public async Task<List<T>> ExecuteAsync(TProp value)
        {
            List<T> list = new List<T>();
            this.DbCommand.Parameters[0].Value = value;
            using var dbReader = await this.DbCommand.ExecuteReaderAsync();
            while (await dbReader.ReadAsync())
            {
                var json = dbReader.GetString(0);
                var document = Metapsi.Serialize.FromJson<T>(json);
                list.Add(document);
            }

            return list;
        }

        public ValueTask DisposeAsync()
        {
            return DbCommand.DisposeAsync();
        }

        public void Dispose()
        {
            DbCommand.Dispose();
        }
    }

    public class SaveDocumentCommand<T> : IDisposable, IAsyncDisposable
    {
        public DbCommand DbCommand { get; private set; }

        public SaveDocumentCommand(DbConnection dbConnection)
        {
            DbCommand = dbConnection.CreateCommand();
            DbCommand.CommandText = CommandBuilder.GetSaveDocumentStatement(typeof(T), "json");
            var p1 = DbCommand.CreateParameter();
            p1.ParameterName = "@json";
            DbCommand.Parameters.Add(p1);
        }

        public async Task ExecuteAsync(T document)
        {
            this.DbCommand.Parameters[0].Value = Metapsi.Serialize.ToJson(document);
            await this.DbCommand.ExecuteNonQueryAsync();
        }

        public ValueTask DisposeAsync()
        {
            return DbCommand.DisposeAsync();
        }

        public void Dispose()
        {
            DbCommand.Dispose();
        }
    }

    public class SaveReturnDocumentCommand<T> : IDisposable, IAsyncDisposable
    {
        public DbCommand DbCommand { get; private set; }

        public SaveReturnDocumentCommand(DbConnection dbConnection)
        {
            DbCommand = dbConnection.CreateCommand();
            DbCommand.CommandText = CommandBuilder.GetSaveReturnDocumentStatement(typeof(T), "json");
            var p1 = DbCommand.CreateParameter();
            p1.ParameterName = "@json";
            DbCommand.Parameters.Add(p1);
        }

        public async Task<T> ExecuteAsync(T document)
        {
            this.DbCommand.Parameters[0].Value = Metapsi.Serialize.ToJson(document);
            var result = (string)await this.DbCommand.ExecuteScalarAsync();
            return Metapsi.Serialize.FromJson<T>(result);
        }

        public ValueTask DisposeAsync()
        {
            return DbCommand.DisposeAsync();
        }

        public void Dispose()
        {
            DbCommand.Dispose();
        }
    }

    public class GetDocumentCommand<T, TId> : IDisposable, IAsyncDisposable
    {
        public DbCommand DbCommand { get; private set; }

        public GetDocumentCommand(DbConnection dbConnection)
        {
            DbCommand = dbConnection.CreateCommand();
            DbCommand.CommandText = CommandBuilder.GetSelectDocumentsStatement(typeof(T), "Id", "value");
            var p1 = DbCommand.CreateParameter();
            p1.ParameterName = "@value";
            DbCommand.Parameters.Add(p1);
        }

        public async Task<T> ExecuteAsync(TId id)
        {
            this.DbCommand.Parameters[0].Value = id;
            var result = await this.DbCommand.ExecuteScalarAsync();
            if (result == null)
                return default(T);
            return Metapsi.Serialize.FromJson<T>((string)result);
        }

        public ValueTask DisposeAsync()
        {
            return DbCommand.DisposeAsync();
        }

        public void Dispose()
        {
            DbCommand.Dispose();
        }
    }

    public class GetDocumentsCommand<T, TProp> : IDisposable, IAsyncDisposable
    {
        public DbCommand DbCommand { get; private set; }

        public GetDocumentsCommand(DbConnection dbConnection, string byIndexProperty)
        {
            DbCommand = dbConnection.CreateCommand();
            DbCommand.CommandText = CommandBuilder.GetSelectDocumentsStatement(typeof(T), byIndexProperty, "value");
            var p1 = DbCommand.CreateParameter();
            p1.ParameterName = "@value";
            DbCommand.Parameters.Add(p1);
        }

        public async IAsyncEnumerable<T> IterateAsync(TProp value)
        {
            this.DbCommand.Parameters[0].Value = value;
            using var dbReader = await this.DbCommand.ExecuteReaderAsync();
            while (await dbReader.ReadAsync())
            {
                var json = dbReader.GetString(0);
                var document = Metapsi.Serialize.FromJson<T>(json);
                yield return document;
            }
        }

        public async Task<List<T>> ExecuteAsync(TProp value)
        {
            List<T> list = new List<T>();
            this.DbCommand.Parameters[0].Value = value;
            using var dbReader = await this.DbCommand.ExecuteReaderAsync();
            while (await dbReader.ReadAsync())
            {
                var json = dbReader.GetString(0);
                var document = Metapsi.Serialize.FromJson<T>(json);
                list.Add(document);
            }

            return list;
        }

        public ValueTask DisposeAsync()
        {
            return DbCommand.DisposeAsync();
        }

        public void Dispose()
        {
            DbCommand.Dispose();
        }
    }

    public class ListDocumentsCommand<T> : IDisposable, IAsyncDisposable
    {
        public DbCommand DbCommand { get; private set; }

        public ListDocumentsCommand(DbConnection dbConnection)
        {
            DbCommand = dbConnection.CreateCommand();
            DbCommand.CommandText = CommandBuilder.GetListDocumentsStatement(typeof(T));
        }

        public async IAsyncEnumerable<T> IterateAsync()
        {
            using var dbReader = await this.DbCommand.ExecuteReaderAsync();
            while (await dbReader.ReadAsync())
            {
                var json = dbReader.GetString(0);
                var document = Metapsi.Serialize.FromJson<T>(json);
                yield return document;
            }
        }

        public async Task<List<T>> ExecuteAsync()
        {
            List<T> list = new List<T>();
            using var dbReader = await this.DbCommand.ExecuteReaderAsync();
            while (await dbReader.ReadAsync())
            {
                var json = dbReader.GetString(0);
                var document = Metapsi.Serialize.FromJson<T>(json);
                list.Add(document);
            }

            return list;
        }

        public ValueTask DisposeAsync()
        {
            return DbCommand.DisposeAsync();
        }

        public void Dispose()
        {
            DbCommand.Dispose();
        }
    }

    public static async Task<InsertDocumentCommand<T>> PrepareInsertDocument<T>(this DbConnection dbConnection)
    {
        var command = new InsertDocumentCommand<T>(dbConnection);
        await command.DbCommand.PrepareAsync();
        return command;
    }

    public static async Task<InsertDocumentCommand<T>> PrepareInsertDocument<T>(this DbTransaction dbTransaction)
    {
        var command = await PrepareInsertDocument<T>(dbTransaction.Connection);
        command.DbCommand.Transaction = dbTransaction;
        return command;
    }

    public static async Task<InsertReturnDocumentCommand<T>> PrepareInsertReturnDocument<T>(this DbConnection dbConnection)
    {
        var command = new InsertReturnDocumentCommand<T>(dbConnection);
        await command.DbCommand.PrepareAsync();
        return command;
    }

    public static async Task<InsertReturnDocumentCommand<T>> PrepareInsertReturnDocument<T>(this DbTransaction dbTransaction)
    {
        var command = await PrepareInsertReturnDocument<T>(dbTransaction.Connection);
        command.DbCommand.Transaction = dbTransaction;
        return command;
    }

    public static async Task<DeleteDocumentCommand<TDocument, TId>> PrepareDeleteDocument<TDocument, TId>(this DbConnection dbConnection)
    {
        var command = new DeleteDocumentCommand<TDocument, TId>(dbConnection);
        await command.DbCommand.PrepareAsync();
        return command;
    }

    public static async Task<DeleteDocumentCommand<TDocument, TId>> PrepareDeleteDocument<TDocument, TId>(this DbTransaction dbTransaction)
    {
        var command = await PrepareDeleteDocument<TDocument, TId>(dbTransaction.Connection);
        command.DbCommand.Transaction = dbTransaction;
        return command;
    }

    public static async Task<DeleteReturnDocumentCommand<TDocument, TId>> PrepareDeleteReturnDocument<TDocument, TId>(
        this DbConnection dbConnection)
    {
        var command = new DeleteReturnDocumentCommand<TDocument, TId>(dbConnection);
        await command.DbCommand.PrepareAsync();
        return command;
    }

    public static async Task<DeleteReturnDocumentCommand<TDocument, TId>> PrepareDeleteReturnDocument<TDocument, TId>(this DbTransaction dbTransaction)
    {
        var command = await PrepareDeleteReturnDocument<TDocument, TId>(dbTransaction.Connection);
        command.DbCommand.Transaction = dbTransaction;
        return command;
    }

    public static async Task<DeleteDocumentsCommand<TDocument, TProp>> PrepareDeleteDocuments<TDocument, TProp>(
        this DbConnection dbConnection,
        System.Linq.Expressions.Expression<Func<TDocument, TProp>> byIndexProperty)
    {
        var command = new DeleteDocumentsCommand<TDocument, TProp>(dbConnection, byIndexProperty.PropertyName());
        await command.DbCommand.PrepareAsync();
        return command;
    }

    public static async Task<DeleteDocumentsCommand<TDocument, TProp>> PrepareDeleteDocuments<TDocument, TProp>(
        this DbTransaction dbTransaction,
        System.Linq.Expressions.Expression<Func<TDocument, TProp>> byIndexProperty)
    {
        var command = await PrepareDeleteDocuments<TDocument, TProp>(dbTransaction.Connection, byIndexProperty);
        command.DbCommand.Transaction = dbTransaction;
        return command;
    }

    public static async Task<DeleteReturnDocumentsCommand<TDocument, TProp>> PrepareDeleteReturnDocuments<TDocument, TProp>(
        this DbConnection dbConnection, System.Linq.Expressions.Expression<Func<TDocument, TProp>> byIndexProperty)
    {
        var command = new DeleteReturnDocumentsCommand<TDocument, TProp>(dbConnection, byIndexProperty.PropertyName());
        await command.DbCommand.PrepareAsync();
        return command;
    }

    public static async Task<DeleteReturnDocumentsCommand<TDocument, TProp>> PrepareDeleteReturnDocuments<TDocument, TProp>(
        this DbTransaction dbTransaction,
        System.Linq.Expressions.Expression<Func<TDocument, TProp>> byIndexProperty)
    {
        var command = await PrepareDeleteReturnDocuments<TDocument, TProp>(dbTransaction.Connection, byIndexProperty);
        command.DbCommand.Transaction = dbTransaction;
        return command;
    }

    public static async Task<SaveDocumentCommand<T>> PrepareSaveDocument<T>(this DbConnection dbConnection)
    {
        var command = new SaveDocumentCommand<T>(dbConnection);
        await command.DbCommand.PrepareAsync();
        return command;
    }

    public static async Task<SaveDocumentCommand<T>> PrepareSaveDocument<T>(this DbTransaction dbTransaction)
    {
        var command = await PrepareSaveDocument<T>(dbTransaction.Connection);
        command.DbCommand.Transaction = dbTransaction;
        return command;
    }

    public static async Task<SaveReturnDocumentCommand<T>> PrepareSaveReturnDocument<T>(this DbConnection dbConnection)
    {
        var command = new SaveReturnDocumentCommand<T>(dbConnection);
        await command.DbCommand.PrepareAsync();
        return command;
    }

    public static async Task<SaveReturnDocumentCommand<T>> PrepareSaveReturnDocument<T>(this DbTransaction dbTransaction)
    {
        var command = await PrepareSaveReturnDocument<T>(dbTransaction.Connection);
        command.DbCommand.Transaction = dbTransaction;
        return command;
    }

    public static async Task<GetDocumentCommand<TDocument, TId>> PrepareGetDocument<TDocument, TId>(this DbConnection dbConnection)
    {
        var command = new GetDocumentCommand<TDocument, TId>(dbConnection);
        await command.DbCommand.PrepareAsync();
        return command;
    }

    public static async Task<GetDocumentCommand<TDocument, TId>> PrepareGetDocument<TDocument, TId>(this DbTransaction dbTransaction)
    {
        var command = await PrepareGetDocument<TDocument, TId>(dbTransaction.Connection);
        command.DbCommand.Transaction = dbTransaction;
        return command;
    }

    public static async Task<GetDocumentsCommand<TDocument, TProp>> PrepareGetDocuments<TDocument, TProp>(this DbConnection dbConnection, System.Linq.Expressions.Expression<Func<TDocument, TProp>> byIndexProperty)
    {
        var command = new GetDocumentsCommand<TDocument, TProp>(dbConnection, byIndexProperty.PropertyName());
        await command.DbCommand.PrepareAsync();
        return command;
    }

    public static async Task<GetDocumentsCommand<TDocument, TProp>> PrepareGetDocuments<TDocument, TProp>(this DbTransaction dbTransaction, System.Linq.Expressions.Expression<Func<TDocument, TProp>> byIndexProperty)
    {
        var command = await PrepareGetDocuments<TDocument, TProp>(dbTransaction.Connection, byIndexProperty);
        command.DbCommand.Transaction = dbTransaction;
        return command;
    }
}