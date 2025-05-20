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

        public static string DocumentTypeIdentifier<T>()
        {
            return $"document-{typeof(T).CSharpTypeName()}";
        }
    }
}