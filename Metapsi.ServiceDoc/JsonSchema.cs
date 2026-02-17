using System;
using System.Collections.Generic;
using System.Linq;

namespace Metapsi;

public class SchemaType
{
    public string type { get; set; }
    public string description { get; set; }
    public string format { get; set; }
    public decimal minimum { get; set; }
    public decimal maximum { get; set; }
    public int minLength { get; set; }
    public int maxLength { get; set; }
    public string pattern { get; set; }
    public List<string> @enum { get; set; } = new();

    public List<SchemaProperty> properties { get; set; } = new();
    public SchemaType items { get; set; }
}

public class SchemaProperty
{
    public string name { get; set; }
    public SchemaType type { get; set; }
}

public static class JsonSchemaExtensions
{
    public static SchemaType GetJsonSchemaType(Type type)
    {
        SchemaType outType = new SchemaType()
        {
            type = "object",
        };

        if (TypeExtensions.PrimitiveTypes.ContainsKey(type.Name))
        {
            if (type == typeof(string))
            {
                outType.type = "string";
            }
            if (type == typeof(bool))
            {
                outType.type = "boolean";
            }
            if (type == typeof(decimal))
            {
                outType.type = "number";
            }
            if(type == typeof(int))
            {
                outType.type = "integer";
            }
        }
        else
        {
            if (type == typeof(Guid))
            {
                outType.type = "string";
            }
            else
            if (type.IsEnum)
            {
                outType.type = "integer";
            }
            else
            if (type.Name == "List`1")
            {
                outType.type = "array";
                outType.items = GetJsonSchemaType(type.GenericTypeArguments.First());
            }
            else if (type.Name == "DateTime")
            {
                outType.type = "string";
            }
            else
            {
                var publicProperties = type.GetProperties();
                foreach (var property in publicProperties)
                {
                    outType.properties.Add(new SchemaProperty()
                    {
                        name = property.Name,
                        type = GetJsonSchemaType(property.PropertyType)
                    });
                }
            }
        }

        return outType;
    }
}