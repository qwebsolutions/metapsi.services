using Metapsi;
using Metapsi.Hyperapp;
using Metapsi.Html;
using Metapsi.Dom;
using Metapsi.Syntax;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace Metapsi;

public class SignInUserPasswordModel
{
    public string LogoUrl { get; set; } = string.Empty;
    public string SignInFormTitle { get; set; } = "Sign in to your account";
    public string SignInFormSubtitle { get; set; } = string.Empty;
    public string UserNameLabel { get; set; } = "User";
    public string PasswordLabel { get; set; } = "Password";
    public string SignInButtonLabel { get; set; } = "Sign in";
    public string SignInPath { get; set; } = "/sign-in";
}

public class SignInServiceUserPasswordOptions
{
    public string SignInPath { get; set; } = "/sign-in";
    public Func<HttpContext, Task<SignInUserPasswordModel>> LoadSignInPage { get; set; } = async (httpContext) => new SignInUserPasswordModel();
    public Func<CommandContext, string, string, Task<List<Claim>>> LoadClaims { get; set; } = async (commandContext, user, password) => new List<Claim>();
    public IResult SignInResult { get; set; } = Results.Redirect("/");
    public Action<HtmlBuilder> ModifyPage { get; set; } = (HtmlBuilder b) => { };
}

public static partial class SignInService
{
    public static void UseUserPasswordSignInPage(this IEndpointRouteBuilder baseEndpoint,
        Action<SignInServiceUserPasswordOptions> configure = null)
    {
        var options = new SignInServiceUserPasswordOptions();

        if (configure != null)
        {
            configure(options);
        };

        baseEndpoint.MapGet(options.SignInPath, async (CommandContext commandContext, HttpContext httpContext) =>
        {
            var model = await options.LoadSignInPage(httpContext);
            model.SignInPath = options.SignInPath;
            return Page.Model(model);
        }).AllowAnonymous();

        baseEndpoint.MapPost(options.SignInPath, async (CommandContext commandContext, HttpContext httpContext) =>
        {
            var userName = httpContext.Request.Form["id-user-name-input"];
            var password = httpContext.Request.Form["id-password-input"];

            var claims = await options.LoadClaims(commandContext, userName, password);

            if (claims != null)
            {
                if (claims.Any())
                {
                    var identity = new ClaimsIdentity(claims, "cookie");

                    var principal = new System.Security.Claims.ClaimsPrincipal(identity);
                    await httpContext.SignInAsync(principal, new AuthenticationProperties()
                    {
                        IsPersistent = true
                    });
                    return options.SignInResult;
                }
            }

            // Sign in failed, render page again
            var model = await options.LoadSignInPage(httpContext);
            return Page.Model(model);

        }).AllowAnonymous();

        baseEndpoint.UseRenderer<SignInUserPasswordModel>(model =>
        {
            return HtmlBuilder.FromDefault(b =>
            {
                SignInUserPasswordPage(b, model);
                options.ModifyPage(b);
            }).ToHtml();
        });
    }

    public static void SignInUserPasswordPage(HtmlBuilder b, SignInUserPasswordModel model)
    {
        b.AddModuleStylesheet();
        StaticFiles.AddAll(typeof(HyperType).Assembly);
        StaticFiles.AddAll(typeof(SignInUserPasswordModel).Assembly);

        b.Document.Body.SetAttribute("class", "w-screen h-screen");
        b.BodyAppend(
            b.HtmlDiv(
            b =>
            {
                b.SetClass("w-screen h-screen");
            },
            b.HtmlDiv(
                b =>
                {
                    b.SetClass("flex flex-col h-full w-full items-center justify-between gap-6 bg-[#fafafa] p-4");
                },
                b.HtmlDiv(
                    b =>
                    {
                        b.SetClass("flex flex-col items-center justify-center gap-4 w-full h-full");
                    },
                    b.HtmlDiv(
                        b =>
                        {
                            b.SetClass("w-full h-full");
                        },
                        b.Optional(
                            !string.IsNullOrEmpty(model.LogoUrl),
                            b =>
                            b.HtmlImg(b =>
                            {
                                b.SetClass("object-contain w-full h-[20vh]");
                                b.SetSrc(model.LogoUrl);
                            }))),
                    b.Optional(
                        !string.IsNullOrEmpty(model.SignInFormTitle),
                        b => b.HtmlSpan(
                            b =>
                            {
                                b.SetClass("w-full text-center text-xl text-gray-700 font-semibold");
                            },
                            b.Text(model.SignInFormTitle))),
                    b.Optional(
                        !string.IsNullOrEmpty(model.SignInFormSubtitle),
                        b => b.HtmlSpan(
                            b =>
                            {
                                b.SetClass("w-full text-center text-sm text-gray-500");
                            },
                            b.Text(model.SignInFormSubtitle)))),
                b.HtmlForm(
                    b =>
                    {
                        b.SetAttribute("id", "id-sign-in-form");
                        b.SetClass("flex flex-col items-center justify-center gap-6 w-full h-full max-w-96");
                        b.SetAttribute("method", "POST");
                        b.SetAttribute("action", model.SignInPath);
                    },
                    b.HtmlDiv(
                        b =>
                        {
                            b.SetClass("flex flex-col gap-3 w-full");
                        },
                        b.HtmlDiv(
                            b =>
                            {
                                b.SetClass("flex flex-col gap-1");
                            },
                            b.HtmlLabel(
                                b =>
                                {
                                    b.SetAttribute("for", "id-user-name-input");
                                    b.SetClass("text-sm text-gray-600 font-semibold");
                                },
                                b.Text(model.UserNameLabel)),
                            b.HtmlInput(b =>
                            {
                                b.SetAttribute("id", "id-user-name-input");
                                b.SetAttribute("name", "id-user-name-input");
                                b.SetClass("rounded border border-gray-200 p-2");
                                b.SetAttribute("autocomplete", "username");
                            })),
                        b.HtmlDiv(
                            b =>
                            {
                                b.SetClass("flex flex-col gap-1");
                            },
                            b.HtmlLabel(
                                b =>
                                {
                                    b.SetAttribute("for", "id-password-input");
                                    b.SetClass("text-sm text-gray-600 font-semibold");
                                },
                                b.Text(model.PasswordLabel)),
                            b.HtmlInput(b =>
                            {
                                b.SetAttribute("type", "password");
                                b.SetAttribute("autocomplete", "current-password");
                                b.SetAttribute("id", "id-password-input");
                                b.SetAttribute("name", "id-password-input");
                                b.SetClass("rounded border border-gray-200 p-2");
                            }))),
                    b.HtmlButton(
                        b =>
                        {
                            b.SetAttribute("id", "id-btn-sign-in");
                            b.SetClass("w-full rounded bg-sky-600 text-white font-semibold p-2");
                        },
                        b.Text(model.SignInButtonLabel),
                        b.HtmlScriptModule(b =>
                        {
                            var signInForm = b.GetElementById(b.Const("id-sign-in-form"));
                            b.CallOnObject(
                                signInForm,
                                "addEventListener",
                                b.Const("submit"),
                                b.Def((SyntaxBuilder b, Var<DomEvent> domEvent) =>
                                {
                                    var userNameInput = b.GetElementById(b.Const("id-user-name-input"));
                                    var userNameValue = b.GetProperty<string>(userNameInput, "value");
                                    b.If(
                                        b.Not(
                                            b.HasValue(userNameValue)),
                                        b =>
                                        {
                                            b.CallOnObject(userNameInput, "focus");
                                            b.PreventDefault(domEvent);
                                        },
                                        b =>
                                        {
                                            var passwordInput = b.GetElementById(b.Const("id-password-input"));
                                            b.Log(passwordInput);
                                            var passwordValue = b.GetProperty<string>(passwordInput, "value");
                                            b.If(
                                                b.Not(
                                                    b.HasValue(passwordValue)),
                                                b =>
                                                {
                                                    b.CallOnObject(passwordInput, "focus");
                                                    b.PreventDefault(domEvent);
                                                },
                                                b =>
                                                {
                                                    // Do default
                                                    //b.SetUrl(b.Const("/"));
                                                });
                                        });
                                }));
                        }))
                    ))));
    }
}
