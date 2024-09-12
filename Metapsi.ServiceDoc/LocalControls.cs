using Metapsi.Hyperapp;
using Metapsi.Syntax;
using System;
using Metapsi.Shoelace;
using System.Linq.Expressions;
using Metapsi.Html;

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
                            b.BindTo(model, b.Def((SyntaxBuilder b, Var<TModel> model) => entity), BoolPropertyExpression<TEntity>(property.Name), Converter.BoolConverter);
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
                                b.Def((SyntaxBuilder b, Var<TModel> model) => entity), IntPropertyExpression<TEntity>(property.Name),
                                Converter.IntConverter);
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
            }

            return b.HtmlDiv(b => b.SetClass("flex flex-col gap-4"), editControls);
        }
    }
}
