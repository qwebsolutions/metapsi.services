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

    public static MessageObject AudioMessage(
        string toPhoneNumber,
        string mediaId)
    {
        return new MessageObject()
        {
            to = toPhoneNumber,
            type = "audio",
            audio = new Messages.MediaObject()
            {
                id = mediaId
            }
        };
    }

    public static MessageObject DocumentMessage(
        string toPhoneNumber,
        string mediaId,
        string fileName)
    {
        return new MessageObject()
        {
            to = toPhoneNumber,
            type = "document",
            document = new Messages.MediaObject()
            {
                filename = fileName,
                id = mediaId
            }
        };
    }

    public static MessageObject ImageMesage(
        string toPhoneNumber,
        string mediaId,
        string caption = null)
    {
        var message = new MessageObject()
        {
            to = toPhoneNumber,
            type = "image",
            image = new Messages.MediaObject()
            {
                id = mediaId
            }
        };

        if(!string.IsNullOrEmpty(caption))
        {
            message.image.caption = caption;
        }

        return message;
    }

    public static MessageObject VideoMessage(
        string toPhoneNumber,
        string mediaId,
        string caption = null)
    {
        var message = new MessageObject()
        {
            to = toPhoneNumber,
            type = "video",
            video = new Messages.MediaObject()
            {
                caption = caption,
                id = mediaId
            }
        };

        if (!string.IsNullOrEmpty(caption))
        {
            message.video.caption = caption;
        }

        return message;
    }

    public static MessageObject MediaMessage(
        string toPhoneNumber,
        string mediaId,
        string contentType,
        string caption,
        string fileName)
    {
        if (MediaType.SupportedAudioTypes.Any(x => x.MimeType == contentType))
        {
            return WhatsAppMessage.AudioMessage(toPhoneNumber, mediaId);
        }
        else if (MediaType.SupportedImageTypes.Any(x => x.MimeType == contentType))
        {
            return WhatsAppMessage.ImageMesage(toPhoneNumber, mediaId, caption);
        }
        else if (MediaType.SupportedVideoTypes.Any(x => x.MimeType == contentType))
        {
            return WhatsAppMessage.VideoMessage(toPhoneNumber, mediaId, caption);
        }
        else return WhatsAppMessage.DocumentMessage(toPhoneNumber, mediaId, fileName);
    }
}
