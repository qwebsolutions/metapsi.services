using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Shoelace;
using Metapsi.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metapsi;

public class JsonEditorDataNode
{
    public string Id { get; set; }
    public string Name { get; set; }
    public SchemaType SchemaType { get; set; }

    public JsonEditorNodeStringProperties StringProperties { get; set; }
    public JsonEditorNodeBoolProperties BoolProperties { get; set; }
    public JsonEditorNodeIntProperties IntProperties { get; set; }
    public JsonEditorNodeNumberProperties NumberProperties { get; set; }
    public JsonEditorNodeObjectProperties ObjectProperties { get; set; }
    public JsonEditorNodeArrayProperties ArrayProperties { get; set; }
}

public class JsonEditorNodeObjectProperties
{
    public bool IsPresent { get; set; } = true;
    // Is either null or has an object. 
    // The actual data inside the object are the children nodes
    public bool IsNull { get; set; }
    public List<JsonEditorDataNode> Properties { get; set; } = new();
}

public class JsonEditorNodeStringProperties
{
    public bool IsPresent { get; set; } = true;
    public bool IsNull { get; set; }
    public string Value { get; set; } = string.Empty;
}

public class JsonEditorNodeIntProperties
{
    public bool IsPresent { get; set; } = true;
    public int Value { get; set; }
}

public class JsonEditorNodeNumberProperties
{
    public bool IsPresent { get; set; } = true;
    public decimal Value { get; set; }
}

public class JsonEditorNodeBoolProperties
{
    public bool IsPresent { get; set; } = true;
    public bool Value { get; set; }
}

public class JsonEditorNodeArrayProperties
{
    public bool IsPresent { get; set; } = true;
    public bool IsNull { get; set; }
    public List<JsonEditorDataNode> Items { get; set; } = new();
}

public static partial class JsonEditorExtensions
{
    public static JsonEditorDataNode JsonEditorEmptyDataNode { get; set; } = new() { Id = string.Empty };
    public static Reference<JsonEditorDataNode> JsonEditorRootDataNode { get; set; } = new Reference<JsonEditorDataNode>();
    public static Reference<JsonEditorDataNode> JsonEditorSelectedDataNode { get; set; } = new Reference<JsonEditorDataNode>();

    public static Var<JsonEditorDataNode> CreateObjectNode(
        this SyntaxBuilder b,
        Var<DynamicObject> data,
        Var<SchemaType> objectType,
        Var<string> nodeName,
        Var<string> nodeId)
    {
        var objectProperties = b.NewObj<JsonEditorNodeObjectProperties>(
            b =>
            {
                b.Set(x => x.IsPresent, b.Not(b.IsUndefined(data)));
                b.Set(x => x.IsNull, b.IsNull(data));
            });

        b.If(
            b.HasObject(data),
            b =>
            {

                b.Foreach(
                    b.Get(objectType, x => x.properties),
                    (b, prop) =>
                    {
                        var propName = b.Get(prop, x => x.name);
                        var childId = b.Concat(nodeId, b.Const("/"), propName);
                        var childData = b.GetProperty<DynamicObject>(data, propName);
                        b.Push(
                            b.Get(objectProperties, x => x.Properties),
                            b.Call(
                                JsonEditorCreateNodeFromData,
                                b.Get(prop, x => x.type),
                                childData,
                                propName,
                                childId));
                    });
            });

        return b.NewObj<JsonEditorDataNode>(
            b =>
            {
                b.Set(x => x.Id, nodeId);
                b.Set(x => x.Name, nodeName);
                b.Set(x => x.SchemaType, objectType);
                b.Set(x => x.ObjectProperties, objectProperties);
            });
    }

    public static Var<JsonEditorDataNode> CreateArrayNode(
        this SyntaxBuilder b,
        Var<DynamicObject> data,
        Var<SchemaType> objectType,
        Var<string> nodeName,
        Var<string> nodeId)
    {
        var arrayProperties = b.NewObj<JsonEditorNodeArrayProperties>(
            b =>
            {
                b.Set(x => x.IsPresent, b.Not(b.IsUndefined(data)));
                b.Set(x => x.IsNull, b.IsNull(data));
            });

        b.If(
            b.HasObject(data),
            b =>
            {
                var itemType = b.Get(objectType, x => x.items);
                var currentIndex = b.Ref(b.Const(0));
                b.Foreach(
                    data.As<List<DynamicObject>>(),
                    (b, item) =>
                    {
                        var childId = b.Concat(nodeId, b.Const("/"), b.AsString(b.GetRef(currentIndex)));
                        b.Push(
                            b.Get(arrayProperties, x => x.Items),
                            b.Call(
                                JsonEditorCreateNodeFromData,
                                itemType,
                                item,
                                b.AsString(b.GetRef(currentIndex)),
                                childId));

                        b.SetRef(currentIndex, b.Get(b.GetRef(currentIndex), x => x + 1));
                    });
            });

        return b.NewObj<JsonEditorDataNode>(
            b =>
            {
                b.Set(x => x.Id, nodeId);
                b.Set(x => x.Name, nodeName);
                b.Set(x => x.SchemaType, objectType);
                b.Set(x => x.ArrayProperties, arrayProperties);
            });
    }

    public static Var<JsonEditorDataNode> CreateStringNode(
        this SyntaxBuilder b,
        Var<DynamicObject> data,
        Var<SchemaType> schemaType,
        Var<string> nodeName,
        Var<string> nodeId)
    {
        var stringProperties = b.NewObj<JsonEditorNodeStringProperties>(
            b =>
            {
                b.Set(x => x.IsPresent, b.Not(b.IsUndefined(data)));
                b.Set(x => x.IsNull, b.IsNull(data));
                b.Set(x => x.Value, data.As<string>());
            });

        return b.NewObj<JsonEditorDataNode>(
            b =>
            {
                b.Set(x => x.Id, nodeId);
                b.Set(x => x.Name, nodeName);
                b.Set(x => x.SchemaType, schemaType);
                b.Set(x => x.StringProperties, stringProperties);
            });
    }

    public static Var<JsonEditorDataNode> CreateBoolNode(
        this SyntaxBuilder b,
        Var<DynamicObject> data,
        Var<SchemaType> schemaType,
        Var<string> nodeName,
        Var<string> nodeId)
    {
        var boolProperties = b.NewObj<JsonEditorNodeBoolProperties>(
            b =>
            {
                b.Set(x => x.IsPresent, b.Not(b.IsUndefined(data)));
                b.Set(x => x.Value, data.As<bool>());
            });

        return b.NewObj<JsonEditorDataNode>(
            b =>
            {
                b.Set(x => x.Id, nodeId);
                b.Set(x => x.Name, nodeName);
                b.Set(x => x.SchemaType, schemaType);
                b.Set(x => x.BoolProperties, boolProperties);
            });
    }

    public static Var<JsonEditorDataNode> CreateIntNode(
        this SyntaxBuilder b,
        Var<DynamicObject> data,
        Var<SchemaType> schemaType,
        Var<string> nodeName,
        Var<string> nodeId)
    {
        var intProperties = b.NewObj<JsonEditorNodeIntProperties>(
            b =>
            {
                b.Set(x => x.IsPresent, b.Not(b.IsUndefined(data)));
                b.Set(x => x.Value, data.As<int>());
            });

        return b.NewObj<JsonEditorDataNode>(
            b =>
            {
                b.Set(x => x.Id, nodeId);
                b.Set(x => x.Name, nodeName);
                b.Set(x => x.SchemaType, schemaType);
                b.Set(x => x.IntProperties, intProperties);
            });
    }

    public static Var<JsonEditorDataNode> CreateNumberNode(
        this SyntaxBuilder b,
        Var<DynamicObject> data,
        Var<SchemaType> schemaType,
        Var<string> nodeName,
        Var<string> nodeId)
    {
        var numberProperties = b.NewObj<JsonEditorNodeNumberProperties>(
            b =>
            {
                b.Set(x => x.IsPresent, b.Not(b.IsUndefined(data)));
                b.Set(x => x.Value, data.As<decimal>());
            });

        return b.NewObj<JsonEditorDataNode>(
            b =>
            {
                b.Set(x => x.Id, nodeId);
                b.Set(x => x.Name, nodeName);
                b.Set(x => x.SchemaType, schemaType);
                b.Set(x => x.NumberProperties, numberProperties);
            });
    }

    public static Var<bool> IsUndefined(this SyntaxBuilder b, Var<DynamicObject> data)
    {
        return b.AreEqual(data, b.GetProperty<DynamicObject>(b.Self(), "undefined"));
    }

    public static Var<bool> IsNull(this SyntaxBuilder b, Var<DynamicObject> data)
    {
        return b.If(
            b.IsUndefined(data),
            b => b.Const(false),
            b => b.Get(data, data => data == null));
    }

    public static Var<JsonEditorDataNode> JsonEditorCreateNodeFromData(
        this SyntaxBuilder b,
        Var<SchemaType> schemaType,
        Var<DynamicObject> data,
        Var<string> nodeName,
        Var<string> nodeId)
    {
        return b.JsonEditorSwitchType(
            b.Get(schemaType, x => x.type),
            defaultValue: b.Const(JsonEditorEmptyDataNode),
            ifObject: b => b.Call(CreateObjectNode, data, schemaType, nodeName, nodeId),
            ifArray: b => b.Call(CreateArrayNode, data, schemaType, nodeName, nodeId),
            ifString: b => b.Call(CreateStringNode, data, schemaType, nodeName, nodeId),
            ifBoolean: b => b.Call(CreateBoolNode, data, schemaType, nodeName, nodeId),
            ifInt: b => b.Call(CreateIntNode, data, schemaType, nodeName, nodeId),
            ifNumber: b => b.Call(CreateNumberNode, data, schemaType, nodeName, nodeId));
    }

    public static Var<JsonEditorDataNode> JsonEditorCreateDefaultNode(
        this SyntaxBuilder b,
        Var<SchemaType> schemaType,
        Var<string> nodeName,
        Var<string> nodeId)
    {
        return b.JsonEditorSwitchType(
            b.Get(schemaType, x => x.type),
            defaultValue: b.Const(JsonEditorEmptyDataNode),
            ifObject: b => b.Call(CreateObjectNode, b.NewObj<DynamicObject>(), schemaType, nodeName, nodeId),
            ifArray: b => b.Call(CreateArrayNode, b.NewCollection<DynamicObject>().As<DynamicObject>(), schemaType, nodeName, nodeId),
            ifString: b => b.Call(CreateStringNode, b.Const(string.Empty).As<DynamicObject>(), schemaType, nodeName, nodeId),
            ifBoolean: b => b.Call(CreateBoolNode, b.Const(false).As<DynamicObject>(), schemaType, nodeName, nodeId),
            ifInt: b => b.Call(CreateIntNode, b.Const(0).As<DynamicObject>(), schemaType, nodeName, nodeId),
            ifNumber: b => b.Call(CreateNumberNode, b.Const(0).As<DynamicObject>(), schemaType, nodeName, nodeId));
    }

    public static Var<TResult> JsonEditorSwitchType<TSyntaxBuilder, TResult>(
        this TSyntaxBuilder b,
        Var<string> type,
        Var<TResult> defaultValue,
        Func<TSyntaxBuilder, Var<TResult>> ifObject,
        Func<TSyntaxBuilder, Var<TResult>> ifArray,
        Func<TSyntaxBuilder, Var<TResult>> ifString,
        Func<TSyntaxBuilder, Var<TResult>> ifBoolean,
        Func<TSyntaxBuilder, Var<TResult>> ifInt,
        Func<TSyntaxBuilder, Var<TResult>> ifNumber)
        where TSyntaxBuilder : SyntaxBuilder, new()
    {
        return b.Switch(
            type,
            b => defaultValue,
            ("object", b => b.Call(ifObject)),
            ("array", b => b.Call(ifArray)),
            ("string", b => b.Call(ifString)),
            ("boolean", b => b.Call(ifBoolean)),
            ("integer", b => b.Call(ifInt)),
            ("number", b => b.Call(ifNumber)));
    }

    public static Var<List<JsonEditorDataNode>> JsonEditorDataNodeChildren(this SyntaxBuilder b, Var<JsonEditorDataNode> node)
    {
        var objectProperties = b.Get(node, x => x.ObjectProperties);
        var arrayProperties = b.Get(node, x => x.ArrayProperties);

        return b.If(
            b.HasObject(objectProperties),
            b => b.Get(objectProperties, x => x.Properties),
            b => b.If(
                b.HasObject(arrayProperties),
                b => b.Get(arrayProperties, x => x.Items),
                b => b.NewCollection<JsonEditorDataNode>()));
    }

    public static Var<IVNode> JsonEditor(this LayoutBuilder b, Var<JsonEditorDataNode> rootNode)
    {
        return b.SlTree(
            b =>
            {
                b.OnSlSelectionChange(b.MakeAction((SyntaxBuilder b, Var<object> model, Var<SlSelectionChangeEventArgs> args) =>
                {
                    var firstItem = b.Get(args, x => x.selection.First());
                    var selectedId = b.GetProperty<string>(firstItem, "id");

                    var selectedNode = b.SearchRecursive(
                        b.GetRef(b.Const(JsonEditorRootDataNode)),
                        b.Def<SyntaxBuilder, JsonEditorDataNode, List<JsonEditorDataNode>>(JsonEditorDataNodeChildren),
                        b.Def((SyntaxBuilder b, Var<JsonEditorDataNode> node) =>
                        {
                            return b.AreEqual(b.Get(node, x => x.Id), selectedId);
                        }),
                        b.Const(JsonEditorEmptyDataNode));

                    b.SetRef(b.Const(JsonEditorSelectedDataNode), selectedNode);

                    return b.Clone(model);
                }));
            },
            b.Map(
                b.Get(b.GetRef(b.Const(JsonEditorRootDataNode)), x => x.ObjectProperties.Properties),
                (b, property) =>
                {
                    return b.JsonEditorTreeItem(property);
                }));
    }

    public static Var<IVNode> JsonEditorTreeItem(this LayoutBuilder b, Var<JsonEditorDataNode> node)
    {
        var children = b.NewCollection<IVNode>();

        b.Push(children, b.Call(JsonEditorNodeLabel, node));

        b.Foreach(
            b.JsonEditorDataNodeChildren(node),
            (b, childNode) =>
            {
                b.Push(children, b.Call(JsonEditorTreeItem, childNode));
            });

        return b.SlTreeItem(
            b =>
            {
                b.SetId(b.Get(node, x => x.Id));
                b.SetExpanded();
            },
            children);
    }


    public static Var<IVNode> JsonEditorNodeLabel(this LayoutBuilder b, Var<JsonEditorDataNode> node)
    {
        var nodeType = b.Get(node, x => x.SchemaType.type);

        return b.JsonEditorSwitchType(
            nodeType,
            b.Text(b.Concat(nodeType, b.Const(" not supported"))),
            ifObject: b => b.Call(JsonEditorObjectLabel, node),
            ifArray: b => b.Call(JsonEditorArrayLabel, node),
            ifString: b => b.Call(JsonEditorStringLabel, node),
            ifBoolean: b => b.Call(JsonEditorBoolLabel, node),
            ifInt: b => b.Call(JsonEditorIntLabel, node),
            ifNumber: b => b.Call(JsonEditorNumberLabel, node));
    }

    public static void AddIsPresentClass(this PropsBuilder<HtmlDiv> b, Var<bool> isPresent)
    {
        b.If(
            isPresent,
            b => b.AddClass("text-gray-800"),
            b => b.AddClass("text-gray-400"));
    }

    public static Var<IVNode> JsonEditorObjectLabel(this LayoutBuilder b, Var<JsonEditorDataNode> node)
    {
        var isPresent = b.Get(node, x => x.ObjectProperties.IsPresent);
        return b.HtmlDiv(
            b =>
            {
                b.AddIsPresentClass(b.Get(node, x => x.ObjectProperties.IsPresent));
            },
            b.JsonEditorNodeNameAndValue(
                b.Get(node, x => x.Name),
                b.If(
                    b.Get(node, x => x.ObjectProperties.IsNull),
                    b => b.Const("null"),
                    b => b.Const("")),
                isPresent));
    }

    public static Var<IVNode> JsonEditorArrayLabel(this LayoutBuilder b, Var<JsonEditorDataNode> node)
    {
        var isPresent = b.Get(node, x => x.ArrayProperties.IsPresent);

        return b.HtmlDiv(
            b =>
            {
                b.AddIsPresentClass(isPresent);
            },
            b.JsonEditorNodeNameAndValue(
                b.Get(node, x => x.Name),
                b.If(
                    b.Get(node, x => x.ArrayProperties.IsNull),
                    b => b.Const("null"),
                    b => b.If(
                        isPresent,
                        b => b.Concat(
                            b.Const("["),
                            b.AsString(b.CollectionLength(b.Get(node, x => x.ArrayProperties.Items))),
                            b.Const("]")),
                        b => b.Const("..."))),
                isPresent));
    }

    public static Var<IVNode> JsonEditorStringLabel(this LayoutBuilder b, Var<JsonEditorDataNode> node)
    {
        var isPresent = b.Get(node, x => x.StringProperties.IsPresent);

        return b.HtmlDiv(
            b =>
            {
                b.AddIsPresentClass(isPresent);
            },
            b.JsonEditorNodeNameAndValue(
                b.Get(node, x => x.Name),
                b.If(
                    isPresent,
                    b => b.If(
                        b.Get(node, x => x.StringProperties.IsNull),
                        b => b.Const("null"),
                        b => b.Get(node, x => x.StringProperties.Value)),
                    b => b.Const("")),
                isPresent));
    }

    public static Var<IVNode> JsonEditorBoolLabel(this LayoutBuilder b, Var<JsonEditorDataNode> node)
    {
        var isPresent = b.Get(node, x => x.BoolProperties.IsPresent);

        return b.HtmlDiv(
            b =>
            {
                b.AddIsPresentClass(isPresent);
            },
            b.JsonEditorNodeNameAndValue(
                b.Get(node, x => x.Name),
                b.If(
                    isPresent,
                    b => b.AsString(b.Get(node, x => x.BoolProperties.Value)),
                    b => b.Const("")),
                isPresent));
    }

    public static Var<IVNode> JsonEditorIntLabel(this LayoutBuilder b, Var<JsonEditorDataNode> node)
    {
        var isPresent = b.Get(node, x => x.IntProperties.IsPresent);

        return b.HtmlDiv(
            b =>
            {
                b.AddIsPresentClass(isPresent);
            },
            b.JsonEditorNodeNameAndValue(
                b.Get(node, x => x.Name),
                b.If(
                    isPresent,
                    b => b.AsString(b.Get(node, x => x.IntProperties.Value)),
                    b => b.Const("")),
                isPresent));
    }

    public static Var<IVNode> JsonEditorNumberLabel(this LayoutBuilder b, Var<JsonEditorDataNode> node)
    {
        var isPresent = b.Get(node, x => x.NumberProperties.IsPresent);

        return b.HtmlDiv(
            b =>
            {
                b.AddIsPresentClass(isPresent);
            },
            b.JsonEditorNodeNameAndValue(
                b.Get(node, x => x.Name),
                b.If(
                    isPresent,
                    b => b.AsString(b.Get(node, x => x.NumberProperties.Value)),
                    b => b.Const("")),
                isPresent));
    }

    public static Var<IVNode> JsonEditorNodeNameAndValue(this LayoutBuilder b, Var<string> name, Var<string> value, Var<bool> isPresent)
    {
        var isPresentValueClass = b.If(isPresent, b => b.Const("text-gray-600"), b => b.Const("text-gray-400"));

        return b.HtmlDiv(
            b =>
            {
                b.SetClass("flex flex-row gap-2 items-baseline");
            },
            b.HtmlDiv(
                b.Text(name)),
            b.HtmlSpan(
                b =>
                {
                    b.AddClass("text-sm");
                    b.AddClass(isPresentValueClass);
                },
                b.Text(value)));
    }

    public static Var<IVNode> JsonEditorBoolOptions(this LayoutBuilder b, Var<JsonEditorDataNode> node)
    {
        var currentValue = b.If(
            b.Not(b.Get(node, x => x.BoolProperties.IsPresent)),
            b => b.Const("not-present"),
            b => b.If(
                b.Get(node, x => x.BoolProperties.Value).As<bool>(),
                b => b.Const("is-true"),
                b => b.Const("is-false")));

        return b.HtmlDiv(
            b =>
            {
                b.SetClass("p-2");
            },
            b.SlRadioGroup(
                b =>
                {
                    b.SetValue(currentValue);
                },
                b.SlRadioButton(
                    b =>
                    {
                        b.SetValue("is-true");
                        b.OnClickAction((SyntaxBuilder b, Var<object> model) =>
                        {
                            b.Set(
                                b.Get(node, x => x.BoolProperties),
                                x => x.IsPresent,
                                b.Const(true));
                            b.Set(
                                b.Get(node, x => x.BoolProperties),
                                x => x.Value,
                                b.Const(true));
                            return b.Clone(model);
                        });
                    },
                    b.Text("true")),
                b.SlRadioButton(
                    b =>
                    {
                        b.SetValue("is-false");
                        b.OnClickAction((SyntaxBuilder b, Var<object> model) =>
                        {
                            b.Set(
                                b.Get(node, x => x.BoolProperties),
                                x => x.IsPresent,
                                b.Const(true));

                            b.Set(
                                b.Get(node, x => x.BoolProperties),
                                x => x.Value,
                                b.Const(false));
                            return b.Clone(model);
                        });
                    },
                    b.Text("false")),
                b.SlRadioButton(b =>
                {
                    b.SetValue("not-present");
                    b.OnClickAction((SyntaxBuilder b, Var<object> model) =>
                    {
                        b.Set(
                            b.Get(node, x => x.BoolProperties),
                            x => x.IsPresent,
                            b.Const(false));
                        return b.Clone(model);
                    });
                },
                b.Text("not present"))));
    }

    public static Var<IVNode> JsonEditorStringOptions(this LayoutBuilder b, Var<JsonEditorDataNode> node)
    {
        var currentValue = b.If(
            b.Not(b.Get(node, x => x.StringProperties.IsPresent)),
            b => b.Const("not-present"),
            b => b.If(
                b.Get(node, x => x.StringProperties.IsNull).As<bool>(),
                b => b.Const("is-null"),
                b => b.Const("is-string")));

        return b.HtmlDiv(
            b =>
            {
                b.SetClass("flex flex-row flex-wrap grow gap-2 p-2");
            },
            b.SlRadioGroup(
                b =>
                {
                    b.SetValue(currentValue);
                },
                b.SlRadioButton(
                    b =>
                    {
                        b.SetValue("is-string");
                        b.OnClickAction((SyntaxBuilder b, Var<object> model) =>
                        {
                            b.Set(
                                b.Get(node, x => x.StringProperties),
                                x => x.IsPresent,
                                b.Const(true));
                            b.Set(
                                b.Get(node, x => x.StringProperties),
                                x => x.IsNull,
                                b.Const(false));

                            b.Set(
                                b.Get(node, x => x.StringProperties),
                                x => x.Value,
                                b.Const(string.Empty));

                            return b.Clone(model);
                        });
                    },
                    b.Text("string")),
                b.SlRadioButton(
                    b =>
                    {
                        b.SetValue("is-null");
                        b.OnClickAction((SyntaxBuilder b, Var<object> model) =>
                        {
                            b.Set(
                                b.Get(node, x => x.StringProperties),
                                x => x.IsPresent,
                                b.Const(true));

                            b.Set(
                                b.Get(node, x => x.StringProperties),
                                x => x.IsNull,
                                b.Const(true));
                            return b.Clone(model);
                        });
                    },
                    b.Text("null")),
                b.SlRadioButton(
                    b =>
                    {
                        b.SetValue("not-present");
                        b.OnClickAction((SyntaxBuilder b, Var<object> model) =>
                        {
                            b.Set(
                                b.Get(node, x => x.StringProperties),
                                x => x.IsPresent,
                                b.Const(false));
                            return b.Clone(model);
                        });
                    },
                    b.Text("not present"))),
            b.Optional(
                b.Get(node, x => x.StringProperties.IsPresent && !x.StringProperties.IsNull),
                b =>
                b.SlInput(
                    b =>
                    {
                        b.SetPlaceholder("value");
                        b.BindToRef(
                            b.Get(node, x => x.StringProperties),
                            x => x.Value);
                    })));
    }

    public static Var<IVNode> JsonEditorNumberOptions(this LayoutBuilder b, Var<JsonEditorDataNode> node)
    {
        var currentValue = b.If(
            b.Not(b.Get(node, x => x.NumberProperties.IsPresent)),
            b => b.Const("not-present"),
            b => b.Const("is-number"));

        return b.HtmlDiv(
            b =>
            {
                b.SetClass("flex flex-row flex-wrap gap-2 p-2");
            },
            b.SlRadioGroup(
                b =>
                {
                    b.SetValue(currentValue);
                },
                b.SlRadioButton(
                    b =>
                    {
                        b.SetValue("is-number");
                        b.OnClickAction((SyntaxBuilder b, Var<object> model) =>
                        {
                            b.Set(
                                b.Get(node, x => x.NumberProperties),
                                x => x.IsPresent,
                                b.Const(true));

                            b.Set(
                                b.Get(node, x => x.NumberProperties),
                                x => x.Value,
                                b.Const(0m));

                            return b.Clone(model);
                        });
                    },
                    b.Text("number")),
                b.SlRadioButton(
                    b =>
                    {
                        b.SetValue("not-present");
                        b.OnClickAction((SyntaxBuilder b, Var<object> model) =>
                        {
                            b.Set(
                                b.Get(node, x => x.NumberProperties),
                                x => x.IsPresent,
                                b.Const(false));
                            return b.Clone(model);
                        });
                    },
                    b.Text("not present"))),
            b.Optional(
                b.Get(node, x => x.NumberProperties.IsPresent),
                b =>
                b.SlInput(
                    b =>
                    {
                        b.SetTypeNumber();
                        b.SetPlaceholder("value");
                        b.SetValue(b.AsString(b.Get(node, x => x.NumberProperties.Value)));
                        b.OnSlInput((SyntaxBuilder b, Var<object> model, Var<Event> e) =>
                        {
                            var newValue = b.GetTargetValue(e);
                            b.Set(
                                b.Get(node, x => x.NumberProperties),
                                x => x.Value,
                                b.ParseDecimal(newValue));
                            return b.Clone(model);
                        });
                    })));
    }

    public static Var<IVNode> JsonEditorIntegerOptions(this LayoutBuilder b, Var<JsonEditorDataNode> node)
    {
        var currentValue = b.If(
            b.Not(b.Get(node, x => x.IntProperties.IsPresent)),
            b => b.Const("not-present"),
            b => b.Const("is-integer"));

        return b.HtmlDiv(
            b =>
            {
                b.SetClass("flex flex-row flex-wrap gap-2 p-2");
            },
            b.SlRadioGroup(
                b =>
                {
                    b.SetValue(currentValue);
                },
                b.SlRadioButton(
                    b =>
                    {
                        b.SetValue("is-integer");
                        b.OnClickAction((SyntaxBuilder b, Var<object> model) =>
                        {
                            b.Set(
                                b.Get(node, x => x.IntProperties),
                                x => x.IsPresent,
                                b.Const(true));

                            b.Set(
                                b.Get(node, x => x.IntProperties),
                                x => x.Value,
                                b.Const(0));

                            return b.Clone(model);
                        });
                    },
                    b.Text("integer")),
                b.SlRadioButton(
                    b =>
                    {
                        b.SetValue("not-present");
                        b.OnClickAction((SyntaxBuilder b, Var<object> model) =>
                        {
                            b.Set(
                                b.Get(node, x => x.IntProperties),
                                x => x.IsPresent,
                                b.Const(false));
                            return b.Clone(model);
                        });
                    },
                    b.Text("not present"))),
            b.Optional(
                b.Get(node, x => x.IntProperties.IsPresent),
                b =>
                b.SlInput(
                    b =>
                    {
                        b.SetTypeNumber();
                        b.SetPlaceholder("value");
                        b.SetValue(b.AsString(b.Get(node, x => x.IntProperties.Value)));
                        b.OnSlInput((SyntaxBuilder b, Var<object> model, Var<Event> e) =>
                        {
                            var newValue = b.GetTargetValue(e);
                            b.Set(
                                b.Get(node, x => x.IntProperties),
                                x => x.Value,
                                b.ParseInt(newValue));
                            return b.Clone(model);
                        });
                    })));
    }

    public static Var<IVNode> JsonEditorObjectOptions(this LayoutBuilder b, Var<JsonEditorDataNode> node)
    {
        var currentValue = b.If(
            b.Not(b.Get(node, x => x.ObjectProperties.IsPresent)),
            b => b.Const("not-present"),
            b => b.If(
                b.Get(node, x => x.ObjectProperties.IsNull),
                b => b.Const("is-null"),
                b => b.Const("is-object")));

        return b.HtmlDiv(
            b =>
            {
                b.SetClass("p-2");
            },
            b.SlRadioGroup(
                b =>
                {
                    b.SetValue(currentValue);
                },
                b.SlRadioButton(
                    b =>
                    {
                        b.SetValue("is-object");
                        b.OnClickAction((SyntaxBuilder b, Var<object> model) =>
                        {
                            b.Set(
                                b.Get(node, x => x.ObjectProperties),
                                x => x.IsPresent,
                                b.Const(true));
                            b.Set(
                                b.Get(node, x => x.ObjectProperties),
                                x => x.IsNull,
                                b.Const(false));

                            var newObject = b.JsonEditorCreateDefaultNode(
                                b.Get(node, x => x.SchemaType),
                                b.Get(node, x => x.Name),
                                b.Get(node, x => x.Id));

                            b.Set(node, x => x.ObjectProperties, b.Get(newObject, x => x.ObjectProperties));

                            return b.Clone(model);
                        });
                    },
                    b.Text("object")),
                b.SlRadioButton(
                    b =>
                    {
                        b.SetValue("is-null");
                        b.OnClickAction((SyntaxBuilder b, Var<object> model) =>
                        {
                            b.Set(
                                b.Get(node, x => x.ObjectProperties),
                                x => x.IsPresent,
                                b.Const(true));

                            b.Set(
                                b.Get(node, x => x.ObjectProperties),
                                x => x.IsNull,
                                b.Const(true));
                            return b.Clone(model);
                        });
                    },
                    b.Text("null")),
                b.SlRadioButton(b =>
                {
                    b.SetValue("not-present");
                    b.OnClickAction((SyntaxBuilder b, Var<object> model) =>
                    {
                        b.Set(
                            b.Get(node, x => x.ObjectProperties),
                            x => x.IsPresent,
                            b.Const(false));

                        b.Set(
                            b.Get(node, x => x.ObjectProperties),
                            x => x.IsNull,
                            b.Const(true));
                        return b.Clone(model);
                    });
                },
                b.Text("not present"))));
    }

    public static Var<IVNode> JsonEditorArrayOptions(this LayoutBuilder b, Var<JsonEditorDataNode> node)
    {
        var currentValue = b.If(
            b.Not(b.Get(node, x => x.ArrayProperties.IsPresent)),
            b => b.Const("not-present"),
            b => b.If(
                b.Get(node, x => x.ArrayProperties.IsNull),
                b => b.Const("is-null"),
                b => b.Const("is-array")));

        return b.HtmlDiv(
            b =>
            {
                b.SetClass("flex flex-row gap-2 p-2");
            },
            b.SlRadioGroup(
                b =>
                {
                    b.SetValue(currentValue);
                },
                b.SlRadioButton(
                    b =>
                    {
                        b.SetValue("is-array");
                        b.OnClickAction((SyntaxBuilder b, Var<object> model) =>
                        {
                            b.Set(
                                b.Get(node, x => x.ArrayProperties),
                                x => x.IsPresent,
                                b.Const(true));
                            b.Set(
                                b.Get(node, x => x.ArrayProperties),
                                x => x.IsNull,
                                b.Const(false));
                            return b.Clone(model);
                        });
                    },
                    b.Text("array")),
                b.SlRadioButton(
                    b =>
                    {
                        b.SetValue("is-null");
                        b.OnClickAction((SyntaxBuilder b, Var<object> model) =>
                        {
                            b.Set(
                                b.Get(node, x => x.ArrayProperties),
                                x => x.IsPresent,
                                b.Const(true));

                            b.Set(
                                b.Get(node, x => x.ArrayProperties),
                                x => x.IsNull,
                                b.Const(true));
                            return b.Clone(model);
                        });
                    },
                    b.Text("null")),
                b.SlRadioButton(b =>
                {
                    b.SetValue("not-present");
                    b.OnClickAction((SyntaxBuilder b, Var<object> model) =>
                    {
                        b.Set(
                            b.Get(node, x => x.ArrayProperties),
                            x => x.IsPresent,
                            b.Const(false));

                        b.Set(
                            b.Get(node, x => x.ArrayProperties),
                            x => x.IsNull,
                            b.Const(true));
                        return b.Clone(model);
                    });
                },
                b.Text("not present"))),
            b.Optional(
                b.Get(node, x => x.ArrayProperties.IsPresent && !x.ArrayProperties.IsNull),
                b =>
                {
                    return b.SlButton(
                        b =>
                        {
                            b.SetOutline();
                            b.SetVariantPrimary();
                            b.OnClickAction((SyntaxBuilder b, Var<object> model) =>
                            {
                                var nextItemIndex = b.Get(node, x => x.ArrayProperties.Items.Count() + 1);
                                var newObject = b.JsonEditorCreateDefaultNode(
                                    b.Get(node, x => x.SchemaType.items),
                                    b.AsString(nextItemIndex),
                                    b.Concat(
                                        b.Get(node, x => x.Id),
                                        b.Const("/"),
                                        b.AsString(nextItemIndex)));

                                b.Push(b.Get(node, x => x.ArrayProperties.Items), newObject);

                                return b.Clone(model);
                            });
                        },
                        b.Text("+"));
                }));
    }

    public static Var<JsonEditorDataNode> JsonEditorGetRootNode(this SyntaxBuilder b)
    {
        return b.GetRef(b.Const(JsonEditorRootDataNode));
    }

    public static Var<JsonEditorDataNode> JsonEditorGetSelectedNode(this SyntaxBuilder b)
    {
        return b.GetRef(b.Const(JsonEditorSelectedDataNode));
    }

    public static Var<IVNode> JsonEditorSelectedNodeOptions(this LayoutBuilder b)
    {
        var selectedNode = b.JsonEditorGetSelectedNode();
        return b.If(
            b.HasObject(selectedNode),
            b =>
            {
                return b.JsonEditorSwitchType(
                    b.Get(selectedNode, x => x.SchemaType.type),
                    b.HtmlDiv(),
                    ifObject: b => b.JsonEditorObjectOptions(selectedNode),
                    ifArray: b => b.JsonEditorArrayOptions(selectedNode),
                    ifString: b => b.JsonEditorStringOptions(selectedNode),
                    ifBoolean: b => b.JsonEditorBoolOptions(selectedNode),
                    ifInt: b => b.JsonEditorIntegerOptions(selectedNode),
                    ifNumber: b => b.JsonEditorNumberOptions(selectedNode));

            },
            b => b.HtmlDiv());
    }

    public static Var<string> JsonEditorGenerate(
        this SyntaxBuilder b,
        Var<JsonEditorDataNode> currentNode,
        Var<int> indentLevel,
        Var<bool> usePropertyName)
    {
        return b.JsonEditorSwitchType(
            b.Get(currentNode, x => x.SchemaType.type),
            b.Const(string.Empty),
            ifObject: b =>
            {
                var nextIndentLevel = b.Get(indentLevel, x => x + 1);

                return b.If(
                    b.Not(b.Get(currentNode, x => x.ObjectProperties.IsPresent)),
                    b => b.Const(string.Empty),
                    b => b.Concat(
                        b.JsonEditorIndentSpace(indentLevel),
                        b.If(
                            usePropertyName,
                            b => b.Concat(
                                b.Const("\""),
                                b.Get(currentNode, x => x.Name),
                                b.Const("\":")),
                            b => b.Const(string.Empty)),
                        b.If(
                            b.Get(currentNode, x => x.ObjectProperties.IsNull),
                            b => b.Const("null"),
                            b => b.Concat(
                                b.Const("{\n"),
                                b.JoinStrings(
                                    b.Const(",\n"),
                                    b.Map(
                                        b.Get(currentNode, x => x.ObjectProperties.Properties),
                                        (b, property) =>
                                        {
                                            return b.Call(JsonEditorGenerate, property, nextIndentLevel, b.Const(true));
                                        })))),
                        b.Const("\n"),
                        b.JsonEditorIndentSpace(indentLevel),
                        b.Const("}")));
            },
            ifArray: b =>
            {
                var nextIndentLevel = b.Get(indentLevel, x => x + 1);

                return b.If(
                    b.Not(b.Get(currentNode, x => x.ArrayProperties.IsPresent)),
                    b => b.Const(string.Empty),
                    b => b.Concat(
                        b.JsonEditorIndentSpace(indentLevel),
                        b.If(
                            usePropertyName,
                            b => b.Concat(
                                b.Const("\""),
                                b.Get(currentNode, x => x.Name),
                                b.Const("\":")),
                            b => b.Const(string.Empty)),
                        b.If(
                            b.Get(currentNode, x => x.ArrayProperties.IsNull),
                            b => b.Const("null"),
                            b => b.Concat(
                                b.Const("[\n"),
                                b.JoinStrings(
                                    b.Const(",\n"),
                                    b.Map(
                                        b.Get(currentNode, x => x.ArrayProperties.Items),
                                        (b, property) =>
                                        {
                                            return b.Call(JsonEditorGenerate, property, nextIndentLevel, b.Const(false));
                                        })))),
                        b.Const("\n"),
                        b.JsonEditorIndentSpace(indentLevel),
                        b.Const("]")));
            },
            ifString: b =>
            {
                return b.If(
                    b.Not(b.Get(currentNode, x => x.StringProperties.IsPresent)),
                    b => b.Const(string.Empty),
                    b => b.Concat(
                        b.JsonEditorIndentSpace(indentLevel),
                        b.If(
                            usePropertyName,
                            b => b.Concat(
                                b.Const("\""),
                                b.Get(currentNode, x => x.Name),
                                b.Const("\":")),
                            b => b.Const(string.Empty)),
                        b.If(
                            b.Get(currentNode, x => x.StringProperties.IsNull),
                            b => b.Const("null"),
                            b => b.Concat(
                                b.Const("\""),
                                b.Get(currentNode, x => x.StringProperties.Value),
                                b.Const("\"")))));

            },
            ifBoolean: b =>
            {
                return b.If(
                    b.Not(b.Get(currentNode, x => x.BoolProperties.IsPresent)),
                    b => b.Const(string.Empty),
                    b => b.Concat(
                        b.JsonEditorIndentSpace(indentLevel),
                        b.If(
                            usePropertyName,
                            b => b.Concat(
                                b.Const("\""),
                                b.Get(currentNode, x => x.Name),
                                b.Const("\":")),
                            b => b.Const(string.Empty)),
                        b.AsString(b.Get(currentNode, x => x.BoolProperties.Value))));
            },
            ifInt: b =>
            {
                return b.If(
                    b.Not(b.Get(currentNode, x => x.IntProperties.IsPresent)),
                    b => b.Const(string.Empty),
                    b => b.Concat(
                        b.JsonEditorIndentSpace(indentLevel),
                        b.If(
                            usePropertyName,
                            b => b.Concat(
                                b.Const("\""),
                                b.Get(currentNode, x => x.Name),
                                b.Const("\":")),
                            b => b.Const(string.Empty)),
                        b.AsString(b.Get(currentNode, x => x.IntProperties.Value))));
            },
            ifNumber: b =>
            {
                return b.If(
                    b.Not(b.Get(currentNode, x => x.NumberProperties.IsPresent)),
                    b => b.Const(string.Empty),
                    b => b.Concat(
                        b.JsonEditorIndentSpace(indentLevel),
                        b.If(
                            usePropertyName,
                            b => b.Concat(
                                b.Const("\""),
                                b.Get(currentNode, x => x.Name),
                                b.Const("\":")),
                            b => b.Const(string.Empty)),
                        b.AsString(b.Get(currentNode, x => x.NumberProperties.Value))));
            });
    }

    public static Var<IVNode> JsonEditorPreview(this LayoutBuilder b, Var<JsonEditorDataNode> currentNode)
    {
        return b.Optional(
            b.HasObject(currentNode),
            b =>
            b.HtmlDiv(
                b =>
                {
                    b.SetClass("text-sm text-gray-600 font-mono");
                },
                b.JsonEditorPreviewLine(currentNode, b.Const(0), b.Const(true), b.Const(true))));
    }

    public static Var<IVNode> JsonEditorPreviewLine(
        this LayoutBuilder b,
        Var<JsonEditorDataNode> currentNode,
        Var<int> indentLevel,
        Var<bool> isLastChild,
        Var<bool> isInArray)
    {
        return b.Optional(
            b.JsonEditorDataNodeIsPresent(currentNode),
            b =>
            {
                return b.HtmlDiv(
                    b =>
                    {
                        b.SetId(b.Concat(b.Const("preview-"), b.Get(currentNode, x => x.Id)));
                        b.If(
                            b.AreEqual(currentNode, b.GetRef(b.Const(JsonEditorSelectedDataNode))),
                            b =>
                            {
                                b.AddClass("text-gray-800 font-semibold");
                            });
                    },
                    b.JsonEditorSwitchType(
                        b.Get(currentNode, x => x.SchemaType.type),
                        b.VoidNode(),
                        ifObject: b =>
                        {
                            var vdomChildren = b.NewCollection<IVNode>();

                            b.Push(vdomChildren,
                                b.HtmlPre(
                                    b.Text(b.Concat(
                                        b.JsonEditorIndentSpace(indentLevel),
                                        b.If(b.Not(isInArray), b => b.Const("\""), b => b.Const(string.Empty)),
                                        b.If(b.Not(isInArray), b => b.Get(currentNode, x => x.Name), b => b.Const(string.Empty)),
                                        b.If(b.Not(isInArray), b => b.Const("\":"), b => b.Const(string.Empty)),
                                        b.Const("{")))));

                            var dataChildren = b.JsonEditorDataNodeChildren(currentNode);
                            var childrenCount = b.Get(dataChildren, x => x.Count());
                            var nextIndentLevel = b.Get(indentLevel, x => x + 1);
                            b.Foreach(
                                dataChildren,
                                (b, child, index) =>
                                {
                                    var isLast = b.Get(childrenCount, index, (childrenCount, index) => childrenCount == index + 1);
                                    b.Push(vdomChildren, b.Call(JsonEditorPreviewLine, child, nextIndentLevel, isLastChild, b.Const(false)));
                                });

                            b.Push(vdomChildren,
                                b.HtmlPre(
                                    b.Text(b.Concat(
                                        b.JsonEditorIndentSpace(indentLevel),
                                        b.Const("}"),
                                        b.If(isLastChild, b => b.Const(""), b => b.Const(","))))));

                            return
                            b.HtmlDiv(
                                b =>
                                {
                                    b.SetClass("flex flex-col");
                                },
                                vdomChildren);
                        },
                        ifArray: b =>
                        {
                            var vdomChildren = b.NewCollection<IVNode>();

                            b.Push(vdomChildren,
                                b.HtmlPre(
                                    b.Text(b.Concat(
                                        b.JsonEditorIndentSpace(indentLevel),
                                        b.Const("\""),
                                        b.Get(currentNode, x => x.Name),
                                        b.Const("\":[")))));

                            var dataChildren = b.JsonEditorDataNodeChildren(currentNode);
                            var childrenCount = b.Get(dataChildren, x => x.Count());
                            var nextIndentLevel = b.Get(indentLevel, x => x + 1);
                            b.Foreach(
                                dataChildren,
                                (b, child, index) =>
                                {
                                    var isLast = b.Get(childrenCount, index, (childrenCount, index) => childrenCount == index + 1);
                                    b.Push(vdomChildren, b.Call(JsonEditorPreviewLine, child, nextIndentLevel, isLastChild, b.Const(true)));
                                });

                            b.Push(vdomChildren,
                                b.HtmlPre(
                                    b.Text(b.Concat(
                                        b.JsonEditorIndentSpace(indentLevel),
                                        b.Const("]"),
                                        b.If(isLastChild, b => b.Const(""), b => b.Const(","))))));

                            return
                            b.HtmlDiv(
                                b =>
                                {
                                    b.SetClass("flex flex-col");
                                },
                                vdomChildren);
                        },
                        ifString: b =>
                        {
                            return b.HtmlPre(
                                b.Text(b.Concat(
                                    b.JsonEditorIndentSpace(indentLevel),
                                    b.Const("\""),
                                    b.Get(currentNode, x => x.Name),
                                    b.Const("\":"),
                                    b.If(
                                        b.Get(currentNode, x => x.StringProperties.IsNull),
                                        b => b.Const("null"),
                                        b => b.Concat(
                                            b.Const("\""),
                                            b.Get(currentNode, x => x.StringProperties.Value),
                                            b.Const("\""))),
                                    b.If(isLastChild, b => b.Const(""), b => b.Const(",")))));
                        },
                        ifBoolean: b =>
                        {
                            return b.HtmlPre(
                                b.Text(b.Concat(
                                    b.JsonEditorIndentSpace(indentLevel),
                                        b.Const("\""),
                                        b.Get(currentNode, x => x.Name),
                                        b.Const("\":"),
                                        b.AsString(b.Get(currentNode, x => x.BoolProperties.Value)),
                                        b.If(isLastChild, b => b.Const(""), b => b.Const(",")))));
                        },
                        ifInt: b =>
                        {
                            return b.HtmlPre(
                                b.Text(b.Concat(
                                    b.JsonEditorIndentSpace(indentLevel),
                                        b.Const("\""),
                                        b.Get(currentNode, x => x.Name),
                                        b.Const("\":"),
                                        b.AsString(b.Get(currentNode, x => x.IntProperties.Value)),
                                        b.If(isLastChild, b => b.Const(""), b => b.Const(",")))));
                        },
                        ifNumber: b =>
                        {
                            return b.HtmlPre(
                                b.Text(b.Concat(
                                    b.JsonEditorIndentSpace(indentLevel),
                                        b.Const("\""),
                                        b.Get(currentNode, x => x.Name),
                                        b.Const("\":"),
                                        b.AsString(b.Get(currentNode, x => x.NumberProperties.Value)),
                                        b.If(isLastChild, b => b.Const(""), b => b.Const(",")))));
                        }));
            });
    }


    //return
    //b.HtmlDiv(
    //    b =>
    //    {
    //        b.SetClass("flex flex-col gap-1");
    //    },
    //    b.HtmlDiv(
    //        b =>
    //        {
    //            b.SetClass("flex flex-row");
    //        },
    //        b.HtmlPre(
    //            b =>
    //            {
    //                b.SetId(b.Concat(b.Const("preview-"), b.Get(currentNode, x => x.Id)));
    //            },
    //            b.Text(b.Concat(
    //                b.JsonEditorIndentSpace(indentLevel),
    //                b.Const("\""),
    //                b.Get(currentNode, x => x.Name),
    //                b.Const("\":"),
    //                b.JsonEditorNodeStartBracket(currentNode))))),
    //    b.HtmlDiv(
    //        b =>
    //        {
    //            b.SetClass("flex flex-row");
    //        },
    //        b.Join(
    //            b.HtmlPre(b.Text(b.Const(","))),
    //        b.Map(
    //            b.JsonEditorDataNodeChildren(currentNode),
    //            (b, child) =>
    //            {
    //                var nextIndentLevel = b.Get(indentLevel, x => x + 1);

    //                //return b.JsonEditorSwitchType(
    //                //    b.Get(currentNode, x=>x.SchemaType.type),
    //                //    b.Const(string.Empty),
    //                //    ifObject: b=>
    //                //    {

    //                //        b.Call(JsonEditorPreviewLine, child, nextIndentLevel);
    //                //    },
    //                //    ifArray: b=>
    //                //    {

    //                //    },
    //                //    ifString: 

    //                return b.Call(JsonEditorPreviewLine, child, nextIndentLevel);
    //            }))));
    //});

    public static Var<string> JsonEditorNodeStartBracket(this SyntaxBuilder b, Var<JsonEditorDataNode> node)
    {
        return b.JsonEditorSwitchType(
            b.Get(node, x => x.SchemaType.type),
            b.Const(string.Empty),
            ifObject: b => b.Const("{"),
            ifArray: b => b.Const("["),
            ifString: b => b.Const(string.Empty),
            ifBoolean: b => b.Const(string.Empty),
            ifInt: b => b.Const(string.Empty),
            ifNumber: b => b.Const(string.Empty));
    }

    public static Var<string> JsonEditorNodeEndBracket(this SyntaxBuilder b, Var<JsonEditorDataNode> node)
    {
        return b.JsonEditorSwitchType(
            b.Get(node, x => x.SchemaType.type),
            b.Const(string.Empty),
            ifObject: b => b.Const("}"),
            ifArray: b => b.Const("]"),
            ifString: b => b.Const(string.Empty),
            ifBoolean: b => b.Const(string.Empty),
            ifInt: b => b.Const(string.Empty),
            ifNumber: b => b.Const(string.Empty));
    }

    public static Var<string> JsonEditorIndentSpace(this SyntaxBuilder b, Var<int> indentLevel)
    {
        return b.CallOnObject<string>(b.Const(" "), "repeat", b.Get(indentLevel, x => x * 2));
    }

    public static Var<bool> JsonEditorDataNodeIsPresent(this SyntaxBuilder b, Var<JsonEditorDataNode> node)
    {
        return b.JsonEditorSwitchType(
            b.Get(node, x => x.SchemaType.type),
            b.Const(false),
            ifObject: b => b.Get(node, x => x.ObjectProperties.IsPresent),
            ifArray: b => b.Get(node, x => x.ArrayProperties.IsPresent),
            ifString: b => b.Get(node, x => x.StringProperties.IsPresent),
            ifBoolean: b => b.Get(node, x => x.BoolProperties.IsPresent),
            ifInt: b => b.Get(node, x => x.IntProperties.IsPresent),
            ifNumber: b => b.Get(node, x => x.NumberProperties.IsPresent));
    }

    public static void BindToRef<TControl, TEntity>(
        this PropsBuilder<TControl> b,
        Var<TEntity> entity,
        System.Linq.Expressions.Expression<System.Func<TEntity, string>> onProperty)
        where TControl : IAllowsBinding<TControl>, new()
    {
        Var<string> value = b.Get(entity, onProperty);

        var setProperty = b.MakeAction<object, string>((SyntaxBuilder b, Var<object> state, Var<string> inputValue) =>
        {
            b.Set(entity, onProperty, inputValue);
            return b.Clone(state);
        });

        var binder = new TControl().GetControlBinder();
        binder.SetControlValue(b, value);
        b.OnEventAction(binder.NewValueEventName, setProperty, b.Def(binder.GetEventValue));
    }
}
