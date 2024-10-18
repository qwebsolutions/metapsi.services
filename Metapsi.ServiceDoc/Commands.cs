using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;

namespace Metapsi;

public static partial class ServiceDoc
{
    public class InsertDocumentCommand<T>
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
    }

    public class InsertReturnDocumentCommand<T>
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
            var result = (string) await this.DbCommand.ExecuteScalarAsync();
            return Metapsi.Serialize.FromJson<T>(result);
        }
    }

    public class DeleteDocumentCommand<T, TId>
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
            var count = (int)await this.DbCommand.ExecuteScalarAsync();

            if (count > 1)
            {
                throw new Exception($"{typeof(T).Name} id {id} matches multiple documents");
            }
        }
    }

    public class DeleteReturnDocumentCommand<T, TId>
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
            var dbReader = await this.DbCommand.ExecuteReaderAsync();

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
    }

    public class DeleteDocumentsCommand<T, TProp>
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
    }

    public class DeleteReturnDocumentsCommand<T, TProp>
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

        public async IAsyncEnumerable<T> ExecuteAsync(TProp value)
        {
            this.DbCommand.Parameters[0].Value = value;
            var dbReader = await this.DbCommand.ExecuteReaderAsync();
            while (await dbReader.ReadAsync())
            {
                var json = dbReader.GetString(0);
                var document = Metapsi.Serialize.FromJson<T>(json);
                yield return document;
            }
        }
    }

    public class SaveDocumentCommand<T>
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
    }

    public class SaveReturnDocumentCommand<T>
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
    }

    public class GetDocumentCommand<T, TId>
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
    }

    public class GetDocumentsCommand<T, TProp>
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

        public async IAsyncEnumerable<T> ExecuteAsync(TProp value)
        {
            this.DbCommand.Parameters[0].Value = value;
            var dbReader = await this.DbCommand.ExecuteReaderAsync();
            while (await dbReader.ReadAsync())
            {
                var json = dbReader.GetString(0);
                var document = Metapsi.Serialize.FromJson<T>(json);
                yield return document;
            }
        }
    }

    public class ListDocumentsCommand<T>
    {
        public DbCommand DbCommand { get; private set; }

        public ListDocumentsCommand(DbConnection dbConnection)
        {
            DbCommand = dbConnection.CreateCommand();
            DbCommand.CommandText = CommandBuilder.GetListDocumentsStatement(typeof(T));
        }

        public async IAsyncEnumerable<T> ExecuteAsync()
        {
            var dbReader = await this.DbCommand.ExecuteReaderAsync();
            while (await dbReader.ReadAsync())
            {
                var json = dbReader.GetString(0);
                var document = Metapsi.Serialize.FromJson<T>(json);
                yield return document;
            }
        }
    }
}