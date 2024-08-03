using System.Collections.Generic;

namespace Metapsi.WhatsApp;

/// <summary>
/// The message that you post
/// </summary>
public class MessageObject
{
    /// <summary>
    /// Required when type=audio.
    /// A media object containing audio.
    /// </summary>
    public Messages.MediaObject audio { get; set; }

    /// <summary>
    /// Optional.
    /// An arbitrary string, useful for tracking.
    /// For example, you could pass the message template ID in this field to track your customer's journey starting from the first message you send. You could then track the ROI of different message template types to determine the most effective one.
    /// Any app subscribed to the messages webhook field on the WhatsApp Business Account can get this string, as it is included in statuses object within webhook payloads.
    /// Cloud API does not process this field, it just returns it as part of sent/delivered/read message webhooks.
    /// Maximum 512 characters.
    /// </summary>
    public string biz_opaque_callback_data { get; set; }

    /// <summary>
    /// <para>DOCS issue: object in description, list in example</para>
    /// Required when type=contacts. A contacts object.
    /// </summary>
    public List<Messages.ContactsObject> contacts { get; set; }

    /// <summary>
    /// Required if replying to any message in the conversation. An object containing the ID of a previous message you are replying to.
    /// </summary>
    public Messages.ContextObject context { get; set; }

    /// <summary>
    /// Required when type=document. A media object containing a document.
    /// </summary>
    public Messages.MediaObject document { get; set; }

    /// <summary>
    /// Required when type=image. A media object containing an image.
    /// </summary>
    public Messages.MediaObject image { get; set; }

    /// <summary>
    /// Required when type=interactive. An interactive object. The components of each interactive object generally follow a consistent pattern: header, body, footer, and action.
    /// </summary>
    public Messages.InteractiveObject interactive { get; set; }

    /// <summary>
    /// Required when type=location.
    /// A location object.
    /// </summary>
    public Messages.LocationObject location { get; set; }

    /// <summary>
    /// Required
    /// Messaging service used for the request.Use "whatsapp".
    /// </summary>
    public string messaging_product { get; set; } = "whatsapp";


    /// <summary>
    /// Required if type=text.
    /// Allows for URL previews in text messages — See the Sending URLs in Text Messages.
    /// This field is optional if not including a URL in your message. Values: false (default), true.
    /// </summary>
    public bool preview_url { get; set; }

    /// <summary>
    /// ?Missing in documentation?
    /// </summary>
    public Messages.ReactionObject reaction { get; set; }

    /// <summary>
    /// Optional.
    /// Currently, you can only send messages to individuals.Set this as individual.
    /// Default: individual
    /// </summary>
    public string recipient_type { get; set; } = "individual";

    /// <summary>
    /// A message's status. You can use this field to mark a message as read.
    /// </summary>
    public string status { get; set; }

    /// <summary>
    /// Required when type=sticker.
    /// A media object containing a sticker.
    /// Cloud API: Static and animated third-party outbound stickers are supported in addition to all types of inbound stickers.A static sticker needs to be 512x512 pixels and cannot exceed 100 KB.An animated sticker must be 512x512 pixels and cannot exceed 500 KB.
    /// On-Premises API: Only static third-party outbound stickers are supported in addition to all types of inbound stickers.A static sticker needs to be 512x512 pixels and cannot exceed 100 KB.Animated stickers are not supported.
    /// </summary>
    public Messages.MediaObject sticker { get; set; }

    /// <summary>
    /// Required when type=template. A template object.
    /// </summary>
    public Messages.TemplateObject template { get; set; }

    /// <summary>
    /// Required for text messages. A text object.
    /// </summary>
    public Messages.TextObject text { get; set; }

    /// <summary>
    /// Required.
    /// WhatsApp ID or phone number of the customer you want to send a message to.See Phone Number Formats.
    /// If needed, On-Premises API users can get this number by calling the contacts endpoint.
    /// </summary>
    public string to { get; set; }

    /// <summary>
    /// Optional. The type of message you want to send.If omitted, defaults to text.
    /// </summary>
    public string type { get; set; }

    public Messages.MediaObject video { get; set; }
}

public interface ICloudApiResponse
{
    ErrorObject error { get; }
}

public class PostMessageResponse : ICloudApiResponse
{
    public string messaging_product { get; set; }
    public List<ContactObject> contacts { get; set; }
    public List<MessageObject> messages { get; set; }
    public ErrorObject error { get; set; }

    public class ContactObject
    {
        public string input { get; set; }
        public string wa_id { get; set; }
    }

    public class MessageObject
    {
        public string id { get; set; }
        /// <summary>
        /// "message_status":"accepted" : means the message was sent to the intended recipient
        /// "message_status":"held_for_quality_assessment": means the message send was delayed until quality can be validated and it will either be sent or dropped at this point
        /// </summary>
        public string message_status { get; set; }
    }
}

public class UploadMediaResponse : ICloudApiResponse
{
    public string id { get; set; }

    public ErrorObject error { get; set; }
}

/// <summary>
/// The message that you receive
/// </summary>
public class NotificationPayloadObject
{
    public string @object { get; set; } = "whatsapp_business_account";
    public List<Webhooks.EntryObject> entry { get; set; }
}

public class ErrorObject
{
    public string message { get; set; }
    public string type { get; set; }
    public int code { get; set; }
    public ErrorDataObject error_data { get; set; }
    public string fbtrace_id { get; set; }

    public class ErrorDataObject
    {
        public string messaging_product { get; set; }
        public string details { get; set; }
    }
}