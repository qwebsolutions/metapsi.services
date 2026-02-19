using System.Collections.Generic;

namespace Metapsi
{
    public static partial class ServiceDoc
    {
        public class SaveResult
        {
            public bool Success { get; set; }
            public string Message { get; set; }
        }

        public class DeleteResult
        {
            public bool Success { get; set; }
            public string Message { get; set; }
        }

        public class SearchInput
        {
            public string Query { get; set; } = string.Empty;
            public int MaxResults { get; set; }
        }

        public class SearchResult<T>
        {
            public List<T> Items { get; set; } = new List<T>();
            public int OutOfTotal { get; set; }
        }

        public static string DocumentTypeIdentifier<T>()
        {
            return $"document-{typeof(T).CSharpTypeName()}";
        }
    }
}