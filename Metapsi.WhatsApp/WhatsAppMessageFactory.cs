using System.Collections.Generic;

namespace Metapsi.WhatsApp;

public static class WhatsAppMessage
{
    public static WhatsAppOutboundTextMessage Text(string toPhoneNumber, string text)
    {
        var message = new WhatsAppOutboundTextMessage()
        {
            to = toPhoneNumber,
        };

        message.text.body = text;

        return message;
    }

    public static WhatsAppOutboundListMessage List(
        string toPhoneNumber,
        string text,
        string buttonText,
        List<WhatsAppListSection> sections)
    {
        var message = new WhatsAppOutboundListMessage()
        {
            to = toPhoneNumber
        };

        message.interactive.body.text = text;
        message.interactive.action.button = buttonText;
        message.interactive.action.sections.AddRange(sections);
        return message;
    }

    public static WhatsAppOutboundButtonMessage Button(
        string toPhoneNumber,
        string text,
        List<WhatsAppButtonActionReply> buttons)
    {
        var message = new WhatsAppOutboundButtonMessage()
        {
            to = toPhoneNumber
        };

        message.interactive.body.text = text;

        foreach (var button in buttons)
        {
            message.interactive.action.buttons.Add(new WhatsAppButtonActionButton()
            {
                reply = button
            });
        }

        return message;

    }
}
