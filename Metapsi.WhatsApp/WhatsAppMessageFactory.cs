using System.Collections.Generic;
using System.Linq;

namespace Metapsi.WhatsApp;

public static class WhatsAppMessage
{
    public static MessageObject Text(string toPhoneNumber, string text)
    {
        var message = new MessageObject()
        {
            to = toPhoneNumber,
            type = "text",
            text = new Messages.TextObject()
            {
                body = text
            }
        };

        return message;
    }

    public static MessageObject Button(
        string toPhoneNumber,
        string text,
        List<Messages.ReplyObject> buttons)
    {
        var message = new MessageObject()
        {
            to = toPhoneNumber,
            type = "interactive",
            interactive = new Messages.InteractiveObject()
            {
                type = "button",
                body = new Messages.BodyObject()
                {
                    text = text
                },
                action = new Messages.ActionObject()
                {
                    buttons = buttons.Select(x => new Messages.ButtonObject() { type = "reply", reply = x }).ToList()
                }
            }
        };

        return message;
    }

    public static MessageObject Button(
        string toPhoneNumber,
        string text,
        params Messages.ReplyObject[] buttons)
    {
        return Button(toPhoneNumber, text, buttons.ToList());
    }

    public static MessageObject List(
    string toPhoneNumber,
    string text,
    string buttonText,
    List<Messages.SectionObject> sections)
    {
        var message = new MessageObject()
        {
            to = toPhoneNumber,
            type = "interactive",
            interactive = new Messages.InteractiveObject()
            {
                type = "list",
                body = new Messages.BodyObject()
                {
                    text = text
                },
                action = new Messages.ActionObject()
                {
                    button = buttonText,
                    sections = sections
                },
            }
        };

        return message;
    }

    public static MessageObject List(
        string toPhoneNumber,
        string text,
        string buttonText,
        params Messages.SectionObject[] sections)
    {
        return List(toPhoneNumber, text, buttonText, sections.ToList());
    }

    public static MessageObject Template(
        string toPhoneNumber,
        string templateName,
        string language,
        List<Messages.ComponentObject> components)
    {
        var message = new MessageObject()
        {
            to = toPhoneNumber,
            type = "template",
            template = new Messages.TemplateObject()
            {
                name = templateName,
                components = components
            }
        };

        if (!string.IsNullOrEmpty(language))
        {
            message.template.language = new Messages.LanguageObject()
            {
                code = language
            };
        }

        return message;
    }

    public static MessageObject Template(
        string toPhoneNumber,
        string templateName,
        string language,
        params Messages.ComponentObject[] components)
    {
        return Template(toPhoneNumber, templateName, language, components.ToList());
    }
}
