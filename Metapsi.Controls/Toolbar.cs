﻿using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Metapsi.Controls
{
    //public static class Toolbar
    //{
    //    internal static Var<HyperNode> Render(BlockBuilder b, params System.Func<BlockBuilder, Var<HyperNode>>[] children)
    //    {
    //        return b.Div("flex flex-row space-x-4 items-center p-4", children);
    //    }
    //}

    public static partial class Controls
    {

        public static Var<HyperNode> Toolbar(this LayoutBuilder b, params System.Func<LayoutBuilder, Var<HyperNode>>[] children)
        {
            return b.Div("flex flex-row space-x-4 items-center p-4", children);
        }

        public static Var<HyperNode> Toolbar(this LayoutBuilder b, params Var<HyperNode>[] children)
        {
            return b.Div("flex flex-row space-x-4 items-center p-4", b.List(children));
        }
    }
}

