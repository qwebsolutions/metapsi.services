using Metapsi.Hyperapp;
using Metapsi.Syntax;
using Metapsi.Shoelace;
using System;
using System.Linq;
using System.Collections.Generic;
using static Metapsi.Hyperapp.HyperType;
using System.Linq.Expressions;
using Metapsi.Html;
using System.Reflection;

namespace Metapsi;

public static partial class ServiceDoc
{
    public class ListDocsPage<T>
    {
        public string InitApiUrl { get; set; }
        public string ListApiUrl { get; set; }
        public string SaveApiUrl { get; set; }
        public string DeleteApiUrl { get; set; }
        public List<T> Documents { get; set; } = new List<T>();
        public T EditDocument { get; set; }
        public SchemaType DocumentSchema { get; set; }
        public string SummaryHtml { get; set; }
        public List<string> Columns { get; set; } = new List<string>();
        public string FilterText { get; set; }
    }

    private const string IdEditDocument = "id-edit-document";
    private const string IdRemoveDocument = "id-remove-document";

    internal static void Render<T, TId>(HtmlBuilder b, ListDocsPage<T> model, Expression<Func<T, TId>> idProperty)
    {
        b.AddServiceDocStylesheet();
        b.HeadAppend(b.HtmlStyle(b.Text(".sl-toast-stack { left: 50%; transform: translateX(-50%); }")));
        b.BodyAppend(b.Hyperapp(model,
            (b, model) =>
            {
                return b.RenderDocumentsList(model, idProperty);
            }));
    }

    internal static Var<string> EntityName<T>(this SyntaxBuilder b)
    {
        return b.Const(typeof(T).Name);
    }

    public static Var<IVNode> RenderDocumentsList<T, TId>(this LayoutBuilder b, Var<ListDocsPage<T>> model, Expression<Func<T, TId>> idProperty)
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
            //b.EditDocumentPopup(model, idProperty),
            b.RemoveDocumentPopup<T, TId>(idProperty));
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

    //public static Reference<bool> isInRawEdit = new Reference<bool>() { Value = false };

    //public static Var<bool> IsInRawEdit(this SyntaxBuilder b)
    //{
    //    return b.GetRef(b.Const(isInRawEdit));
    //}

    //public static void SetRawEdit(this SyntaxBuilder b, Var<bool> value)
    //{
    //    b.SetRef(b.Const(isInRawEdit), value);
    //}

    //public static Var<IVNode> EditDocumentPopup<T, TId>(
    //    this LayoutBuilder b,
    //    Var<ServiceDoc.ListDocsPage<T>> model,
    //    Expression<Func<T, TId>> idProperty)
    //{
    //    var getId = DefineGetIdAsString(b, idProperty);
    //    var isNew = b.Get(model, getId, (model, getId) => model.EditDocument == null || !model.Documents.Any(x => getId(x) == getId(model.EditDocument)));
    //    var caption = b.If(isNew, x => b.Const("Create new "), b => b.Const("Edit "));

    //    //var buildContent = (LayoutBuilder b) => b.AutoEditForm(model, b.Get(model, x => x.EditDocument));
    //    var buildContent = (LayoutBuilder b) =>
    //    b.If(
    //        b.IsInRawEdit(),
    //        b =>
    //        {
    //            return b.RawEditTextArea(model);
    //        },
    //        b =>
    //        {
    //            return b.HtmlDiv(
    //                b =>
    //                {
    //                    b.AddClass("flex md:flex-row md:justify-between gap-4 flex-col items-stretch");
    //                },
    //                b.JsonEditor(b.JsonEditorGetRootNode()),
    //                b.JsonEditorPreview(b.JsonEditorGetRootNode()));
    //        });
    //    return b.SlDialog(
    //        b =>
    //        {
    //            b.AddStyle("--width", "800px");
    //            b.SetId(b.Const(IdEditDocument));
    //            //b.SetLabel());
    //        },
    //        b.Optional(
    //            b.HasObject(
    //                b.Get(model, x => x.EditDocument)),
    //                b =>
    //                {
    //                    return b.HtmlDiv(
    //                        b =>
    //                        {
    //                            b.SetSlot(SlDialog.Slot.Label);
    //                            b.AddClass("flex flex-row gap-8 items-center");
    //                        },
    //                        b.HtmlDiv(b.Text(b.Concat(caption, b.ToLowercase(b.FormatLabel(b.EntityName<T>()))))),
    //                        b.HtmlDiv(
    //                            b.SlCheckbox(
    //                                b =>
    //                                {
    //                                    b.SetChecked(b.IsInRawEdit());
    //                                    b.OnSlChange((SyntaxBuilder b, Var<ListDocsPage<T>> model, Var<Html.Event> e) =>
    //                                    {
    //                                        b.SetRawEdit(b.GetTargetChecked(e));
    //                                        return b.Clone(model);
    //                                    });
    //                                },
    //                                b.Text("Raw"))));
    //                }),
    //        b.Optional(
    //            b.HasObject(b.Get(model, x => x.EditDocument)),
    //            b => b.Call(buildContent)),
    //        b.If(
    //            b.IsInRawEdit(),
    //            b =>
    //            {
    //                // Raw edit does not show node options and does not extract json from editor
    //                return b.SlButton(
    //                    b =>
    //                    {
    //                        b.SetSlot(SlDialog.Slot.Footer);
    //                        b.SetVariantPrimary();

    //                        b.OnClickAction(b.MakeAction((SyntaxBuilder b, Var<ServiceDoc.ListDocsPage<T>> model) =>
    //                        {
    //                            return SaveDocument(b, model);
    //                        }));
    //                    },
    //                    b.Text("Save"));
    //            },
    //            b =>
    //            {
    //                return b.Optional(
    //                    b.HasObject(b.Get(model, x => x.EditDocument)),
    //                    b => b.HtmlDiv(
    //                        b =>
    //                        {
    //                            b.SetSlot("footer");
    //                            b.AddClass("flex md:flex-row md:justify-between md:items-center flex-col items-stretch");
    //                            //b.SetProperty(b.Props, b.Const("key"), b.Call(getId, b.Get(model, x => x.EditDocument)));
    //                        },
    //                        b.JsonEditorSelectedNodeOptions(),
    //                        b.SlButton(
    //                            b =>
    //                            {
    //                                b.SetVariantPrimary();

    //                                b.OnClickAction(b.MakeAction((SyntaxBuilder b, Var<ServiceDoc.ListDocsPage<T>> model) =>
    //                                {
    //                                    // Extracts the json from the editor
    //                                    b.Set(
    //                                        model, x => x.EditDocument,
    //                                        b.Deserialize<T>(b.JsonEditorGenerate(b.JsonEditorGetRootNode(), b.Const(0), b.Const(false))));
    //                                    b.Log(b.Get(model, x => x.EditDocument));

    //                                    return SaveDocument(b, model);
    //                                }));
    //                            },
    //                    b.Text("Save"))));
    //            }));
    //}

    //public static Var<IVNode> RawEditTextArea<T>(this LayoutBuilder b, Var<ListDocsPage<T>> model)
    //{
    //    var editDoc = b.Get(model, x => x.EditDocument);
    //    return b.Optional(
    //        b.HasObject(editDoc),
    //        b =>
    //        {
    //            var json = b.JsonStringify(
    //                editDoc, 
    //                b=>
    //                {
    //                    b.SetSpace(b.Const(2));
    //                });
    //            return b.SlTextarea(
    //                b =>
    //                {
    //                    b.SetRows(12);
    //                    b.SetValue(json);
    //                    b.OnSlChange(b.MakeAction((SyntaxBuilder b, Var<ListDocsPage<T>> model, Var<Html.Event> e) =>
    //                    {
    //                        b.Set(model, x => x.EditDocument, b.Deserialize<T>(b.GetTargetValue(e)));
    //                        return b.Clone(model);
    //                    }));
    //                });
    //        });
    //}



    public static Var<IVNode> RemoveDocumentPopup<T, TId>(
        this LayoutBuilder b,
        Expression<Func<T, TId>> idProperty)
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
                return b.MakeStateWithEffects(
                    model,
                    (b, dispatch) =>
                    {
                        var edit = b.EditDocumentPromise<T>(
                            result,
                            b.Get(model, x => x.DocumentSchema),
                            b.Const(DocumentEditor.EditAction.Create));
                        b.PromiseThen(edit, (SyntaxBuilder b, Var<object> updatedObject) =>
                        {
                            b.Dispatch(
                                dispatch,
                                b.MakeStateWithEffects(
                                    model,
                                    SaveDocumentEffect(b, updatedObject)));
                        });
                    });
            });

        var onError = b.MakeAction(
            (SyntaxBuilder b, Var<ServiceDoc.ListDocsPage<T>> model, Var<Html.Error> apiError) =>
            {
                b.Alert(b.Get(apiError, x => x.message));
                var editPopup = b.GetElementById(b.Const(IdEditDocument));
                b.SetProperty(editPopup, b.Const("open"), b.Const(false));

                var removePopup = b.GetElementById(b.Const(IdRemoveDocument));
                b.SetProperty(removePopup, b.Const("open"), b.Const(false));
                return b.Clone(model);
            });

        return b.MakeStateWithEffects(
            model,
            b.GetJsonEffect(
                b.Get(model, x => x.InitApiUrl),
                onResult,
                onError));
    }

    public static Var<HyperType.Effect> SaveDocumentEffect<T>(SyntaxBuilder b, Var<T> document)
    {
        var onResult = b.MakeAction(
            (SyntaxBuilder b, Var<ServiceDoc.ListDocsPage<T>> model, Var<SaveResult> saveResult) =>
            {
                b.ToastResult(b.Get(saveResult, x => x.Success), b.Get(saveResult, x => x.Message));
                return b.RefreshAllDocuments<T>();
            });

        var onError = b.MakeAction(
            (SyntaxBuilder b, Var<ServiceDoc.ListDocsPage<T>> model, Var<Html.Error> apiError) =>
            {
                b.Alert(b.Get(apiError, x => x.message));
                return b.RefreshAllDocuments<T>();
            });

        return b.MakeEffect(
            (b, dispatch) =>
            {
                b.Dispatch(dispatch, b.MakeAction((SyntaxBuilder b, Var<ServiceDoc.ListDocsPage<T>> model) =>
                {
                    return b.MakeStateWithEffects(
                        model,
                        b.PostJsonEffect(
                            b.Get(model, x => x.SaveApiUrl),
                            document,
                            onResult,
                            onError));
                }));
            });
    }

    public static Var<HyperType.StateWithEffects> SaveDocument<T>(SyntaxBuilder b, Var<ServiceDoc.ListDocsPage<T>> model)
    {
        var onResult = b.MakeAction(
            (SyntaxBuilder b, Var<ServiceDoc.ListDocsPage<T>> model, Var<SaveResult> saveResult) =>
            {
                b.ToastResult(b.Get(saveResult, x => x.Success), b.Get(saveResult, x => x.Message));
                var editPopup = b.GetElementById(b.Const(IdEditDocument));
                b.SetProperty(editPopup, b.Const("open"), b.Const(false));

                var removePopup = b.GetElementById(b.Const(IdRemoveDocument));
                b.SetProperty(removePopup, b.Const("open"), b.Const(false));

                b.Push(b.Get(model, x => x.Documents), b.Get(model, x => x.EditDocument));
                return b.RefreshAllDocuments<T>();
            });

        var onError = b.MakeAction(
            (SyntaxBuilder b, Var<ServiceDoc.ListDocsPage<T>> model, Var<Html.Error> apiError) =>
            {
                b.Alert(b.Get(apiError, x => x.message));
                var editPopup = b.GetElementById(b.Const(IdEditDocument));
                b.SetProperty(editPopup, b.Const("open"), b.Const(false));

                var removePopup = b.GetElementById(b.Const(IdRemoveDocument));
                b.SetProperty(removePopup, b.Const("open"), b.Const(false));
                return b.RefreshAllDocuments<T>();
            });
        
        return b.MakeStateWithEffects(
            model,
            b.PostJsonEffect(
                b.Get(model, x => x.SaveApiUrl),
                b.Get(model, x => x.EditDocument),
                onResult,
                onError));
    }


    public static Var<HyperType.StateWithEffects> RemoveDocument<T, TId>(
        SyntaxBuilder b,
        Var<ServiceDoc.ListDocsPage<T>> model,
        Expression<Func<T, TId>> idProperty)
    {
        var getId = DefineGetIdAsString(b, idProperty);

        var onResult = b.MakeAction(
            (SyntaxBuilder b, Var<ServiceDoc.ListDocsPage<T>> model, Var<DeleteResult> deleteResult) =>
            {
                b.ToastResult(b.Get(deleteResult, x => x.Success), b.Get(deleteResult, x => x.Message));

                var editPopup = b.GetElementById(b.Const(IdEditDocument));
                b.SetProperty(editPopup, b.Const("open"), b.Const(false));

                var removePopup = b.GetElementById(b.Const(IdRemoveDocument));
                b.SetProperty(removePopup, b.Const("open"), b.Const(false));

                return b.RefreshAllDocuments<T>();
            });

        var onError = b.MakeAction(
            (SyntaxBuilder b, Var<ServiceDoc.ListDocsPage<T>> model, Var<Html.Error> apiError) =>
            {
                b.Alert(b.Get(apiError, x => x.message));
                var editPopup = b.GetElementById(b.Const(IdEditDocument));
                b.SetProperty(editPopup, b.Const("open"), b.Const(false));

                var removePopup = b.GetElementById(b.Const(IdRemoveDocument));
                b.SetProperty(removePopup, b.Const("open"), b.Const(false));
                return b.RefreshAllDocuments<T>();
            });

        return b.MakeStateWithEffects(
            model,
            b.PostJsonEffect(
                b.Get(model, x => x.DeleteApiUrl),
                b.Get(model, x => x.EditDocument),
                onResult,
                onError));
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
                (SyntaxBuilder b, Var<ServiceDoc.ListDocsPage<T>> model, Var<Html.Error> apiError) =>
                {
                    b.Alert(b.Get(apiError, x => x.message));
                    return b.Clone(model);
                });

            return b.MakeStateWithEffects(
                model,
                b.GetJsonEffect(
                    b.Get(model, x => x.ListApiUrl),
                    onResult,
                    onError));
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

    private static Var<Func<T, string>> DefineGetIdAsString<T, TId>(SyntaxBuilder b, Expression<Func<T, TId>> IdProperty)
    {
        return b.Def((SyntaxBuilder b, Var<T> item) =>
        {
            return b.If(
                b.HasObject(item),
                b => b.AsString(b.Get(item, IdProperty)),
                b => b.Const(string.Empty));
        });
    }

    private static void AddServiceDocStylesheet(this HtmlBuilder b)
    {
        var embeddedCss = b.Document.Metadata.AddEmbeddedResourceMetadata(typeof(Metapsi.ServiceDoc).Assembly, "Metapsi.ServiceDoc.css");
        //StaticFiles.Add(typeof(ServiceDoc).Assembly, "Metapsi.ServiceDoc.css");

        var link = new HtmlTag("link");
        link.SetAttribute("rel", "stylesheet");
        link.SetAttribute("href", embeddedCss);

        b.HeadAppend(new HtmlNode()
        {
            Tags = new List<HtmlTag>() { link }
        });
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

    public class EnumProperty
    {
        public string PropertyName { get; set; }
        public string PropertyType { get; set; }
        public List<EnumValue> EnumValues { get; set; } = new List<EnumValue>();
    }

    public class EnumValue
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }

    public static Var<string> GetEnumValueName<T>(this SyntaxBuilder b, Var<string> columnName, Var<int> value)
    {
        List<EnumProperty> enumProperties = new List<EnumProperty>();
        foreach (var enumProperty in LocalControls.GetEnumProperties(typeof(T)))
        {
            EnumProperty toAdd = new EnumProperty()
            {
                PropertyName = enumProperty.Name,
                PropertyType = enumProperty.PropertyType.Name
            };
            var enumType = enumProperty.PropertyType;
            var enumValues = enumType.GetEnumValues();
            for (int i = 0; i < enumValues.Length; i++)
            {
                toAdd.EnumValues.Add(new EnumValue()
                {
                    Name = enumValues.GetValue(i).ToString(),
                    Value = i
                });
            }
            enumProperties.Add(toAdd);
        }

        var allEnums = b.Const(enumProperties);

        var propertyValues = b.Get(
            allEnums,
            columnName,
            (allProperties, columnName) => allProperties.Single(x => x.PropertyName == columnName));

        var valueName = b.Get(propertyValues, value, (propertyValues, value) => propertyValues.EnumValues.SingleOrDefault(x => x.Value == value));
        return b.If(
            b.HasObject(valueName),
            b => b.Get(valueName, x => x.Name),
            b => b.Const("[Invalid enum]"));
    }

    public static Var<Func<T, string, IVNode>> DefGetReadOnlyControl<T>(this SyntaxBuilder b)
    {
        return b.Def((LayoutBuilder b, Var<T> document, Var<string> column) =>
        {
            var boolProperties = LocalControls.GetBoolProperties(typeof(T));
            var enumProperties = LocalControls.GetEnumProperties(typeof(T));

            var boolPropertyNames = b.Const(boolProperties.Select(x => x.Name).ToList());
            var enumPropertyNames = b.Const(enumProperties.Select(x => x.Name).ToList());

            return b.If(
                b.ContainsValue(boolPropertyNames, column),
            b =>
            {
                var value = b.GetProperty<bool>(document, column);
                return b.SlCheckbox(
                    b =>
                    {
                        b.SetSizeSmall();
                        b.SetDisabled();
                        b.SetChecked(value);
                    });
            },
            b => b.If(
                b.ContainsValue(enumPropertyNames, column),
                b =>
                {
                    var value = b.GetProperty<int>(document, column);
                    var name = b.Call(GetEnumValueName<T>, column, value);
                    return b.Text(b.Concat(name, b.Const(" ("), b.AsString(value), b.Const(")")));
                },
                    b =>
                    {
                        return b.Text(b.GetProperty<string>(document, column));
                    }));
        });
    }

    public static Var<IVNode> DocsGrid<T, TId>(
        this LayoutBuilder b,
        Var<ListDocsPage<T>> model,
        Expression<Func<T, TId>> idProperty)
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

                        foreach (var boolProperty in LocalControls.GetBoolProperties(typeof(T)))
                        {
                            gridBuilder.DataTableBuilder.OverrideDataCell(
                                boolProperty.Name,
                                (b, entity) =>
                                {
                                    var value = b.GetProperty<bool>(entity, boolProperty.Name);
                                    return b.SlCheckbox(
                                        b =>
                                        {
                                            b.SetDisabled();
                                            b.SetChecked(value);
                                        });
                                });
                        }

                        foreach (var enumProperty in LocalControls.GetEnumProperties(typeof(T)))
                        {
                            gridBuilder.DataTableBuilder.OverrideDataCell(
                                enumProperty.Name,
                                (b, entity) =>
                                {
                                    var value = b.GetProperty<int>(entity, enumProperty.Name);
                                    var name = b.Call(GetEnumValueName<T>, b.Const(enumProperty.Name), value);
                                    return b.Text(b.Concat(name, b.Const(" ("), b.AsString(value), b.Const(")")));
                                });
                        }

                        foreach (var collectionProperty in LocalControls.GetCollectionProperties(typeof(T)))
                        {
                            gridBuilder.DataTableBuilder.OverrideDataCell(
                                collectionProperty.Name,
                                (b, entity) =>
                                {
                                    var collection = b.GetProperty<List<object>>(entity, collectionProperty.Name);
                                    var length = b.Get(collection, x => x.Count());
                                    return b.Text(b.Concat(b.AsString(length), b.Const(" items")));
                                });
                        }

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
                                                        b.HtmlSpan(
                                                            b =>
                                                            {
                                                                b.SetClass("text-gray-800");
                                                            },
                                                            b.Call(b.DefGetReadOnlyControl<T>(), document, column)));
                                                })),
                                        b.HtmlDiv(
                                            b =>
                                            {
                                                b.SetClass("flex flex-row items-center justify-end gap-2 pt-2");
                                            },
                                            b.EditDocumentButton(document, idProperty),
                                            b.DeleteDocumentButton(document, idProperty)));
                                })));
                    },
                    b =>
                    {
                        return b.Text("There are no documents to display. Create some to see them here");
                    }));
    }


    public static Var<IVNode> EditDocumentButton<T, TId>(this LayoutBuilder b, Var<T> document, Expression<Func<T, TId>> IdProperty)
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
                            b.AsString(b.Get(document, IdProperty)),
                            DefineGetIdAsString(b, IdProperty),
                            (model, id, getId) => model.Documents.Single(x => getId(x) == id));

                        //b.Set(model, x => x.EditDocument, b.Clone(editDocReference));
                        ////b.SetJsonEditorRoot(b.Get(model, x => x.DocumentSchema), b.Get(model, x => x.EditDocument));

                        //b.Throw(b.Const("Not implemented!"));

                        //var popup = b.GetElementById(b.Const(IdEditDocument));
                        //b.SetProperty(popup, b.Const("open"), b.Const(true));
                        //return b.Clone(model);

                        return b.MakeStateWithEffects(
                            model,
                            (b, dispatch) =>
                            {
                                var edit = b.EditDocumentPromise<T>(
                                    editDocReference,
                                    b.Get(model, x => x.DocumentSchema),
                                    b.Const(DocumentEditor.EditAction.Edit));
                                b.PromiseThen(edit, (SyntaxBuilder b, Var<object> updatedObject) =>
                                {
                                    b.Dispatch(
                                        dispatch,
                                        b.MakeStateWithEffects(
                                            model,
                                            SaveDocumentEffect(b, updatedObject)));
                                });
                            });

                    }));
            });
    }

    public static Var<IVNode> DeleteDocumentButton<T, TId>(this LayoutBuilder b, Var<T> document, Expression<Func<T, TId>> idProperty)
    {
        return b.SlIconButton(
            b =>
            {
                b.SetName("trash");
                b.OnClickAction(
                    b.MakeAction((SyntaxBuilder b, Var<ServiceDoc.ListDocsPage<T>> model) =>
                    {
                        var getId = DefineGetIdAsString(b, idProperty);
                        var editDocReference =
                        b.Get(
                            model,
                            b.Call(getId, document),
                            getId,
                            (model, id, getId) => model.Documents.Single(x => getId(x) == id));

                        b.Set(model, x => x.EditDocument, b.Clone(editDocReference));
                        //b.SetJsonEditorRoot(b.Get(model, x => x.DocumentSchema), b.Get(model, x => x.EditDocument));
                        b.Throw(b.Const("Not implemented!"));

                        var popup = b.GetElementById(b.Const(IdRemoveDocument));
                        b.SetProperty(popup, b.Const("open"), b.Const(true));
                        return b.Clone(model);

                    }));
            });
    }

    public static void ToastResult(this SyntaxBuilder b, Var<bool> succes, Var<string> message)
    {
        b.SlToast(b =>
        {
            b.SetDuration(5000);
            b.SetClosable();
            b.If(
                succes,
                b => b.SetVariantSuccess(),
                b => b.SetVariantDanger());
        },
        b.CreateElement<SlIcon>(
            "sl-icon",
            b =>
            {
                b.SetSlot(SlAlert.Slot.Icon);
                b.SetName(
                    b.If(
                        succes,
                        b => b.Const("check2-circle"),
                        b => b.Const("exclamation-octagon")));
            }),
        b.CreateTextNode(message));
    }

    //public static void SetJsonEditorRoot<T>(this SyntaxBuilder b, Var<SchemaType> documentSchema, Var<T> edited)
    //{
    //    var rootNode = b.JsonEditorCreateNodeFromData(
    //        documentSchema,
    //        edited.As<object>(),
    //        b.Const("-"),
    //        b.Const("r"));

    //    b.SetRef(b.Const(JsonEditorExtensions.JsonEditorRootDataNode), rootNode);
    //}
}