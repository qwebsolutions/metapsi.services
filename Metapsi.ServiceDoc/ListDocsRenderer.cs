﻿using Metapsi.Hyperapp;
using Metapsi.Syntax;
using Microsoft.AspNetCore.Http;
using Metapsi.Shoelace;
using Metapsi.Dom;
using System;
using System.Linq;
using System.Collections.Generic;
using Metapsi.Ui;
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
        }

        private const string IdEditDocument = "id-edit-document";
        private const string IdRemoveDocument = "id-remove-document";

        public static void Render<T>(HtmlBuilder b, ListDocsPage<T> model, Expression<Func<T, string>> idProperty)
        {
            b.AddStylesheet();
            b.Document.Body.SetAttribute("class", "fixed top-0 right-0 left-0 bottom-0");
            b.BodyAppend(b.Hyperapp(model,
                (b, model) =>
                {
                    return b.RenderClient(model, idProperty);
                }));
        }

        public static void Render(HtmlBuilder b, DocsOverviewModel model)
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
                        b.Map(b.Get(model, x => x.DocServices), (b, service) =>
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
                                        b.Text(b.AsString(b.Get(service, x => x.Count))))));
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
                b.HtmlDiv(
                    b =>
                    {
                        b.SetClass("flex relative flex-row items-center justify-center py-4 text-lg bg-gray-100 text-gray-700");
                    },
                    b.Text(b.Concat(b.FormatLabel(b.Const(EntityName)), b.Const(" Service Overview"))),
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
                            }))),
                b.Optional(
                    b.HasValue(b.Get(model, x => x.SummaryHtml)),
                    b => b.DocDescriptionPanel(b.Get(model, x => x.SummaryHtml))),
                b.DocsGrid(model, idProperty),
                b.EditDocumentPopup(model, idProperty),
                b.RemoveDocumentPopup<T>(idProperty));
        }

        public static Var<IVNode> EditDocumentPopup<T>(
            this LayoutBuilder b,
            Var<ServiceDoc.ListDocsPage<T>> model,
            Expression<Func<T, string>> idProperty)
        {
            var getId = DefineGetId(b, idProperty);
            var isNew = b.Get(model, getId, (model, getId) => model.EditDocument == null || !model.Documents.Any(x => getId(x) == getId(model.EditDocument)));
            var caption = b.If(isNew, x => b.Const("Create new "), b => b.Const("Edit "));

            var buildContent = (LayoutBuilder b) =>
            b.HtmlDiv(
                b =>
                {
                    b.SetClass("flex flex-col gap-4");
                },
                b.AutoEditForm(model, b.Get(model, x => x.EditDocument)),
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
                            b.SetVariantPrimary();

                            b.OnClickAction(b.MakeAction((SyntaxBuilder b, Var<ServiceDoc.ListDocsPage<T>> model) =>
                            {
                                return SaveDocument(b, model);
                            }));
                        },
                        b.Text("Save")))
                );

            return b.SlDialog(
                b =>
                {
                    b.SetId(b.Const(IdEditDocument));
                    b.SetLabel(b.Concat(caption, b.ToLowercase(b.FormatLabel(b.EntityName<T>()))));
                },
                b.Optional(
                    b.HasObject(b.Get(model, x => x.EditDocument)),
                    b => b.Call(buildContent)));
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
                (SyntaxBuilder b, Var<HyperType.Dispatcher<ListDocsPage<T>>> dispatch) =>
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
                (SyntaxBuilder b, Var<HyperType.Dispatcher<ListDocsPage<T>>> dispatch) =>
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
                (SyntaxBuilder b, Var<HyperType.Dispatcher<ListDocsPage<T>>> dispatch) =>
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
                    (SyntaxBuilder b, Var<HyperType.Dispatcher<ListDocsPage<T>>> dispatch) =>
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

        public static Var<IVNode> DocsGrid<T>(this LayoutBuilder b, Var<ListDocsPage<T>> model, Expression<Func<T, string>> idProperty)
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
                            var gridBuilder = DataGridBuilder.DataGrid<T>();
                            gridBuilder.AddRowAction((b, item) => b.EditDocumentButton(item, idProperty));
                            gridBuilder.AddRowAction((b, item) => b.DeleteDocumentButton(item, idProperty));
                            return b.DataGrid(gridBuilder, b.Get(model, x => x.Documents));
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