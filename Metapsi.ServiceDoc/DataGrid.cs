﻿using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metapsi.Html
{
    public class DataGridBuilder<TRow>
    {
        public DataTableBuilder<TRow> DataTableBuilder { get; set; }

        public Action<PropsBuilder<HtmlDiv>> SetContainerProps { get; set; } = b => { b.SetClass(b.Const("flex flex-col gap-2")); };

        public Func<LayoutBuilder, Var<IVNode>> CreateToolbarActions { get; set; } = b => b.VoidNode();

        public Func<LayoutBuilder, Var<TRow>, Var<List<IVNode>>> CreateRowActions { get; set; } = (b, row) => b.NewCollection<IVNode>();
    }

    public static class DataGridControl
    {
        private const string GarbageBinIcon = "<svg\r\n id=\"Layer_1\"\r\n data-name=\"Layer 1\"\r\n xmlns=\"http://www.w3.org/2000/svg\"\r\n        viewBox=\"0 0 105.16 122.88\" width=\"20\" height=\"20\"\r\n        ><defs\r\n            ><style>\r\n                .cls-1 {\r\n                    fill-rule: evenodd;\r\n                }\r\n            </style></defs\r\n        ><path\r\n            fill=\"currentColor\"\r\n            class=\"cls-1\"\r\n            d=\"M11.17,37.16H94.65a8.4,8.4,0,0,1,2,.16,5.93,5.93,0,0,1,2.88,1.56,5.43,5.43,0,0,1,1.64,3.34,7.65,7.65,0,0,1-.06,1.44L94,117.31v0l0,.13,0,.28v0a7.06,7.06,0,0,1-.2.9v0l0,.06v0a5.89,5.89,0,0,1-5.47,4.07H17.32a6.17,6.17,0,0,1-1.25-.19,6.17,6.17,0,0,1-1.16-.48h0a6.18,6.18,0,0,1-3.08-4.88l-7-73.49a7.69,7.69,0,0,1-.06-1.66,5.37,5.37,0,0,1,1.63-3.29,6,6,0,0,1,3-1.58,8.94,8.94,0,0,1,1.79-.13ZM5.65,8.8H37.12V6h0a2.44,2.44,0,0,1,0-.27,6,6,0,0,1,1.76-4h0A6,6,0,0,1,43.09,0H62.46l.3,0a6,6,0,0,1,5.7,6V6h0V8.8h32l.39,0a4.7,4.7,0,0,1,4.31,4.43c0,.18,0,.32,0,.5v9.86a2.59,2.59,0,0,1-2.59,2.59H2.59A2.59,2.59,0,0,1,0,23.62V13.53H0a1.56,1.56,0,0,1,0-.31v0A4.72,4.72,0,0,1,3.88,8.88,10.4,10.4,0,0,1,5.65,8.8Zm42.1,52.7a4.77,4.77,0,0,1,9.49,0v37a4.77,4.77,0,0,1-9.49,0v-37Zm23.73-.2a4.58,4.58,0,0,1,5-4.06,4.47,4.47,0,0,1,4.51,4.46l-2,37a4.57,4.57,0,0,1-5,4.06,4.47,4.47,0,0,1-4.51-4.46l2-37ZM25,61.7a4.46,4.46,0,0,1,4.5-4.46,4.58,4.58,0,0,1,5,4.06l2,37a4.47,4.47,0,0,1-4.51,4.46,4.57,4.57,0,0,1-5-4.06l-2-37Z\"\r\n        />\r\n    </svg>\r\n\r\n";

        private const string ActionColumnName = "__action__";

        public static Var<IVNode> DataGrid<TRow>(
            this LayoutBuilder b,
            DataGridBuilder<TRow> dataGridBuilder,
            Var<List<TRow>> rows,
            Var<List<string>> columns)
        {
            var dataTableBuilder = dataGridBuilder.DataTableBuilder.Clone();

            var withActionsColumn = b.NewCollection<string>();
            b.PushRange(withActionsColumn, columns);
            b.Push(withActionsColumn, b.Const(ActionColumnName));

            dataTableBuilder.CreateHeaderCell = (b, column) =>
            {
                return b.If(
                    b.AreEqual(column, b.Const(ActionColumnName)),
                    b =>
                    {
                        return b.VoidNode();
                    },
                    b =>
                    {
                        return dataGridBuilder.DataTableBuilder.CreateHeaderCell(b, column);
                    });
            };

            dataTableBuilder.AddTrProps((b, row) =>
            {
                b.AddClass("group");
            });

            dataTableBuilder.AddTdProps(
                (b, row, column) =>
                b.If(
                    b.AreEqual(column, b.Const(ActionColumnName)),
                    b => b.AddClass("relative")));

            dataTableBuilder.OverrideDataCell(ActionColumnName,
                (b, row) =>
                {
                    return b.HtmlDiv(
                        b =>
                        {
                            b.SetClass("hidden absolute group-hover:flex flex-row items-center justify-center right-1 top-0 bottom-0");
                        },
                        dataGridBuilder.CreateRowActions(b, row));
                });

            return b.HtmlDiv(
                dataGridBuilder.SetContainerProps,
                b.Call(dataGridBuilder.CreateToolbarActions),
                b.DataTable(dataTableBuilder, rows, withActionsColumn));
        }

        public static Var<IVNode> DataGrid<TRow>(
            this LayoutBuilder b,
            DataGridBuilder<TRow> dataGridBuilder,
            Var<List<TRow>> rows)
        {
            return b.DataGrid(dataGridBuilder, rows, b.Const(DataTable.GetColumns<TRow>()));
        }

        public static Var<IVNode> DataGrid<TRow>(
            this LayoutBuilder b,
            DataGridBuilder<TRow> dataGridBuilder,
            Var<List<TRow>> rows,
            params string[] columns)
        {
            return b.DataGrid(dataGridBuilder, rows, b.Const(columns.ToList()));
        }

        public static void AddRowAction<TRow>(
            this DataGridBuilder<TRow> dataGridBuilder,
            Func<LayoutBuilder, Var<TRow>, Var<IVNode>> create)
        {
            var prevBuilder = dataGridBuilder.CreateRowActions;

            dataGridBuilder.CreateRowActions = (b, row) =>
            {
                var prevActions = prevBuilder(b, row);
                var outActions = b.NewCollection<IVNode>();
                b.PushRange(outActions, prevActions);
                b.Push(outActions, create(b, row));
                return outActions;
            };
        }

        public static Var<IVNode> RowIconAction(this LayoutBuilder b, Action<PropsBuilder<HtmlButton>> buildContainer, Var<IVNode> child)
        {
            return b.HtmlButton(
                b =>
                {
                    b.SetClass("flex rounded bg-gray-200 w-10 h-10 p-1 cursor-pointer justify-center items-center opacity-50 hover:opacity-100");
                    buildContainer(b);
                },
                child);
        }

        public static Var<IVNode> DeleteRowIconAction(this LayoutBuilder b, Action<PropsBuilder<HtmlButton>> buildContainer)
        {
            return b.RowIconAction(b =>
            {
                b.AddClass("text-red-500");
                buildContainer(b);
            },
            b.HtmlDiv(
                b =>
                {
                    b.SetClass("w-6 h-6");
                    b.SetInnerHtml(GarbageBinIcon);
                }));
        }
    }

    public static class DataGridBuilder
    {
        public static DataTableBuilder<TRow> DataTable<TRow>()
        {
            return new DataTableBuilder<TRow>()
            {
                SetTableProps = b =>
                {
                    b.SetClass("bg-white border-collapse w-full overflow-hidden");
                },
                SetTheadProps = b =>
                {
                    b.SetClass("text-left text-sm text-gray-500 bg-white top-16 shadow");
                },
                SetThProps = (b, column) => b.SetClass("py-4 px-2 border-b border-gray-300 bg-white"),
                SetTdProps = (b, row, column) => b.SetClass("py-4 px-2 border-b border-gray-300"),
                SetTbodyProps = (b) => b.SetClass("break-normal")
            };
        }

        public static DataGridBuilder<TRow> DataGrid<TRow>()
        {
            return new DataGridBuilder<TRow>()
            {
                DataTableBuilder = DataTable<TRow>(),
                SetContainerProps = b =>
                {
                    b.SetClass("flex flex-col w-full bg-white gap-8");
                }
            };
        }
    }
}

