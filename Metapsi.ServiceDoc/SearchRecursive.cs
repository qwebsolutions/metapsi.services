using Metapsi.Syntax;
using System.Collections.Generic;
using System;

namespace Metapsi;

public static class SearchRecursiveExtensions
{
    public static T SearchRecursive<T>(
        T node,
        Func<T, IEnumerable<T>> drill,
        Func<T, bool> stop,
        T defaultValue)
    {
        Reference<T> intoReference = new Reference<T>() { Value = defaultValue };
        SearchRecursive(node, drill, stop, intoReference);
        return intoReference.Value;
    }

    public static void SearchRecursive<T>(
        T node,
        Func<T, IEnumerable<T>> drill,
        Func<T, bool> stop,
        Reference<T> intoReference = null)
    {
        if (stop(node))
        {
            if (intoReference != null)
            {
                intoReference.Value = node;
            }
            return;
        }
        foreach (var child in drill(node))
        {
            SearchRecursive(child, drill, stop, intoReference);
        }
    }

    public static Var<T> SearchRecursive<T>(
        this SyntaxBuilder b,
        Var<T> root,
        Var<Func<T, List<T>>> drill,
        Var<Func<T, bool>> match,
        Var<T> defaultValue)
    {
        var intoReference = b.Ref(b.NewObj<SearchResult<T>>(
            b =>
            {
                b.Set(x => x.Found, false);
                b.Set(x => x.Result, defaultValue);
            }));
        b.Call(SearchRecursive, root, drill, match, intoReference);
        return b.Get(b.GetRef(intoReference), x => x.Result);
    }

    internal class SearchResult<T>
    {
        public bool Found { get; set; } = false;
        public T Result { get; set; }
    }

    private static void SearchRecursive<T>(
        SyntaxBuilder b,
        Var<T> current,
        Var<Func<T, List<T>>> drill,
        Var<Func<T, bool>> match,
        Var<Reference<SearchResult<T>>> intoReference)
    {
        b.If(
            b.Not(
                b.Get(b.GetRef(intoReference), x => x.Found)),
            b => b.If(
                b.Call(match, current),
               b =>
               {
                   b.Set(b.GetRef(intoReference), x => x.Found, true);
                   b.Set(b.GetRef(intoReference), x => x.Result, current);
               },
               b =>
               {
                   b.Foreach(
                       b.Call(drill, current),
                       (b, child) =>
                       {
                           b.Call(SearchRecursive, child, drill, match, intoReference);
                       });
               }));
    }
}
