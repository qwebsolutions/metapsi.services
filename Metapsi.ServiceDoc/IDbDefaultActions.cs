using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Metapsi
{
    public static partial class ServiceDoc
    {
        public interface IDbDefaultActions
        {
            System.Func<Task> GetDefaultMigrate(TableMetadata tableMetadata);
            System.Func<Task<int>> GetDefaultCount<T>();
            System.Func<Task<List<T>>> GetDefaultList<T>();
            System.Func<T, Task<SaveResult>> GetDefaultSave<T>();
            System.Func<T, Task<DeleteResult>> GetDefaultDelete<T, TId>(System.Linq.Expressions.Expression<Func<T, TId>> getId);
        }
    }
}