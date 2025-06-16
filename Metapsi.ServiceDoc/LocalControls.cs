using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System;
using Metapsi.Shoelace;
using System.Linq.Expressions;
using Metapsi.Html;
using System.Collections.Generic;
using System.Linq;

namespace Metapsi
{
    public static class LocalControls
    {
        public static System.Linq.Expressions.Expression<Func<TEntity, int>> IntPropertyExpression<TEntity>(string propertyName)
        {
            var x = Expression.Parameter(typeof(TEntity), "x");
            return Expression.Lambda<Func<TEntity, int>>(Expression.Property(x, propertyName), x);
        }


        public static System.Linq.Expressions.Expression<Func<TEntity, string>> StringPropertyExpression<TEntity>(string propertyName)
        {
            var x = Expression.Parameter(typeof(TEntity), "x");
            return Expression.Lambda<Func<TEntity, string>>(Expression.Property(x, propertyName), x);
        }

        public static System.Linq.Expressions.Expression<Func<TEntity, bool>> BoolPropertyExpression<TEntity>(string propertyName)
        {
            var x = Expression.Parameter(typeof(TEntity), "x");
            return Expression.Lambda<Func<TEntity, bool>>(Expression.Property(x, propertyName), x);
        }

        public static IEnumerable<System.Reflection.PropertyInfo> GetBoolProperties(Type type)
        {
            var properties = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            return properties.Where(x=>x.PropertyType == typeof(bool));
        }

        public static IEnumerable<System.Reflection.PropertyInfo> GetCollectionProperties(Type type)
        {
            var properties = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            return properties.Where(x => typeof(System.Collections.IList).IsAssignableFrom(x.PropertyType));
        }

        public static IEnumerable<System.Reflection.PropertyInfo> GetEnumProperties(Type type)
        {
            var properties = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            return properties.Where(x => x.PropertyType.IsEnum);
        }

        public static Var<IVNode> AutoEditForm<TModel, TEntity>(
            this LayoutBuilder b, 
            Var<TModel> model,
            Var<TEntity> entity)
        {
            var editControls = b.NewCollection<IVNode>();

            var properties = typeof(TEntity).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            foreach (var property in properties)
            {
                if (property.PropertyType == typeof(bool))
                {
                    var checkbox = b.SlCheckbox(
                        b =>
                        {
                            // ignore model, use reference
                            b.BindTo(model, b.Def((SyntaxBuilder b, Var<TModel> model) => entity), BoolPropertyExpression<TEntity>(property.Name));
                        },
                        b.Text(b.FormatLabel(b.Const(property.Name))));
                    b.Push(editControls, checkbox);
                }
                if (property.PropertyType == typeof(int))
                {
                    var input = b.SlInput(
                        b =>
                        {
                            b.SetTypeNumber();
                            b.SetLabel(b.FormatLabel(b.Const(property.Name)));
                            b.BindTo(
                                model,
                                b.Def((SyntaxBuilder b, Var<TModel> model) => entity), IntPropertyExpression<TEntity>(property.Name));
                        });
                    b.Push(editControls, input);
                }
                else if (property.PropertyType == typeof(string))
                {
                    var input = b.SlInput(
                        b =>
                        {
                            b.SetLabel(b.FormatLabel(b.Const(property.Name)));
                            b.BindTo(model, b.Def((SyntaxBuilder b, Var<TModel> model) => entity), StringPropertyExpression<TEntity>(property.Name));
                        });
                    b.Push(editControls, input);
                }
                else if (property.PropertyType.IsEnum)
                {
                    Dictionary<string, int> enumOptions = new Dictionary<string, int>();

                    var options = b.NewCollection<IVNode>();

                    var enumValues = property.PropertyType.GetEnumValues();
                    for (int i = 0; i < enumValues.Length; i++)
                    {
                        b.Push(options, b.SlOption(
                            b =>
                            {
                                b.SetValue(i.ToString());
                            },
                            b.Text(enumValues.GetValue(i).ToString())));
                    }

                    var input = b.SlSelect(
                        b =>
                        {
                            b.SetLabel(b.FormatLabel(b.Const(property.Name)));
                            b.OnSlChange((SyntaxBuilder b, Var<TModel> model, Var<Event> domEvent) =>
                            {
                                var selectedValue = b.GetTargetValue(domEvent);
                                b.SetProperty(entity, b.Const(property.Name), b.ParseInt(selectedValue));
                                return b.Clone(model);
                            });
                            var currentValue = b.AsString(b.GetProperty<int>(entity, b.Const(property.Name)));
                            b.SetValue(currentValue);
                        },
                        options);
                    b.Push(editControls, input);
                }
            }

            return b.HtmlDiv(b => b.SetClass("flex flex-col gap-4"), editControls);
        }
    }
}
