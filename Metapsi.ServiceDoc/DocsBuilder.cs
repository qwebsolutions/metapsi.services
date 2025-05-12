using System.Linq.Expressions;
using System.Linq;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Metapsi
{
    public static partial class ServiceDoc
    {
        public static void AddIndex<T, TProp>(this IDocumentProps<T> b, Expression<Func<T, TProp>> property)
        {
            var documentProps = b as DocumentProps<T>;
            var indexPropertyName = property.PropertyName();
            // If configured twice just remove previous
            documentProps.IndexColumns.RemoveAll(x => x.SourceProperty == property.PropertyName());

            documentProps.IndexColumns.Add(new IndexColumn()
            {
                CSharpType = typeof(TProp),
                SourceProperty = property.PropertyName()
            });
        }

        //public static void ConfigureGroupRoutes(this DocsGroup docsGroup, Action<RouteHandlerBuilder> builder)
        //{
        //    docsGroup.GroupRoutesBuilder = builder;
        //}

        //public static void ConfigureDocumentRoutes<T>(this IDocumentProps<T> b, Action<RouteHandlerBuilder> builder)
        //{
        //    (b as DocumentProps<T>).DocumentRoutesBuilder = builder;
        //}

        public static void SetFrontendDefaultColumns<T>(this IDocumentProps<T> b, params string[] columnNames)
        {
            (b as DocumentProps<T>).FrontendDefaultColumns = columnNames.ToList();
        }

        /// <summary>
        /// Used to create new document in frontend. Set default values here
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="b"></param>
        /// <param name="createNew"></param>
        public static void SetFrontendNew<T>(this IDocumentProps<T> b, Func<Task<T>> createNew)
        {
            (b as DocumentProps<T>).Create = createNew;
        }

        /// <summary>
        /// Lists documents in frontend. Order documents here
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="b"></param>
        /// <param name="list"></param>
        public static void SetFrontendList<T>(this IDocumentProps<T> b, Func<Task<List<T>>> list)
        {
            (b as DocumentProps<T>).List = list;
        }

        public static void SetFrontendSave<T>(this IDocumentProps<T> b, Func<T, Task<SaveResult>> save)
        {
            (b as DocumentProps<T>).Save= save;
        }

        public static void SetFrontendDelete<T>(this IDocumentProps<T> b, Func<T, Task<DeleteResult>> delete)
        {
            (b as DocumentProps<T>).Delete = delete;
        }
    }
}