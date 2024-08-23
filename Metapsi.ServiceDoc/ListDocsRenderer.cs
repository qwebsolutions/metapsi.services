using Metapsi.Hyperapp;
using Metapsi.Syntax;
using Microsoft.AspNetCore.Http;
using Metapsi.Shoelace;
using Metapsi.Dom;
using System;
using System.Linq;
using System.Collections.Generic;
using static Metapsi.Hyperapp.HyperType;
using System.Linq.Expressions;
using Metapsi.Html;
using Metapsi.Log;

namespace Metapsi
{
    public static partial class ServiceDoc
    {
        public class ListDocsPage<T>
        {
            public string ApiBase { get; set; }
            public List<T> Documents { get; set; } = new List<T>();
            public T EditDocument { get; set; }
            public string SummaryHtml { get; set; }
            public List<string> Columns { get; set; } = new List<string>();
            public string FilterText { get; set; }
        }

        private const string IdEditDocument = "id-edit-document";
        private const string IdRemoveDocument = "id-remove-document";

        internal static void Render<T>(HtmlBuilder b, ListDocsPage<T> model, Expression<Func<T, string>> idProperty)
        {
            b.AddStylesheet();
            //b.Document.Body.SetAttribute("class", "fixed top-0 right-0 left-0 bottom-0");
            b.BodyAppend(b.Hyperapp(model,
                (b, model) =>
                {
                    return b.RenderClient(model, idProperty);
                }));
        }

        internal static void Render(HtmlBuilder b, DocsOverviewModel model)
        {
            b.AddStylesheet();
            b.BodyAppend(b.Hyperapp(model,
                (b, model) =>
                {
                    return b.HtmlDiv(
                        b =>
                        {
                            b.SetClass("flex flex-row flex-wrap gap-2 p-4");
                        },
                        b.Map(b.Get(model, x => x.DocTypes), (b, service) =>
                        {
                            return b.HtmlA(
                                b =>
                                {
                                    b.SetHref(b.Get(service, x => x.ListUrl));
                                },
                                b.SlCard(
                                    b =>
                                    {
                                    },
                                    b.HtmlDiv(
                                        b=>
                                        {
                                            b.SetClass("font-semibold text-gray-800");
                                        },
                                        b.Text(b.Get(service, x => x.DocTypeName))),
                                    b.HtmlDiv(
                                        b=>
                                        {
                                            b.SetClass("text-gray-600");
                                        },
                                        b.Text(b.AsString(b.Get(service, x => x.Count)))),
                                    b.HtmlDiv(
                                        b=>
                                        {
                                            b.SetClass("text-xs");
                                        },
                                        b.Text(b.Get(service, x=>x.ApiUrl)))));
                        }));
                }));
        }

        private static Var<string> EntityName<T>(this SyntaxBuilder b)
        {
            return b.Const(typeof(T).Name);
        }

        private static Var<string> GetApiUrl<T>(this SyntaxBuilder b, Var<ListDocsPage<T>> model, Var<string> request)
        {
            return b.Concat(b.Get(model, x => x.ApiBase), b.Const("/"), request);
        }

        private static Var<string> GetApiUrl<T>(this SyntaxBuilder b, Var<ListDocsPage<T>> model)
        {
            return b.Get(model, x => x.ApiBase);
        }

        public static Var<IVNode> RenderClient<T>(this LayoutBuilder b, Var<ListDocsPage<T>> model, Expression<Func<T, string>> idProperty)
        {
            string EntityName = typeof(T).Name;
            var rows = b.Get(model, x => x.Documents);

            return b.HtmlDiv(
                b.HtmlDiv(b => b.SetClass("h-16")),
                b.Optional(
                    b.HasValue(b.Get(model, x => x.SummaryHtml)),
                    b => b.DocDescriptionPanel(b.Get(model, x => x.SummaryHtml))),
                b.FilterBox(model),
                b.DocsGrid(model, idProperty),
                b.PageHeader<T>(), // Added after content for Z layer
                b.EditDocumentPopup(model, idProperty),
                b.RemoveDocumentPopup<T>(idProperty));
        }

        public static Var<IVNode> FilterBox<T>(this LayoutBuilder b, Var<ListDocsPage<T>> model)
        {
            return b.HtmlDiv(
                b =>
                {
                    b.SetClass("flex flex-row justify-end p-4");
                },
                b.SlInput(
                    b =>
                    {
                        b.SetPill();
                        b.SetClearable();
                        b.BindTo(model, x => x.FilterText);
                    },
                    b.SlIcon(
                        b =>
                        {
                            b.SetName("search");
                            b.SetSlot(SlInput.Slot.Prefix);
                        })));
        }

        public static Var<IVNode> PageHeader<T>(this LayoutBuilder b)
        {
            return b.HtmlDiv(
                b =>
                {
                    b.SetClass("fixed top-0 left-0 right-0 h-16 flex flex-row items-center justify-center py-4 text-lg bg-gray-100 text-gray-700");
                },
                b.Text(b.Concat(b.FormatLabel(b.EntityName<T>()), b.Const(" Service Overview"))),
                b.SlButton(
                    b =>
                    {
                        b.SetVariantText();
                        b.AddClass("absolute right-2 text-2xl ");//text-[color:var(--sl-color-primary-600)] hover:text-[color:var(--sl-color-primary-500)]

                        b.OnClickAction(b.MakeAction((SyntaxBuilder b, Var<ServiceDoc.ListDocsPage<T>> model) =>
                        {
                            return InitDocument(b, model);
                        }));
                    },
                    b.HtmlDiv(
                        b =>
                        {
                            b.AddClass("flex w-6 h-6");
                            b.SetInnerHtml(b.Const(Metapsi.Heroicons.Solid.PlusCircle));
                            b.SetSlot("prefix");
                        })));
        }

        public static Var<IVNode> EditDocumentPopup<T>(
            this LayoutBuilder b,
            Var<ServiceDoc.ListDocsPage<T>> model,
            Expression<Func<T, string>> idProperty)
        {
            var getId = DefineGetId(b, idProperty);
            var isNew = b.Get(model, getId, (model, getId) => model.EditDocument == null || !model.Documents.Any(x => getId(x) == getId(model.EditDocument)));
            var caption = b.If(isNew, x => b.Const("Create new "), b => b.Const("Edit "));

            var buildContent = (LayoutBuilder b) => b.AutoEditForm(model, b.Get(model, x => x.EditDocument));
            return b.SlDialog(
                b =>
                {
                    b.SetId(b.Const(IdEditDocument));
                    b.SetLabel(b.Concat(caption, b.ToLowercase(b.FormatLabel(b.EntityName<T>()))));
                },
                b.Optional(
                    b.HasObject(b.Get(model, x => x.EditDocument)),
                    b => b.Call(buildContent)),
                b.SlButton(
                        b =>
                        {
                            b.SetSlot("footer");
                            b.SetVariantPrimary();

                            b.OnClickAction(b.MakeAction((SyntaxBuilder b, Var<ServiceDoc.ListDocsPage<T>> model) =>
                            {
                                return SaveDocument(b, model);
                            }));
                        },
                        b.Text("Save")));
        }



        public static Var<IVNode> RemoveDocumentPopup<T>(
            this LayoutBuilder b,
            Expression<Func<T, string>> idProperty)
        {
            var content =
                b.HtmlDiv(
                    b => b.SetClass("flex flex-col gap-4"),
                    b.Text(
                        b.Concat(
                            b.Const("Are you sure you want to remove this "),
                            b.ToLowercase(b.FormatLabel(b.EntityName<T>())),
                            b.Const(" ?"))),
                b.HtmlDiv(
                    b =>
                    {
                        // To make the buttons smaller at end
                        b.AddClass("flex flex-row gap-2 justify-end");
                    },
                    b.SlButton(
                        b =>
                        {
                            b.SetSlot("footer");
                            b.SetVariantDanger();
                            b.OnClickAction(b.MakeAction((SyntaxBuilder b, Var<ServiceDoc.ListDocsPage<T>> model) =>
                            {
                                //b.Remove(b.Get(model, x => x.Documents), b.Get(model, model => model.Documents.Single(x => x.Id == model.EditDocument.Id)));
                                return RemoveDocument(b, model, idProperty);
                            }));
                        },
                        b.Text("Remove")))
                );

            return b.SlDialog(
                b =>
                {
                    b.SetId(b.Const(IdRemoveDocument));
                    b.SetLabel(b.Concat(b.Const("Remove "), b.ToLowercase(b.FormatLabel(b.EntityName<T>()))));
                },
                content);
        }


        public static Var<HyperType.StateWithEffects> InitDocument<T>(SyntaxBuilder b, Var<ServiceDoc.ListDocsPage<T>> model)
        {
            var onResult = b.MakeAction(
                (SyntaxBuilder b, Var<ServiceDoc.ListDocsPage<T>> model, Var<T> result) =>
                {
                    b.Set(model, x => x.EditDocument, result);
                    var popup = b.GetElementById(b.Const(IdEditDocument));
                    b.SetDynamic(popup, DynamicProperty.Bool("open"), b.Const(true));
                    return b.Clone(model);
                });

            var onError = b.MakeAction(
                (SyntaxBuilder b, Var<ServiceDoc.ListDocsPage<T>> model, Var<ClientSideException> apiError) =>
                {
                    var editPopup = b.GetElementById(b.Const(IdEditDocument));
                    b.SetDynamic(editPopup, DynamicProperty.Bool("open"), b.Const(false));

                    var removePopup = b.GetElementById(b.Const(IdRemoveDocument));
                    b.SetDynamic(removePopup, DynamicProperty.Bool("open"), b.Const(false));
                    return b.Clone(model);
                });

            return b.MakeStateWithEffects(
                model,
                (SyntaxBuilder b, Var<HyperType.Dispatcher> dispatch) =>
                {
                    b.GetJson<T>(b.GetApiUrl(model, b.Const(ServiceDoc.InitDocument<T>().Name)),
                        b.Def((SyntaxBuilder b, Var<T> newDocument) =>
                        {
                            b.Dispatch(dispatch, onResult, newDocument);
                        }),
                        b.Def((SyntaxBuilder b, Var<ClientSideException> ex) =>
                        {
                            b.Dispatch(dispatch, onError, ex);
                        }));
                });
        }

        public static Var<HyperType.StateWithEffects> SaveDocument<T>(SyntaxBuilder b, Var<ServiceDoc.ListDocsPage<T>> model)
        {
            var onResult = b.MakeAction(
                (SyntaxBuilder b, Var<ServiceDoc.ListDocsPage<T>> model, Var<ServiceDoc.SaveResult<T>> result) =>
                {
                    var editPopup = b.GetElementById(b.Const(IdEditDocument));
                    b.SetDynamic(editPopup, DynamicProperty.Bool("open"), b.Const(false));

                    var removePopup = b.GetElementById(b.Const(IdRemoveDocument));
                    b.SetDynamic(removePopup, DynamicProperty.Bool("open"), b.Const(false));

                    b.Push(b.Get(model, x => x.Documents), b.Get(model, x => x.EditDocument));
                    return b.RefreshAllDocuments<T>();
                });

            var onError = b.MakeAction(
                (SyntaxBuilder b, Var<ServiceDoc.ListDocsPage<T>> model, Var<ClientSideException> apiError) =>
                {
                    var editPopup = b.GetElementById(b.Const(IdEditDocument));
                    b.SetDynamic(editPopup, DynamicProperty.Bool("open"), b.Const(false));

                    var removePopup = b.GetElementById(b.Const(IdRemoveDocument));
                    b.SetDynamic(removePopup, DynamicProperty.Bool("open"), b.Const(false));
                    return b.RefreshAllDocuments<T>();
                });

            return b.MakeStateWithEffects(
                model,
                (SyntaxBuilder b, Var<HyperType.Dispatcher> dispatch) =>
                {
                    b.PostJson(b.GetApiUrl(model),
                        b.Get(model, x => x.EditDocument),
                        b.Def((SyntaxBuilder b, Var<SaveResult<T>> saveResult) =>
                        {
                            b.Dispatch(dispatch, onResult, saveResult);
                        }),
                        b.Def((SyntaxBuilder b, Var<ClientSideException> ex) =>
                        {
                            b.Dispatch(dispatch, onError, ex);
                        }));
                });
        }


        public static Var<HyperType.StateWithEffects> RemoveDocument<T>(
            SyntaxBuilder b,
            Var<ServiceDoc.ListDocsPage<T>> model,
            Expression<Func<T, string>> idProperty)
        {
            var getId = DefineGetId(b, idProperty);

            var onResult = b.MakeAction(
                (SyntaxBuilder b, Var<ServiceDoc.ListDocsPage<T>> model, Var<ServiceDoc.DeleteResult<T>> result) =>
                {
                    var editPopup = b.GetElementById(b.Const(IdEditDocument));
                    b.SetDynamic(editPopup, DynamicProperty.Bool("open"), b.Const(false));

                    var removePopup = b.GetElementById(b.Const(IdRemoveDocument));
                    b.SetDynamic(removePopup, DynamicProperty.Bool("open"), b.Const(false));

                    var editedDoc = b.Get(model, getId, (model, getId) => model.Documents.Single(x => getId(x) == getId(model.EditDocument)));

                    b.Remove(b.Get(model, x => x.Documents), editedDoc);

                    return b.RefreshAllDocuments<T>();
                });

            var onError = b.MakeAction(
                (SyntaxBuilder b, Var<ServiceDoc.ListDocsPage<T>> model, Var<ClientSideException> apiError) =>
                {
                    var editPopup = b.GetElementById(b.Const(IdEditDocument));
                    b.SetDynamic(editPopup, DynamicProperty.Bool("open"), b.Const(false));

                    var removePopup = b.GetElementById(b.Const(IdRemoveDocument));
                    b.SetDynamic(removePopup, DynamicProperty.Bool("open"), b.Const(false));
                    return b.RefreshAllDocuments<T>();
                });

            return b.MakeStateWithEffects(
                model,
                (SyntaxBuilder b, Var<HyperType.Dispatcher> dispatch) =>
                {
                    var documentId = b.Call(getId, b.Get(model, x => x.EditDocument));
                    var url = b.GetApiUrl(model, documentId);

                    var fetch = b.Fetch(url, b =>
                    {
                        b.SetMethod("DELETE");
                    });
                    b.HandleJsonResponse(
                        fetch,
                        b.Def((SyntaxBuilder b, Var<DeleteResult<T>> deleteResult) =>
                        {
                            b.Dispatch(dispatch, onResult, deleteResult);
                        }),
                        b.Def((SyntaxBuilder b, Var<ClientSideException> ex) =>
                        {
                            b.Dispatch(dispatch, onError, ex);
                        }));
                });
        }

        public static Var<HyperType.Action<ServiceDoc.ListDocsPage<T>>> RefreshAllDocuments<T>(this SyntaxBuilder b)
        {
            return b.MakeAction((SyntaxBuilder b, Var<ServiceDoc.ListDocsPage<T>> model) =>
            {
                var onResult = b.MakeAction(
                    (SyntaxBuilder b, Var<ServiceDoc.ListDocsPage<T>> model, Var<List<T>> result) =>
                    {
                        b.Set(model, x => x.Documents, result);
                        return b.Clone(model);
                    });

                var onError = b.MakeAction(
                    (SyntaxBuilder b, Var<ServiceDoc.ListDocsPage<T>> model, Var<ClientSideException> apiError) =>
                    {
                        return b.Clone(model);
                    });

                return b.MakeStateWithEffects(
                    model,
                    (SyntaxBuilder b, Var<HyperType.Dispatcher> dispatch) =>
                    {
                        b.GetJson<List<T>>(b.GetApiUrl(model, b.Const(ServiceDoc.ListDocuments<T>().Name)),
                            b.Def((SyntaxBuilder b, Var<List<T>> newList) =>
                            {
                                b.Dispatch(dispatch, onResult, newList);
                            }),
                            b.Def((SyntaxBuilder b, Var<ClientSideException> ex) =>
                            {
                                b.Dispatch(dispatch, onError, ex);
                            }));
                    });
            });
        }

        public static Var<IVNode> DocDescriptionPanel(
            this LayoutBuilder b,
            Var<string> summaryHtml)
        {
            return b.SlDetails(
                b =>
                {
                    b.SetSummary(b.Const("Description"));
                    b.SetClass("p-4");
                },
                b.HtmlSpan(
                    b =>
                    {
                        b.SetInnerHtml(summaryHtml);
                    }));
        }

        private static Var<Func<T, string>> DefineGetId<T>(SyntaxBuilder b, Expression<Func<T, string>> IdProperty)
        {
            return b.Def((SyntaxBuilder b, Var<T> item) =>
            {
                return b.If(
                    b.HasObject(item),
                    b => b.Get(item, IdProperty),
                    b => b.Const(string.Empty));
            });
        }

        private static void AddStylesheet(this HtmlBuilder b)
        {
            StaticFiles.Add(typeof(ServiceDoc).Assembly, "Metapsi.ServiceDoc.css");

            b.HeadAppend(
                b.HtmlLink(
                    b =>
                    {
                        b.SetAttribute("rel", "stylesheet");
                        b.SetAttribute("href", "/Metapsi.ServiceDoc.css");
                    }));
        }


        public static Var<List<TItem>> FilterList<TItem>(
            this SyntaxBuilder b,
            Var<List<TItem>> list,
            Var<string> value)
        {
            var filteredItems = b.Get(
                list,
                value,
                b.Def<SyntaxBuilder, TItem, string, bool>(ContainsValue),
                (all, value, filterFunc) => all.Where(x => filterFunc(x, value)).ToList());

            return filteredItems;
        }

        public static Var<bool> ContainsValue<T>(this SyntaxBuilder b, Var<T> item, Var<string> value)
        {
            return b.Includes(b.ToLowercase(b.ConcatObjectValues(item)), b.ToLowercase(value));
        }

        public static Var<IVNode> DocsGrid<T>(
            this LayoutBuilder b, 
            Var<ListDocsPage<T>> model, 
            Expression<Func<T, string>> idProperty)
        {
            return
                b.HtmlDiv(
                    b =>
                    {
                        b.SetClass("p-4");
                    },
                    b.If(
                        b.Get(model, x => x.Documents.Any()),
                        b =>
                        {
                            var filteredRows = b.FilterList(b.Get(model, x => x.Documents), b.Get(model, x => x.FilterText));

                            var gridBuilder = DataGridBuilder.DataGrid<T>();
                            gridBuilder.AddRowAction((b, item) => b.EditDocumentButton(item, idProperty));
                            gridBuilder.AddRowAction((b, item) => b.DeleteDocumentButton(item, idProperty));
                            var dataGrid = b.DataGrid(gridBuilder, filteredRows, b.Get(model, x => x.Columns));

                            return b.HtmlDiv(
                                b.HtmlDiv(
                                    b =>
                                    {
                                        b.SetClass("hidden md:block");
                                    },
                                    dataGrid),
                                b.HtmlDiv(
                                    b =>
                                    {
                                        b.SetClass("flex flex-col gap-2 md:hidden");
                                    },
                                    b.Map(filteredRows, (b, document) =>
                                    {
                                        return b.SlCard(
                                            b.HtmlDiv(
                                                b =>
                                                {
                                                    b.SetClass("flex flex-col gap-2");
                                                },
                                                b.Map(
                                                    b.Get(model, x => x.Columns),
                                                    (b, column) =>
                                                    {
                                                        return b.HtmlDiv(
                                                            b =>
                                                            {
                                                                b.SetClass("flex flex-col");
                                                            },
                                                            b.HtmlSpanText(
                                                                b =>
                                                                {
                                                                    b.SetClass("text-sm text-gray-600");
                                                                },
                                                                column),
                                                            b.HtmlSpanText(
                                                                b =>
                                                                {
                                                                    b.SetClass("text-gray-800");
                                                                },
                                                                b.GetProperty<string>(document, column)));
                                                    })),
                                            b.HtmlDiv(
                                                b =>
                                                {
                                                    b.SetClass("flex flex-row items-center justify-end gap-2 pt-2");
                                                },
                                                b.EditDocumentButton<T>(document, idProperty),
                                                b.DeleteDocumentButton<T>(document, idProperty)));
                                    })));
                        },
                        b =>
                        {
                            return b.Text("There are no documents to display. Create some to see them here");
                        }));
        }


        public static Var<IVNode> EditDocumentButton<T>(this LayoutBuilder b, Var<T> document, Expression<Func<T, string>> IdProperty)
        {
            return b.SlIconButton(
                b =>
                {
                    b.SetName("pencil-square");
                    b.OnClickAction(
                        b.MakeAction((SyntaxBuilder b, Var<ServiceDoc.ListDocsPage<T>> model) =>
                        {
                            var editDocReference = b.Get(
                                model,
                                b.Get(document, IdProperty),
                                DefineGetId(b, IdProperty),
                                (model, id, getId) => model.Documents.Single(x => getId(x) == id));

                            b.Set(model, x => x.EditDocument, b.Clone(editDocReference));

                            var popup = b.GetElementById(b.Const(IdEditDocument));
                            b.SetDynamic(popup, DynamicProperty.Bool("open"), b.Const(true));
                            return b.Clone(model);

                        }));
                });
        }

        public static Var<IVNode> DeleteDocumentButton<T>(this LayoutBuilder b, Var<T> document, Expression<Func<T, string>> idProperty)
        {
            return b.SlIconButton(
                b =>
                {
                    b.SetName("trash");
                    b.OnClickAction(
                        b.MakeAction((SyntaxBuilder b, Var<ServiceDoc.ListDocsPage<T>> model) =>
                        {
                            var getId = DefineGetId(b, idProperty);
                            var editDocReference =
                            b.Get(
                                model,
                                b.Call(getId, document),
                                getId,
                                (model, id, getId) => model.Documents.Single(x => getId(x) == id));

                            b.Set(model, x => x.EditDocument, b.Clone(editDocReference));

                            var popup = b.GetElementById(b.Const(IdRemoveDocument));
                            b.SetDynamic(popup, DynamicProperty.Bool("open"), b.Const(true));
                            return b.Clone(model);

                        }));
                });
        }
    }
}