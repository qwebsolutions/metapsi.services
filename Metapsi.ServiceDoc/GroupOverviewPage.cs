using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Shoelace;
using Metapsi.Syntax;
using System.Collections.Generic;

namespace Metapsi
{
    internal class DocsOverviewModel
    {
        public List<DocTypeOverview> DocTypes { get; set; } = new();
    }

    internal class DocTypeOverview
    {
        public string DocTypeName { get; set; } = string.Empty;
        public int Count { get; set; } = 0;
        public string DocumentTypeUrl { get; set; } = string.Empty;
        //public string ApiUrl { get; set; } = string.Empty;
    }

    public static partial class ServiceDoc
    {
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
                                    b.SetHref(b.Get(service, x => x.DocumentTypeUrl));
                                },
                                b.SlCard(
                                    b =>
                                    {
                                    },
                                    b.HtmlDiv(
                                        b =>
                                        {
                                            b.SetClass("font-semibold text-gray-800");
                                        },
                                        b.Text(b.Get(service, x => x.DocTypeName))),
                                    b.HtmlDiv(
                                        b =>
                                        {
                                            b.SetClass("text-gray-600");
                                        },
                                        b.Text(b.AsString(b.Get(service, x => x.Count))))));
                        }));
                }));
        }

        internal static Var<string> EncodeUriComponent(this SyntaxBuilder b, Var<string> initial)
        {
            return b.CallOnObject<string>(b.Window(), "encodeURIComponent", initial);
        }
    }
}