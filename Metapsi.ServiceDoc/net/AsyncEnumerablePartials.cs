using System;
using System.Collections.Generic;

namespace Metapsi
{
    public static partial class ServiceDoc
    {
        public partial class DeleteReturnDocumentsCommand<T, TProp> : IDisposable
        {
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
        }

        public partial class GetDocumentsCommand<T, TProp> : IDisposable
        {
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
        }

        public partial class ListDocumentsCommand<T> : IDisposable
        {
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
        }
    }
}
