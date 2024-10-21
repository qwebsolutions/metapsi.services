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

        public static string GetInsertStatement(System.Type type, string parameterName)
        {
            var tableName = GetTableName(type);
            return $"insert into {tableName} (json) values(@{parameterName})";
        }

        public static string GetInsertReturnDocumentStatement(System.Type type, string parameterName)
        {
            var tableName = GetTableName(type);
            return $"insert into {tableName} (json) values(@{parameterName}) returning json";
        }

        public static string GetDeleteDocumentsStatement(System.Type type, string byProperty, string parameterName)
        {
            var tableName = GetTableName(type);
            return $"delete from {tableName} where {byProperty}=@{parameterName}";
        }

        public static string GetDeleteReturnDocumentsStatement(System.Type type, string byProperty, string parameterName)
        {
            var tableName = GetTableName(type);
            return $"delete from {tableName} where {byProperty}=@{parameterName} returning json";
        }

        public static string GetSaveDocumentStatement(System.Type type, string parameterName)
        {
            var tableName = GetTableName(type);
            return $"insert into {tableName} (json) values(@{parameterName}) ON CONFLICT (Id) DO UPDATE SET json=@{parameterName}";
        }

        public static string GetSaveReturnDocumentStatement(System.Type type, string parameterName)
        {
            var tableName = GetTableName(type);
            return $"insert into {tableName} (json) values(@{parameterName}) ON CONFLICT (Id) DO UPDATE SET json=@{parameterName} returning json";
        }

        public static string GetSelectDocumentsStatement(System.Type type, string byIndexProperty, string parameterName)
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
}
