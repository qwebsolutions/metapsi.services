﻿using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System;
using System.Collections.Generic;

namespace Metapsi.Controls
{
    public static class CommandButton
    {
        public class Props
        {
            public string Label { get; set; }
            //public Command OnClick { get; set; }
            public bool Enabled { get; set; } = true;
            public string SvgIcon { get; set; }
            public Button.Style Style { get; set; } = Button.Style.Primary;
        }

        public class Props<TState> : Props
        {
            public HyperType.Action<TState> OnClick { get; set; }
        }

        public class Props<TState, TPayload> : Props
        {
            public HyperType.Action<TState, TPayload> OnClick { get; set; }
            public TPayload Payload { get; set; }
        }

        public static Var<HyperNode> Render<TState, TPayload>(this LayoutBuilder b, Var<Props<TState, TPayload>> props)
        {
            var button = b.RenderBase(props.As<Props>());
            b.SetOnClick<TState,TPayload>(button, b.Get(props, x => x.OnClick), b.Get(props, x => x.Payload));
            return button;
        }

        public static Var<HyperNode> Render<TState>(this LayoutBuilder b, Var<Props<TState>> props)
        {
            var button = b.RenderBase(props.As<Props>());
            b.SetOnClick<TState>(button, b.Get(props, x => x.OnClick));
            return button;
        }

        private static Var<HyperNode> RenderBase(this LayoutBuilder b, Var<Props> props)
        {
            var button = b.Node("button", "rounded");
            var buttonContent = b.Add(button, b.Div("flex flex-row space-x-2 items-center"));
            b.If(b.HasValue(b.Get(props, x => x.SvgIcon)), b =>
            {
                var iconContainer = b.Add(buttonContent, b.Div("h-5 w-5"));
                b.SetInnerHtml(iconContainer, b.Get(props, x => x.SvgIcon));
            });

            b.If(b.HasValue(b.Get(props, x => x.Label)), b =>
            {
                b.Add(buttonContent, b.Text(b.Get(props, x => x.Label)));
                b.AddClass(button, "py-2 px-4 shadow");
            },
            b =>
            {
                b.AddClass(button, "p-1 shadow");
            });


            b.If(b.Get(props, x => x.Enabled), b =>
            {
                var bgClass = b.Switch(
                    b.Get(props, x => x.Style),
                    b => b.Const(""),
                    (Button.Style.Primary, b => b.Const("bg-sky-500")),
                    (Button.Style.Danger, b => b.Const("bg-red-500")),
                    (Button.Style.Light, b => b.Const("bg-white")));

                b.If(b.HasValue(bgClass), b =>
                {
                    b.AddClass(button, bgClass);
                });
            },
            b =>
            {
                // if disabled
                b.SetAttr(button, Html.disabled, true);
                b.AddClass(button, "bg-gray-300");
            });

            return button;
        }

        public static void AddButtonStyle(this PropsBuilder b, Var<DynamicObject> props)
        {
            b.AddClass(props, "rounded flex flex-row items-center py-2 px-4 shadow text-white");
        }

        public static void AddPrimaryButtonStyle(this PropsBuilder b, Var<DynamicObject> props)
        {
            b.AddButtonStyle(props);
            b.AddClass(props, "bg-sky-500");
        }
    }

    public static partial class Controls
    {
        public static Var<HyperNode> CommandButton<TState>(this LayoutBuilder b, Var<Metapsi.Controls.CommandButton.Props<TState>> props)
        {
            return b.Call(Metapsi.Controls.CommandButton.Render, props);
        }

        public static Var<HyperNode> CommandButton<TState>(this LayoutBuilder b, Action<Modifier<CommandButton.Props<TState>>> updateDefaults)
        {
            return b.CommandButton(b.NewObj(updateDefaults));
        }

        public static Var<HyperNode> CommandButton<TState, TPayload>(this LayoutBuilder b, Var<Metapsi.Controls.CommandButton.Props<TState, TPayload>> props)
        {
            return b.Call(Metapsi.Controls.CommandButton.Render, props);
        }

        public static Var<HyperNode> CommandButton<TState, TPayload>(this LayoutBuilder b, Action<Modifier<CommandButton.Props<TState, TPayload>>> updateDefaults)
        {
            return b.CommandButton(b.NewObj(updateDefaults));
        }
    }
}
