using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection.Metadata;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Numerics;
using static Metapsi.Timer.Command;
using Microsoft.VisualBasic;
using System.Drawing;

namespace Metapsi.WhatsApp.Webhooks;

public class NotificationPayloadObject
{
    public string @object { get; set; } = "whatsapp_business_account";
    public List<EntryObject> entry { get; set; }
}

public class EntryObject
{
    /// <summary>
    /// The WhatsApp Business Account ID for the business that is subscribed to the webhook.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// Array of objects. An array of change objects.
    /// </summary>
    public List<ChangeObject> changes { get; set; }
}

public class ChangeObject
{
    /// <summary>
    /// A value object.
    /// </summary>
    public ValueObject value { get; set; }

    /// <summary>
    /// Notification type. Value will be messages.
    /// </summary>
    public string field { get; set; }
}

public class ValueObject
{
    /// <summary>
    /// Array of contact objects with information for the customer who sent a message to the business. 
    /// </summary>
    public List<ContactObject> contacts { get; set; }

    /// <summary>
    /// An array of error objects describing the error.
    /// </summary>
    public List<ErrorObject> errors { get; set; }

    /// <summary>
    /// Product used to send the message. Value is always whatsapp.
    /// </summary>
    public string messaging_product { get; set; }

    /// <summary>
    /// Information about a message received by the business that is subscribed to the webhook.
    /// </summary>
    public List<MessageObject> messages { get; set; }

    /// <summary>
    /// A metadata object describing the business subscribed to the webhook.
    /// </summary>
    public MetadataObject metadata { get; set; }

    /// <summary>
    /// Status object for a message that was sent by the business that is subscribed to the webhook.
    /// </summary>
    public List<StatusObject> statuses { get; set; }
}

public class ContactObject
{
    /// <summary>
    /// The customer's WhatsApp ID. A business can respond to a customer using this ID. This ID may not match the customer's phone number, which is returned by the API as input when sending a message to the customer.
    /// </summary>
    public string wa_id { get; set; }

    /// <summary>
    /// Additional unique, alphanumeric identifier for a WhatsApp user.
    /// </summary>
    public string user_id { get; set; }

    /// <summary>
    ///  A customer profile object.
    /// </summary>
    public ProfileObject profile { get; set; }
}

public class ProfileObject
{
    /// <summary>
    /// The customer's name.
    /// </summary>
    public string name { get; set; }
}

/// <summary>
/// Webhooks triggered by v16.0 and newer requests
/// </summary>
public class ErrorObject
{
    /// <summary>
    /// Error code. Example: 130429.
    /// </summary>
    public int code { get; set; }

    /// <summary>
    ///  Error code title. Example: Rate limit hit.
    /// </summary>
    public string title { get; set; }

    /// <summary>
    ///  Error code message. This value is the same as the title value. For example: Rate limit hit. Note that the message property in API error response payloads pre-pends this value with the a # symbol and the error code in parenthesis. For example: (#130429) Rate limit hit.
    /// </summary>
    public string message { get; set; }

    /// <summary>
    /// An error data object
    /// </summary>
    public ErrorDataObject error_data { get; set; }
}

public class ErrorDataObject
{
    /// <summary>
    /// Describes the error. Example: Message failed to send because there were too many messages sent from this phone number in a short period of time.
    /// </summary>
    public string details { get; set; }
}

public class MetadataObject
{
    /// <summary>
    /// The phone number that is displayed for a business.
    /// </summary>
    public string display_phone_number { get; set; }

    /// <summary>
    /// ID for the phone number. A business can respond to a message using this ID.
    /// </summary>
    public string phone_number_id { get; set; }
}

public class MessageObject
{
    /// <summary>
    /// When the messages type is set to audio, including voice messages, this object is included in the messages object
    /// </summary>
    public AudioObject audio { get; set; }

    /// <summary>
    /// When the messages type field is set to button, this object is included in the messages object
    /// </summary>
    public ButtonObject button { get; set; }

    /// <summary>
    /// Context object. Only included when a user replies or interacts with one of your messages.
    /// </summary>
    public ContextObject context { get; set; }

    /// <summary>
    /// A document object. When messages type is set to document, this object is included in the messages object. 
    /// </summary>
    public DocumentObject document { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public List<ErrorObject> errors { get; set; }

    /// <summary>
    /// The customer's WhatsApp ID. A business can respond to a customer using this ID. This ID may not match the customer's phone number, which is returned by the API as input when sending a message to the customer.
    /// </summary>
    public string from { get; set; }

    /// <summary>
    /// The ID for the message that was received by the business. You could use messages endpoint to mark this specific message as read.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// An identity object. Webhook is triggered when a customer's phone number or profile information has been updated. See messages system identity.
    /// </summary>
    public IdentityObject identity { get; set; }

    /// <summary>
    /// When messages type is set to image, this object is included in the messages object.
    /// </summary>
    public ImageObject image { get; set; }

    /// <summary>
    /// When a customer has interacted with your message, this object is included in the messages object.
    /// </summary>
    public InteractiveObject interactive { get; set; }

    /// <summary>
    /// Included in the messages object when a customer has placed an order.
    /// </summary>
    public OrderObject order { get; set; }

    /// <summary>
    /// Referral object. When a customer clicks an ad that redirects to WhatsApp, this object is included in the messages object. 
    /// The referral object can be included in the following types of message: text, location, contact, image, video, document, voice, and sticker.
    /// </summary>
    public ReferralObject referral { get; set; }

    /// <summary>
    /// When messages type is set to sticker, this object is included in the messages object.
    /// </summary>
    public StickerObject sticker { get; set; }

    /// <summary>
    /// When messages type is set to system, a customer has updated their phone number or profile information, this object is included in the messages object.
    /// </summary>
    public SystemObject system { get; set; }

    /// <summary>
    /// When messages type is set to text, this object is included.
    /// </summary>
    public TextObject text { get; set; }

    /// <summary>
    /// Unix timestamp indicating when the WhatsApp server received the message from the customer.
    /// </summary>
    public string timestamp { get; set; }

    /// <summary>
    /// The type of message that has been received by the business that has subscribed to Webhooks. Possible value can be one of the following:
    /// audio
    /// button
    /// document
    /// text
    /// image
    /// interactive
    /// order
    /// sticker
    /// system – for customer number change messages
    /// unknown
    /// video
    /// </summary>
    public string type { get; set; }

    /// <summary>
    /// When messages type is set to video, this object is included in messages object.
    /// </summary>
    public VideoObject video { get; set; }
}

public class AudioObject
{
    /// <summary>
    /// ID for the audio file.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// Mime type of the audio file.
    /// </summary>
    public string mime_type { get; set; }
}

public class ButtonObject
{
    /// <summary>
    /// The payload for a button set up by the business that a customer clicked as part of an interactive message.
    /// </summary>
    public string payload { get; set; }

    /// <summary>
    /// Button text.
    /// </summary>
    public string text { get; set; }
}

public class ContextObject
{
    /// <summary>
    /// Set to true if the message received by the business has been forwarded.
    /// </summary>
    public bool forwarded { get; set; }

    /// <summary>
    ///  Set to true if the message received by the business has been forwarded more than 5 times.
    /// </summary>
    public bool frequently_forwarded { get; set; }

    /// <summary>
    /// The WhatsApp ID for the customer who replied to an inbound message.
    /// </summary>
    public string from { get; set; }

    /// <summary>
    /// The message ID for the sent message for an inbound reply.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// Referred product object describing the product the user is requesting information about. You must parse this value if you support Product Enquiry Messages. See Receive Response From Customers.
    /// </summary>
    public ReferredProductObject referred_product { get; set; }
}

public class ReferredProductObject
{
    /// <summary>
    /// Unique identifier of the Meta catalog linked to the WhatsApp Business Account.
    /// </summary>
    public string catalog_id { get; set; }

    /// <summary>
    /// Unique identifier of the product in a catalog.
    /// </summary>
    public string product_retailer_id { get; set; }
}

public class DocumentObject
{
    /// <summary>
    /// Caption for the document, if provided.
    /// </summary>
    public string caption { get; set; }

    /// <summary>
    /// Name for the file on the sender's device.
    /// </summary>
    public string filename { get; set; }

    /// <summary>
    /// SHA 256 hash
    /// </summary>
    public string sha256 { get; set; }

    /// <summary>
    /// Mime type of the document file.
    /// </summary>
    public string mime_type { get; set; }

    /// <summary>
    /// ID for the document.
    /// </summary>
    public string id { get; set; }
}

public class IdentityObject
{
    // Bool or string?! Docs don't say. Better avoid it so deserializer does not crash

    //State of acknowledgment for the messages system customer_identity_changed.
    // public bool acknowledged { get; set; }

    /// <summary>
    /// The time when the WhatsApp Business Management API detected the customer may have changed their profile information.
    /// </summary>
    public string created_timestamp { get; set; }

    /// <summary>
    /// The ID for the messages system customer_identity_changed
    /// </summary>
    public string hash { get; set;}
}

public class ImageObject
{
    /// <summary>
    /// Caption for the image, if provided.
    /// </summary>
    public string caption { get; set; }

    /// <summary>
    ///  Image hash.
    /// </summary>
    public string sha256 { get; set; }

    /// <summary>
    ///  ID for the image.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// Mime type for the image.
    /// </summary>
    public string mime_type { get; set; }
}

public class InteractiveObject
{
    /// <summary>
    /// 
    /// </summary>
    public TypeObject type { get; set; }
}

public class TypeObject
{
    /// <summary>
    /// Sent when a customer clicks a button.
    /// </summary>
    public ButtonReplyObject button_reply { get; set; }

    /// <summary>
    ///  Sent when a customer selects an item from a list.
    /// </summary>
    public ListReplyObject list_reply { get; set; }
}

public class ButtonReplyObject
{
    /// <summary>
    /// Unique ID of a button.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// Title of a button.
    /// </summary>
    public string title { get; set; }
}

public class ListReplyObject
{
    /// <summary>
    /// Unique ID of the selected list item.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// Title of the selected list item.
    /// </summary>
    public string title { get; set; }

    /// <summary>
    ///  Description of the selected row.
    /// </summary>
    public string description { get; set; }
}

public class OrderObject
{
    /// <summary>
    /// ID for the catalog the ordered item belongs to.
    /// </summary>
    public string catalog_id { get; set; }

    /// <summary>
    /// Text message from the user sent along with the order.
    /// </summary>
    public string text { get; set; }

    /// <summary>
    /// Array of product item objects 
    /// </summary>
    public List<ProductItemObject> product_items { get; set; }
}

public class ProductItemObject
{
    /// <summary>
    /// Unique identifier of the product in a catalog.
    /// </summary>
    public string product_retailer_id { get; set; }

    /// <summary>
    /// Number of items.
    /// </summary>
    public string quantity { get; set; } // Is it really a string?

    /// <summary>
    /// Price of each item.
    /// </summary>
    public string item_price { get; set; }

    /// <summary>
    /// Price currency.
    /// </summary>
    public string currency { get; set; }
}

public class ReferralObject
{
    /// <summary>
    /// The Meta URL that leads to the ad or post clicked by the customer. Opening this url takes you to the ad viewed by your customer.
    /// </summary>
    public string source_url { get; set; }

    /// <summary>
    /// The type of the ad’s source; ad or post.
    /// </summary>
    public string source_type { get; set; }

    /// <summary>
    /// Meta ID for an ad or a post.
    /// </summary>
    public string source_id { get; set; }

    /// <summary>
    /// Headline used in the ad or post.
    /// </summary>
    public string headline { get; set; }

    /// <summary>
    /// Body for the ad or post.
    /// </summary>
    public string body { get; set; }

    /// <summary>
    /// Media present in the ad or post; image or video.
    /// </summary>
    public string media_type { get; set; }

    /// <summary>
    /// URL of the image, when media_type is an image.
    /// </summary>
    public string image_url { get; set; }

    /// <summary>
    /// RL of the video, when media_type is a video.
    /// </summary>
    public string video_url { get; set; }

    /// <summary>
    ///  URL for the thumbnail, when media_type is a video.
    /// </summary>
    public string thumbnail_url { get; set; }

    /// <summary>
    /// Click ID generated by Meta for ads that click to WhatsApp.
    /// </summary>
    public string ctwa_clid { get; set; }
}

public class StickerObject
{
    /// <summary>
    /// image/webp.
    /// </summary>
    public string mime_type { get; set; }

    /// <summary>
    /// Hash for the sticker.
    /// </summary>
    public string sha256 { get; set; }

    /// <summary>
    /// ID for the sticker.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// Set to true if the sticker is animated; false otherwise.
    /// </summary>
    public bool animated { get; set; }
}

public class SystemObject
{
    /// <summary>
    /// Describes the change to the customer's identity or phone number.
    /// </summary>
    public string body { get; set; }

    /// <summary>
    /// Hash for the identity fetched from server.
    /// </summary>
    public string identity { get; set; }

    /// <summary>
    /// New WhatsApp ID for the customer when their phone number is updated. Available on webhook versions v11.0 and earlier.
    /// </summary>
    public string new_wa_id { get; set; }

    /// <summary>
    /// New WhatsApp ID for the customer when their phone number is updated. Available on webhook versions v12.0 and later.
    /// </summary>
    public string wa_id { get; set; }

    /// <summary>
    /// Type of system update. Will be one of the following:
    /// customer_changed_number – A customer changed their phone number.
    /// customer_identity_changed – A customer changed their profile information.
    /// </summary>
    public string type { get; set; }

    /// <summary>
    /// The WhatsApp ID for the customer prior to the update.
    /// </summary>
    public string customer { get; set; }
}

public class TextObject
{
    /// <summary>
    /// The text of the message.
    /// </summary>
    public string body { get; set; }
}

public class VideoObject
{
    /// <summary>
    /// The caption for the video, if provided.
    /// </summary>
    public string caption { get; set; }

    /// <summary>
    /// The name for the file on the sender's device.
    /// </summary>
    public string filename { get; set; }

    /// <summary>
    /// The hash for the video.
    /// </summary>
    public string sha256 { get; set; }

    /// <summary>
    /// The ID for the video.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// The mime type for the video file.
    /// </summary>
    public string mime_type { get; set; }
}

public class StatusObject
{
    /// <summary>
    /// Arbitrary string included in sent message.
    /// </summary>
    public string biz_opaque_callback_data { get; set; }

    /// <summary>
    /// Information about the conversation.
    /// </summary>
    public ConversationObject conversation { get; set; }

    /// <summary>
    /// An array of error objects describing the error. 
    /// </summary>
    public List<ErrorObject> errors { get; set; }

    /// <summary>
    /// The ID for the message that the business that is subscribed to the webhooks sent to a customer
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// An object containing pricing information.
    /// </summary>
    public PricingObject pricing { get; set; }

    /// <summary>
    /// The customer's WhatsApp ID. A business can respond to a customer using this ID. This ID may not match the customer's phone number, which is returned by the API as input when sending a message to the customer.
    /// </summary>
    public string recipient_id { get; set; }

    /// <summary>
    /// delivered – A webhook is triggered when a message sent by a business has been delivered
    /// read – A webhook is triggered when a message sent by a business has been read
    /// sent – A webhook is triggered when a business sends a message to a customer
    /// </summary>
    public string status { get; set; }

    /// <summary>
    /// Date for the status message
    /// </summary>
    public string timestamp { get; set; }
}

public class ConversationObject
{
    /// <summary>
    /// Represents the ID of the conversation the given status notification belongs to
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// Describes conversation category
    /// </summary>
    public OriginObject origin { get; set; }

    /// <summary>
    /// Date when the conversation expires. This field is only present for messages with a `status` set to `sent`.
    /// </summary>
    public string expiration_timestamp { get; set; }
}

public class OriginObject
{
    /// <summary>
    /// Indicates conversation category. This can also be referred to as a conversation entry point
    /// authentication – Indicates the conversation was opened by a business sending template categorized as AUTHENTICATION to the customer.This applies any time it has been more than 24 hours since the last customer message.
    /// marketing – Indicates the conversation was opened by a business sending template categorized as MARKETING to the customer. This applies any time it has been more than 24 hours since the last customer message.
    /// utility – Indicates the conversation was opened by a business sending template categorized as UTILITY to the customer. This applies any time it has been more than 24 hours since the last customer message.
    /// service – Indicates that the conversation opened by a business replying to a customer within a customer service window.
    /// referral_conversion – Indicates a free entry point conversation.
    /// </summary>
    public string type { get; set; }
}

public class PricingObject
{
    /// <summary>
    /// Indicates the conversation category:
    /// authentication – Indicates an authentication conversation.
    /// authentication-international – Indicates an authentication-international conversation.
    /// marketing – Indicates an marketing conversation.
    /// utility – Indicates a utility conversation.
    /// service – Indicates an service conversation.
    /// referral_conversion – Indicates a free entry point conversation.
    /// </summary>
    public string category { get; set; }

    /// <summary>
    /// Type of pricing model used by the business. Current supported value is CBP
    /// </summary>
    public string pricing_model { get; set; }
}