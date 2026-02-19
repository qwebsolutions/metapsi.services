using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Shoelace;
using Metapsi.Syntax;
using System.Drawing;
using static Metapsi.Hyperapp.HyperType;

namespace Metapsi;

public static class DocumentEditor
{
    const string EditDialogModalId = "id-edit-dialog-modal";
    const string IdEditDialogAppDiv = "id-edit-dialog-app";

    public enum EditAction
    {
        Create,
        Edit
    }

    public class Model
    {
        //public System.Func<object, string> GetIdFn { get; set; }
        //public string DocumentJson { get; set; }
        //public object Document { get; set; }

        public string RawJson { get; set; }

        public EditAction EditAction { get; set; }
        public bool IsInRawEditMode { get; set; }
        public JsonEditorDataNode RootNode { get; set; }
        public JsonEditorDataNode SelectedNode { get; set; }
        public string JsonError { get; set; } = string.Empty;
        public string EntityName { get; set; }
    }

    public static Var<Promise> EditDocumentPromise<TDocument>(
        this SyntaxBuilder b,
        Var<TDocument> document,
        Var<SchemaType> schema,
        Var<EditAction> editAction)
    {
        return b.NewPromise<object, object>(
            (SyntaxBuilder b, Var<System.Action<object>> resolve, Var<System.Action<object>> reject) =>
            {
                var rootNode = b.CreateRootNode(schema, document);

                var model = b.NewObj<Model>(
                    b =>
                    {
                        //b.Set(x => x.DocumentJson, b.Serialize(document));
                        b.Set(x => x.EditAction, editAction);
                        b.Set(x => x.EntityName, b.EntityName<TDocument>());
                        b.Set(x => x.RootNode, rootNode);
                        //b.Set(x => x.SelectedNode, rootNode);
                    });

                var existingNode = b.QuerySelector("#" + IdEditDialogAppDiv);
                var attachNode = b.If(
                    b.HasObject(existingNode),
                    b => existingNode,
                    b =>
                    {
                        return b.NodeAppendChild(
                            b.Get(b.Document(), x => x.body),
                            b.CreateElement<HtmlDiv>(
                                "div",
                                b =>
                                {
                                    b.SetId(IdEditDialogAppDiv);
                                }));
                    });
                b.RequestAnimationFrame(
                    b =>
                    {
                        b.Hyperapp(
                            b.NewObj<HyperType.App<Model>>(
                                b =>
                                {
                                    b.Set(x => x.init, b.MakeInit(model));
                                    b.Set(x => x.node, attachNode);
                                    b.Set(
                                        x => x.view,
                                        b.Def(
                                            (LayoutBuilder b, Var<Model> model)=>
                                            {
                                                return b.Call(OnRender, model, resolve);
                                            }));
                                }));
                    });
            });
    }


    private static Var<HyperType.Effect> ShutdownEffect(this SyntaxBuilder b)
    {
        return b.MakeEffect(
            (b, dispatch) =>
            {
                b.RequestAnimationFrame(
                    b =>
                    {
                        b.Dispatch(dispatch, b.GetProperty<object>(b.Self(), "undefined").As<HyperType.Action<Model>>());
                        var dialog = b.QuerySelector("#" + EditDialogModalId);
                        b.If(
                            b.HasObject(dialog),
                            b =>
                            {
                                b.CallOnObject(dialog, "remove");
                            });
                    });
            });
    }

    public static Var<IVNode> OnRender(LayoutBuilder b, Var<Model> model, Var<System.Action<object>> onEdited)
    {
        //var getId = b.Get(model, x => x.GetIdFn);
        var caption = b.If(b.Get(model, x => x.EditAction == EditAction.Create), x => b.Const("Create new "), b => b.Const("Edit "));

        var buildContent = (LayoutBuilder b) =>
        b.If(
            b.Get(model, x => x.IsInRawEditMode),
            b =>
            {
                return RawEditTextArea<Model>(b, model);
            },
            b =>
            {
                return b.HtmlDiv(
                    b =>
                    {
                        b.AddClass("flex md:flex-row md:justify-between gap-4 flex-col items-stretch");
                    },
                    b.JsonEditor(
                        b.Get(model, x => x.RootNode),
                        b.MakeAction((SyntaxBuilder b, Var<Model> model, Var<JsonEditorDataNode> selectedNode) =>
                        {
                            b.Log("Selected node changed", selectedNode);
                            b.Set(model, x => x.SelectedNode, selectedNode);
                            return b.Clone(model);
                        })),
                    b.JsonEditorJsonPreview(
                        b.Get(model, x => x.RootNode),
                        b.Get(model, x => x.SelectedNode)));
            });
        var dialog = b.SlDialog(
            b =>
            {
                b.AddStyle("--width", "800px");
                b.SetId(b.Const(EditDialogModalId));
                b.SetOpen(true);

                b.OnEventAction("sl-request-close", b.MakeAction((SyntaxBuilder b, Var<Model> model, Var<CustomEvent<SlRequestCloseDetail>> e) =>
                {
                    var fromXButton = b.Get(e, x => x.detail.source == "close-button");

                    return b.If(
                        fromXButton,
                        b =>
                        {
                            return b.MakeStateWithEffects(
                                model,
                                (b, dispatch) =>
                                {
                                    b.If(
                                        b.Get(e, x => x.detail.source != "overlay"),
                                        b =>
                                        {
                                            b.Dispatch(
                                                dispatch,
                                                b.MakeStateWithEffects(model,
                                                b.ShutdownEffect()));
                                        });
                                });
                        },
                        b =>
                        {
                            b.PreventDefault(e);
                            return b.MakeStateWithEffects(model);
                        });
                }));
            },
            b.HtmlDiv(
                b =>
                {
                    b.SetSlot(SlDialog.Slot.Label);
                    b.AddClass("flex flex-row gap-8 items-center");
                },
                b.HtmlDiv(b.Text(b.Concat(caption, b.ToLowercase(b.FormatLabel(b.Get(model, x => x.EntityName)))))),
                b.HtmlDiv(
                    b.SlCheckbox(
                        b =>
                        {
                            //b.BindTo(model, x => x.IsInRawEditMode);
                            b.OnSlChange(b.MakeAction((SyntaxBuilder b, Var<Model> model, Var<Html.Event> e) =>
                            {
                                var toRawJson = b.GetTargetChecked(e);
                                b.If(
                                    toRawJson,
                                    b =>
                                    {
                                        var json = b.JsonEditorGenerate(b.Get(model, x => x.RootNode), b.Const(0), b.Const(false));
                                        b.Set(model, x => x.RawJson, json);
                                        b.Set(model, x => x.IsInRawEditMode, true);
                                    },
                                    b =>
                                    {
                                        var _ = b.TryCatchReturn(
                                            b.Def((SyntaxBuilder b) =>
                                            {
                                                var obj = b.JsonParse(b.Get(model, x => x.RawJson));
                                                var schema = b.Get(model, x => x.RootNode.SchemaType);
                                                var rootNode = b.CreateRootNode(schema, obj);
                                                b.Set(model, x => x.RootNode, rootNode);
                                                b.Set(model, x => x.IsInRawEditMode, false);
                                                b.Set(model, x => x.RawJson, string.Empty);
                                                b.Set(model, x => x.JsonError, string.Empty);
                                                return b.Const(0);
                                            }),
                                            b.Def((SyntaxBuilder b, Var<object> error) =>
                                            {
                                                b.Set(model, x => x.JsonError, b.Get(error.As<Html.Error>(), x => x.message));
                                                b.Set(model, x => x.IsInRawEditMode, true);
                                                return b.Const(0);
                                            }));
                                    });

                                return b.Clone(model);
                            }));
                            b.SetChecked(b.Get(model, x => x.IsInRawEditMode));
                        },
                        b.Text("Raw")))),
            b.Call(buildContent),
            //b.Optional(
            //    b.HasObject(b.Get(model, x => x.SelectedNode)),
            //    b => b.Call(buildContent)),
            b.If(
                b.Get(model, x => x.IsInRawEditMode),
                b =>
                {
                    // Raw edit does not show node options and does not extract json from editor
                    return b.SlButton(
                        b =>
                        {
                            b.SetSlot(SlDialog.Slot.Footer);
                            b.SetVariantPrimary();
                            b.SetDisabled(b.HasValue(b.Get(model, x => x.JsonError)));

                            b.OnClickAction(b.MakeAction((SyntaxBuilder b, Var<Model> model) =>
                            {
                                var editedObject = b.JsonParse(b.Get(model, x => x.RawJson));
                                return b.MakeStateWithEffects(
                                    model,
                                    (b, dispatch) =>
                                    {
                                        var modal = b.QuerySelector("#" + EditDialogModalId);
                                        b.CallOnObject(modal, SlDialog.Method.Hide);
                                        b.Call(onEdited, editedObject);
                                        b.Dispatch(dispatch, b.MakeStateWithEffects(model, b.ShutdownEffect()));
                                    });
                            }));
                        },
                        b.Text("Save"));
                },
                b =>
                {
                    return b.HtmlDiv(
                            b =>
                            {
                                b.SetSlot("footer");
                                b.AddClass("flex md:flex-row md:justify-between md:items-center flex-col items-stretch");
                                //b.SetProperty(b.Props, b.Const("key"), b.Call(getId, b.Get(model, x => x.EditDocument)));
                            },
                            b.JsonEditorSelectedNodeOptions(b.Get(model, x => x.SelectedNode)),
                            b.SlButton(
                                b =>
                                {
                                    b.SetVariantPrimary();

                                    b.OnClickAction(b.MakeAction((SyntaxBuilder b, Var<Model> model) =>
                                    {
                                        var editedObject = b.Deserialize<object>(
                                            b.JsonEditorGenerate(b.Get(model, x => x.RootNode), b.Const(0), b.Const(false)));
                                        return b.MakeStateWithEffects(
                                            model,
                                            (b, dispatch) =>
                                            {
                                                var modal = b.QuerySelector("#" + EditDialogModalId);
                                                b.CallOnObject(modal, SlDialog.Method.Hide);
                                                b.Call(onEdited, editedObject);
                                                b.Dispatch(dispatch, b.MakeStateWithEffects(model, b.ShutdownEffect()));
                                            });
                                    }));
                                },
                        b.Text("Save")));
                }));

        return b.HtmlDiv(
            b =>
            {
                b.SetId(IdEditDialogAppDiv);
            },
            dialog);
    }

    public static Var<IVNode> RawEditTextArea<T>(LayoutBuilder b, Var<Model> model)
    {
        return
            b.HtmlDiv(
                b.SlTextarea(
                b =>
                {
                    b.SetRows(12);
                    b.OnSlInput(b.MakeAction((SyntaxBuilder b, Var<Model> model, Var<Event> e) =>
                    {
                        var newValue = b.GetTargetValue(e);
                        b.Set(model, x => x.RawJson, newValue);

                        return b.TryCatchReturn(
                            b.Def(
                                (SyntaxBuilder b) =>
                                {
                                    var parsedOk = b.JsonParse(newValue);
                                    b.Set(model, x => x.JsonError, string.Empty);
                                    //var schemaType = b.Get(model, x => x.RootNode.SchemaType);
                                    //var newRoot = b.CreateRootNode(schemaType, );
                                    //b.Set(model, x => x.RootNode, newRoot);
                                    return b.Clone(model);
                                }),
                            b.Def(
                                (SyntaxBuilder b, Var<object> error) =>
                                {
                                    b.Set(model, x => x.JsonError, b.Get(error.As<Html.Error>(), x => x.message));
                                    return b.Clone(model);
                                }));
                    }));

                    b.SetValue(b.Get(model, x => x.RawJson));
                }),
                b.HtmlDiv(
                    b =>
                    {
                        b.AddClass("text-red-500");
                    },
                    b.Text(b.Get(model, x => x.JsonError))));
    }


    public static Var<JsonEditorDataNode> CreateRootNode<T>(this SyntaxBuilder b, Var<SchemaType> documentSchema, Var<T> edited)
    {
        var rootNode = b.JsonEditorCreateNodeFromData(
            documentSchema,
            edited.As<object>(),
            b.Const("-"),
            b.Const("r"));

        return rootNode;
    }
}
