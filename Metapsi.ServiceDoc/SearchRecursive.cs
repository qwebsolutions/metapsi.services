using Metapsi.Syntax;
using System.Collections.Generic;
using System;

namespace Metapsi;

public static class SearchRecursiveExtensions
{
    public static Var<T> SearchRecursive<T>(
        this ISyntaxBuilder b,
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
        b.Call<ISyntaxBuilder, T, Func<T, List<T>>, Func<T, bool>, Reference<SearchResult<T>>>(SearchRecursiveExtensions.SearchRecursive, root, drill, match, intoReference);
        return b.Get(b.GetRef(intoReference), x => x.Result);
    }

    internal class SearchResult<T>
    {
        public bool Found { get; set; } = false;
        public T Result { get; set; }
    }

    private static void SearchRecursive<T>(
        ISyntaxBuilder b,
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
                           var sr = b.Def<ISyntaxBuilder, T, Func<T, List<T>>, Func<T, bool>, Reference<SearchResult<T>>>(SearchRecursiveExtensions.SearchRecursive);
                           b.Call(sr, child, drill, match, intoReference);
                       });
               }));
    }
}
