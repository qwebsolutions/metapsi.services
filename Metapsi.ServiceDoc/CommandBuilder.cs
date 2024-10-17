using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
namespace Metapsi;

public static partial class ServiceDoc
{
    public class CommandBuilder
    {
        DbCommand command = null;
        private int ParameterIndex = 0;
        private StringBuilder stringBuilder = new StringBuilder();

        public CommandBuilder(DbConnection connection)
        {
            command = connection.CreateCommand();
        }

        public DbCommand GetCommand()
        {
            command.CommandText = stringBuilder.ToString();
            return command;
        }

        internal string NextParameterName()
        {
            ParameterIndex++;
            return "p" + ParameterIndex;
        }

        public void AppendCommand(string statement)
        {
            stringBuilder.Append(statement);
            stringBuilder.AppendLine(";");
        }

        public void AppendCommand<TParam>(string statement, string parameterName, TParam parameter)
        {
            stringBuilder.Append(statement);
            stringBuilder.AppendLine(";");
            var dbParameter = command.CreateParameter();
            dbParameter.ParameterName = "@" + parameterName;
            dbParameter.Value = parameter;
            command.Parameters.Add(dbParameter);
        }

        public static string GetInsertStatement(System.Type type, string parameterName = "json")
        {
            var tableName = GetTableName(type);
            return $"insert into {tableName} (json) values(@{parameterName})";
        }

        public static string GetInsertReturnDocumentStatement(System.Type type, string parameterName = "json")
        {
            var tableName = GetTableName(type);
            return $"insert into {tableName} (json) values(@{parameterName}) returning json";
        }

        public static string GetDeleteDocumentsStatement(System.Type type, string byProperty, string parameterName = "value")
        {
            var tableName = GetTableName(type);
            return $"delete from {tableName} where {byProperty}=@{parameterName}";
        }

        public static string GetDeleteReturnDocumentsStatement(System.Type type, string byProperty, string parameterName = "value")
        {
            var tableName = GetTableName(type);
            return $"delete from {tableName} where {byProperty}=@{parameterName} returning json";
        }

        public static string GetSaveDocumentStatement(System.Type type, string parameterName = "json")
        {
            var tableName = GetTableName(type);
            return $"insert into {tableName} (json) values(@{parameterName}) ON CONFLICT (Id) DO UPDATE SET json=@{parameterName}";
        }

        public static string GetSaveReturnDocumentStatement(System.Type type, string parameterName = "json")
        {
            var tableName = GetTableName(type);
            return $"insert into {tableName} (json) values(@{parameterName}) ON CONFLICT (Id) DO UPDATE SET json=@{parameterName} returning json";
        }

        public static string GetSelectDocumentsStatement(System.Type type, string byIndexProperty, string parameterName = "value")
        {
            var tableName = GetTableName(type);
            return $"select json from {tableName} where {byIndexProperty} = @{parameterName}";
        }

        public static string GetListDocumentsStatement(System.Type type)
        {
            var tableName = GetTableName(type);
            return $"select json from {tableName}";
        }
    }

    public static void InsertDocument<T>(this CommandBuilder commandBuilder, T document)
    {
        var parameterName = commandBuilder.NextParameterName();
        commandBuilder.AppendCommand(CommandBuilder.GetInsertStatement(document.GetType(), parameterName), parameterName, Metapsi.Serialize.ToJson(document));
    }

    public static void InsertReturnDocument<T>(this CommandBuilder commandBuilder, T document)
    {
        var parameterName = commandBuilder.NextParameterName();
        commandBuilder.AppendCommand(CommandBuilder.GetInsertReturnDocumentStatement(document.GetType(), parameterName), parameterName, Metapsi.Serialize.ToJson(document));
    }

    public static void DeleteDocuments<TDocument, TValue>(this CommandBuilder commandBuilder, string byIndexProperty, TValue value)
    {
        var parameterName = commandBuilder.NextParameterName();
        commandBuilder.AppendCommand(CommandBuilder.GetDeleteDocumentsStatement(typeof(TDocument), byIndexProperty, parameterName), parameterName, value);
    }

    public static void DeleteDocuments<TDocument, TValue>(this CommandBuilder commandBuilder, System.Linq.Expressions.Expression<Func<TDocument, TValue>> byIndexProperty, TValue value)
    {
        commandBuilder.DeleteDocuments<TDocument, TValue>(byIndexProperty.PropertyName(), value);
    }

    public static void DeleteReturnDocuments<TDocument,TValue>(this CommandBuilder commandBuilder, string byIndexProperty, TValue value)
    {
        var parameterName = commandBuilder.NextParameterName();
        commandBuilder.AppendCommand(CommandBuilder.GetDeleteReturnDocumentsStatement(typeof(TDocument), byIndexProperty, parameterName), parameterName, value);
    }

    public static void DeleteReturnDocuments<TDocument, TValue>(this CommandBuilder commandBuilder, System.Linq.Expressions.Expression<Func<TDocument, TValue>> byIndexProperty, TValue value)
    {
        commandBuilder.DeleteReturnDocuments<TDocument, TValue>(byIndexProperty.PropertyName(), value);
    }

    public static void SaveDocument<T>(this CommandBuilder commandBuilder, T document)
    {
        var parameterName = commandBuilder.NextParameterName();
        commandBuilder.AppendCommand(CommandBuilder.GetSaveDocumentStatement(document.GetType(), parameterName), parameterName, Metapsi.Serialize.ToJson(document));
    }

    public static void SaveReturnDocument<T>(this CommandBuilder commandBuilder, T document)
    {
        var parameterName = commandBuilder.NextParameterName();
        commandBuilder.AppendCommand(CommandBuilder.GetSaveReturnDocumentStatement(document.GetType(), parameterName), parameterName, Metapsi.Serialize.ToJson(document));
    }

    public static void GetDocuments<T, TValue>(this CommandBuilder commandBuilder, string byIndexProperty, TValue value)
    {
        var parameterName = commandBuilder.NextParameterName();
        commandBuilder.AppendCommand(CommandBuilder.GetSelectDocumentsStatement(typeof(T), byIndexProperty, parameterName), parameterName, value);
    }

    public static void GetDocuments<T, TValue>(this CommandBuilder commandBuilder, System.Linq.Expressions.Expression<Func<T, TValue>> byIndexProperty, TValue value)
    {
        commandBuilder.GetDocuments<T, TValue>(byIndexProperty.PropertyName(), value);
    }

    public static void ListDocuments<T>(this CommandBuilder commandBuilder)
    {
        commandBuilder.AppendCommand(CommandBuilder.GetListDocumentsStatement(typeof(T)));
    }

    public static DbCommand BuildCommand(this DbConnection dbConnection, Action<CommandBuilder> action, DbTransaction dbTransaction)
    {
        CommandBuilder commandBuilder = new CommandBuilder(dbConnection);
        action(commandBuilder);
        var command = commandBuilder.GetCommand();
        if (dbTransaction != null)
        {
            command.Transaction = dbTransaction;
        }
        return command;
    }

    public static async Task ChangeDocuments(this DbConnection dbConnection, Action<CommandBuilder> action, DbTransaction dbTransaction = null)
    {
        var command = dbConnection.BuildCommand(action, dbTransaction);
        await command.ExecuteNonQueryAsync();
    }

    public static async Task ChangeDocuments(this DbTransaction dbTransaction, Action<CommandBuilder> action)
    {
        await dbTransaction.Connection.ChangeDocuments(action, dbTransaction);
    }

    public static async Task<List<T>> GetDocuments<T>(this DbConnection dbConnection, Action<CommandBuilder> action, DbTransaction transaction = null)
    {
        List<T> results = new List<T>();
        var command = dbConnection.BuildCommand(action, transaction);
        using var dbReader = await command.ExecuteReaderAsync();
        while (await dbReader.ReadAsync())
        {
            var json = dbReader.GetString(0);
            Metapsi.Serialize.FromJson<T>(json);
        }

        return results;
    }

    public static async Task<List<T>> GetDocuments<T>(this DbTransaction dbTransaction, Action<CommandBuilder> action)
    {
        return await dbTransaction.Connection.GetDocuments<T>(action, dbTransaction);
    }

    public static async Task<T> GetScalar<T>(this DbConnection dbConnection, Action<CommandBuilder> action, DbTransaction dbTransaction = null)
    {
        var command = dbConnection.BuildCommand(action, dbTransaction);
        var result = (T)await command.ExecuteScalarAsync();
        return result;
    }

    public static async Task<T> GetScalar<T>(this DbTransaction dbTransaction, Action<CommandBuilder> action)
    {
        return await dbTransaction.Connection.GetScalar<T>(action, dbTransaction);
    }
}
