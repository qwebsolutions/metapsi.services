﻿using Metapsi.Hyperapp;
using Metapsi.Syntax;

namespace Metapsi.Controls
{
    public static partial class Inline
    {
        public static void SetInnerHtml(this LayoutBuilder b, Var<HyperNode> parent, Var<string> content)
        {
            b.SetAttr(parent, Html.innerHTML, content);
        }

        public static void SetInnerHtml(this LayoutBuilder b, Var<HyperNode> parent, string content)
        {
            b.SetAttr(parent, Html.innerHTML, b.Const(content));
        }

        public static Var<HyperNode> Svg(this LayoutBuilder b, string content, string classNames = "")
        {
            var container = b.Div(classNames);
            b.SetInnerHtml(container, content);
            return container;
        }
    }
}

