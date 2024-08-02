using System.Collections.Generic;
using System.Linq;

namespace Metapsi.WhatsApp.Messages;

public class ContextObject
{
    public string message_id { get; set; }
}

public class ContactsObject
{
    /// <summary>
    /// Optional.
    /// Full contact address(es) formatted as an addresses object.
    /// </summary>
    public List<AddressObject> addresses { get; set; }

    /// <summary>
    /// Optional.
    /// YYYY-MM-DD formatted string.
    /// </summary>
    public string birthday { get; set; }

    /// <summary>
    /// Optional.
    /// Contact email address(es) formatted as an emails object.
    /// </summary>
    public List<EmailObject> emails { get; set; }

    /// <summary>
    /// Required. Full contact name formatted as a name object.
    /// </summary>
    public NameObject name { get; set; }

    /// <summary>
    /// Optional. Contact organization information formatted as an org object.
    /// </summary>
    public OrgObject org { get; set; }

    /// <summary>
    /// Optional. Contact phone number(s) formatted as a phone object.
    /// </summary>
    public List<PhoneObject> phones { get; set; }

    /// <summary>
    /// Optional. Contact URL(s) formatted as a urls object. 
    /// </summary>
    public List<UrlObject> urls { get; set; }
}

public class AddressObject
{
    /// <summary>
    /// Optional. Street number and name.
    /// </summary>
    public string street { get; set; }
    /// <summary>
    ///  Optional. City name.
    /// </summary>
    public string city { get; set; }
    /// <summary>
    /// Optional. State abbreviation.
    /// </summary>
    public string state { get; set; }
    /// <summary>
    /// Optional. ZIP code.
    /// </summary>
    public string zip { get; set; }
    /// <summary>
    ///  Optional. Full country name.
    /// </summary>
    public string country { get; set; }
    /// <summary>
    /// Optional. Two-letter country abbreviation.
    /// </summary>
    public string country_code { get; set; }
    /// <summary>
    /// Optional. Standard values are HOME and WORK.
    /// </summary>
    public string type { get; set; }
}

public class EmailObject
{
    /// <summary>
    /// Optional. Email address.
    /// </summary>
    public string email { get; set; }

    /// <summary>
    /// Optional. Standard values are HOME and WORK.
    /// </summary>
    public string type { get; set; }
}

/// <summary>
/// * At least one of the optional parameters needs to be included along with the formatted_name parameter.
/// </summary>
public class NameObject
{
    /// <summary>
    /// Required. Full name, as it normally appears.
    /// </summary>
    public string formatted_name { get; set; }

    /// <summary>
    /// Optional*. First name.
    /// </summary>
    public string first_name { get; set; }

    /// <summary>
    /// Optional*. Last name.
    /// </summary>
    public string last_name { get; set; }

    /// <summary>
    /// Optional*. Middle name.
    /// </summary>
    public string middle_name { get; set; }

    /// <summary>
    /// Optional*. Name suffix.
    /// </summary>
    public string suffix { get; set; }

    /// <summary>
    /// Optional*. Name prefix.
    /// </summary>
    public string prefix { get; set; }
}

public class OrgObject
{
    /// <summary>
    /// Optional. Name of the contact's company.
    /// </summary>
    public string company { get; set; }

    /// <summary>
    /// Optional. Name of the contact's department.
    /// </summary>
    public string department { get; set; }

    /// <summary>
    /// Optional. Contact's business title.
    /// </summary>
    public string title { get; set; }
}

public class PhoneObject
{
    /// <summary>
    /// Optional. Automatically populated with the `wa_id` value as a formatted phone number.
    /// </summary>
    public string phone { get; set; }

    /// <summary>
    /// Optional. Standard Values are CELL, MAIN, IPHONE, HOME, and WORK.
    /// </summary>
    public string type { get; set; }

    /// <summary>
    /// Optional. WhatsApp ID.
    /// </summary>
    public string wa_id { get; set; }
}

public class UrlObject
{
    /// <summary>
    /// Optional. URL.
    /// </summary>
    public string url { get; set; }

    /// <summary>
    /// Optional. Standard values are HOME and WORK.
    /// </summary>
    public string type { get; set; }
}

public class InteractiveObject
{
    /// <summary>
    /// Required. Action you want the user to perform after reading the message.
    /// </summary>
    public ActionObject action { get; set; }

    /// <summary>
    /// Optional for type product. Required for other message types.
    /// An object with the body of the message.
    /// </summary>
    public BodyObject body { get; set; }

    /// <summary>
    /// Optional. An object with the footer of the message.
    /// </summary>
    public FooterObject footer { get; set; }

    /// <summary>
    /// Required for type product_list. Optional for other types.
    /// Header content displayed on top of a message.You cannot set a header if your interactive object is of product type.See header object for more information.
    /// </summary>
    public HeaderObject header { get; set; }

    /// <summary>
    /// Required.The type of interactive message you want to send.Supported values:
    /// button: Use for Reply Buttons.
    /// catalog_message: Use for Catalog Messages.
    /// list: Use for List Messages.
    /// product: Use for Single-Product Messages.
    /// product_list: Use for Multi-Product Messages.
    /// flow: Use for Flows Messages.
    /// </summary>
    public string type { get; set; }
}

public class BodyObject
{
    /// <summary>
    /// Required if body is present. The content of the message. Emojis and markdown are supported. Maximum length: 1024 characters.
    /// </summary>
    public string text { get; set; }
}

public class FooterObject
{
    /// <summary>
    /// Required if footer is present. The footer content. Emojis, markdown, and links are supported. Maximum length: 60 characters.
    /// </summary>
    public string text { get; set; }
}

public class ActionObject
{
    // The thing that was called "action" in docs?
    /// <summary>
    /// Required for List Messages.
    /// Button content.It cannot be an empty string and must be unique within the message. Emojis are supported, markdown is not.
    /// Maximum length: 20 characters.
    /// </summary>
    public string button { get; set; }

    /// <summary>
    /// Required for Reply Buttons.
    /// You can have up to 3 buttons.You cannot have leading or trailing spaces when setting the ID.
    /// </summary>
    public List<ButtonObject> buttons { get; set; }

    /// <summary>
    /// Required for Single Product Messages and Multi-Product Messages.
    /// Unique identifier of the Facebook catalog linked to your WhatsApp Business Account.This ID can be retrieved via the Meta Commerce Manager.
    /// </summary>
    public string catalog_id { get; set; }

    /// <summary>
    /// Required for Single Product Messages and Multi-Product Messages.
    /// Unique identifier of the product in a catalog.
    /// To get this ID go to Meta Commerce Manager and select your Meta Business account. You will see a list of shops connected to your account.Click the shop you want to use.On the left-side panel, click Catalog > Items, and find the item you want to mention. The ID for that item is displayed under the item's name.
    /// </summary>
    public string product_retailer_id { get; set; }

    /// <summary>
    /// Required for List Messages and Multi-Product Messages.
    // Array of section objects.Minimum of 1, maximum of 10. See section object.
    /// </summary>
    public List<SectionObject> sections { get; set; }

    /// <summary>
    /// ???
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// ???
    /// </summary>
    public FlowObject parameters { get; set; }
}

public class ButtonObject
{
    /// <summary>
    /// only supported type is reply (for Reply Button)
    /// </summary>
    public string type { get; set; } = "reply";

    public ReplyObject reply { get; set; }
}

public class ReplyObject
{
    /// <summary>
    /// Button title. It cannot be an empty string and must be unique within the message. Emojis are supported, markdown is not. Maximum length: 20 characters.
    /// </summary>
    public string title { get; set; }

    /// <summary>
    /// Unique identifier for your button. This ID is returned in the webhook when the button is clicked by the user. Maximum length: 256 characters.
    /// </summary>
    public string id { get; set; }
}

public class FlowObject
{
    /// <summary>
    /// Optional for Flows Messages. 
    /// The current mode of the Flow, either draft or published.
    /// Default: published
    /// </summary>
    public string mode { get; set; }

    /// <summary>
    /// Required for Flows Messages.
    /// Must be 3.
    /// </summary>
    public string flow_message_version { get; set; }

    /// <summary>
    /// Required for Flows Messages.
    /// A token that is generated by the business to serve as an identifier.
    /// </summary>
    public string flow_token { get; set; }

    /// <summary>
    /// Required for Flows Messages.
    /// Unique identifier of the Flow provided by WhatsApp.
    /// </summary>
    public string flow_id { get; set; }

    /// <summary>
    /// Required for Flows Messages.
    /// Text on the CTA button, eg. "Signup".
    /// Maximum length: 20 characters(no emoji).
    /// </summary>
    public string flow_cta { get; set; }

    /// <summary>
    /// Optional for Flows Messages.
    /// navigate or data_exchange.Use navigate to predefine the first screen as part of the message.Use data_exchange for advanced use-cases where the first screen is provided by your endpoint.
    /// Default: navigate
    /// </summary>
    public string flow_action { get; set; }

    public FlowActionPayloadObject flow_action_payload { get; set; }
}

public class FlowActionPayloadObject
{
    /// <summary>
    /// Required. The id of the first screen of the Flow.
    /// </summary>
    public string screen { get; set; }

    /// <summary>
    /// Optional. The input data for the first screen of the Flow. Must be a non-empty object.
    /// </summary>
    public object data { get; set; }
}

public class HeaderObject
{
    /// <summary>
    /// Required if type is set to document.
    /// Contains the media object for this document.
    /// </summary>
    public MediaObject document { get; set; }

    /// <summary>
    /// Required if type is set to image.
    /// Contains the media object for this image.
    /// </summary>
    public MediaObject image { get; set; }

    /// <summary>
    /// Required if type is set to text.
    /// Text for the header.Formatting allows emojis, but not markdown.
    /// Maximum length: 60 characters.
    /// </summary>
    public string text { get; set; }

    /// <summary>
    /// Required.
    /// The header type you would like to use.Supported values:
    /// text: Used for List Messages, Reply Buttons, and Multi-Product Messages.
    /// video: Used for Reply Buttons.
    /// image: Used for Reply Buttons.
    /// document: Used for Reply Buttons.
    /// </summary>
    public string type { get; set; }

    /// <summary>
    /// Required if type is set to video.
    /// Contains the media object for this video.
    /// </summary>
    public MediaObject video { get; set; }
}

public class SectionObject
{
    /// <summary>
    /// Required for Multi-Product Messages.
    /// Array of product objects.There is a minimum of 1 product per section and a maximum of 30 products across all sections.
    /// </summary>
    public List<ProductObject> product_items { get; set; }

    /// <summary>
    /// Required for List Messages.
    /// Contains a list of rows.You can have a total of 10 rows across your sections.
    /// </summary>
    public List<RowObject> rows { get; set; }

    /// <summary>
    /// Required if the message has more than one section.
    /// Title of the section.
    /// Maximum length: 24 characters.
    /// </summary>
    public string title { get; set; }

    public SectionObject() { }

    public SectionObject(string title, params RowObject[] rows)
    {
        this.title = title;
        this.rows = rows.ToList();
    }

    public SectionObject(params RowObject[] rows)
    {
        this.rows = rows.ToList();
    }
}

public class ProductObject
{
    /// <summary>
    ///  Required for Multi-Product Messages. Unique identifier of the product in a catalog. To get this ID, go to the Meta Commerce Manager, select your account and the shop you want to use. Then, click Catalog > Items, and find the item you want to mention. The ID for that item is displayed under the item's name.
    /// </summary>
    public string product_retailer_id { get; set; }
}

public class RowObject
{
    /// <summary>
    /// Required, maximum length: 200 characters
    /// </summary>
    public string id { get; set; }
    /// <summary>
    /// Required, maximum length: 24 characters
    /// </summary>
    public string title { get; set; }

    /// <summary>
    /// Optional, maximum length: 72 characters
    /// </summary>
    public string description { get; set; }
}

public class LocationObject
{
    /// <summary>
    /// Required.
    /// Location latitude in decimal degrees.
    /// </summary>
    public decimal latitude { get; set; }

    /// <summary>
    /// Required.
    /// Location longitude in decimal degrees.
    /// </summary>
    public decimal longitude { get; set; }

    /// <summary>
    /// Required.
    /// Name of the location.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// Required.
    /// Address of the location.
    /// </summary>
    public string address { get; set; }
}

/// <summary>
/// https://developers.facebook.com/docs/whatsapp/cloud-api/reference/messages#media-object
/// </summary>
public class MediaObject
{
    /// <summary>
    /// Required when type is audio, document, image, sticker, or video and you are not using a link.
    /// The media object ID.Do not use this field when message type is set to text.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// Required when type is audio, document, image, sticker, or video and you are not using an uploaded media ID (i.e. you are hosting the media asset on your public server).
    /// The protocol and URL of the media to be sent.Use only with HTTP/HTTPS URLs.
    /// Do not use this field when message type is set to text.
    /// </summary>
    public string link { get; set; }

    /// <summary>
    /// Optional.
    /// Media asset caption.Do not use with audio or sticker media.
    /// </summary>
    public string caption { get; set; }

    /// <summary>
    /// Optional.
    /// Describes the filename for the specific document.Use only with document media.
    /// The extension of the filename will specify what format the document is displayed as in WhatsApp.
    /// </summary>
    public string filename { get; set; }

    /// <summary>
    /// Optional. On-Premises API only.
    /// This path is optionally used with a link when the HTTP/HTTPS link is not directly accessible and requires additional configurations like a bearer token.For information on configuring providers, see the Media Providers documentation.
    /// </summary>
    public string provider { get; set; }
}

public class TemplateObject
{
    /// <summary>
    /// Required.
    /// Name of the template.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// Required.
    /// Contains a language object. Specifies the language the template may be rendered in.
    /// </summary>
    public LanguageObject language { get; set; }

    /// <summary>
    /// Optional.
    /// Array of components objects containing the parameters of the message.
    /// </summary>
    public List<ComponentObject> components { get; set; }

    /// <summary>
    /// Optional. Only used for On-Premises API.
    /// Namespace of the template.
    /// </summary>
    public string @namespace { get; set; }
}

public class LanguageObject
{
    /// <summary>
    /// Required. The language policy the message should follow. The only supported option is deterministic. See Language Policy Options.
    /// </summary>
    public string policy { get; set; } = "deterministic";

    /// <summary>
    ///  Required. The code of the language or locale to use. Accepts both language and language_locale formats (e.g., en and en_US). For all codes, see Supported Languages. 
    ///  https://developers.facebook.com/docs/whatsapp/api/messages/message-templates#supported-languages
    /// </summary>
    public string code { get; set; }
}

///// <summary>
///// This does not seem to be a separate class at all
///// </summary>
//public class ButtonParameterObject
//{
//    /// <summary>
//    /// Required.
//    /// Indicates the type of parameter for the button.
//    /// Supported Options 
//    /// "payload"
//    /// "text"
//    /// </summary>
//    public string type { get; set; }

//    /// <summary>
//    /// Required for URL buttons.
//    /// Developer-provided suffix that is appended to the predefined prefix URL in the template.
//    /// </summary>
//    public string text { get; set; }
//}

public class ComponentObject
{
    /// <summary>
    /// Required.
    /// Describes the component type.
    /// Supported Options
    /// header
    /// body
    /// button
    /// For text-based templates, we only support the type = body.
    /// </summary>
    public string type { get; set; }

    /// <summary>
    /// Required when type=button. Not used for the other types.
    /// Type of button to create.
    /// Supported Options
    /// quick_reply: Refers to a previously created quick reply button that allows for the customer to return a predefined message.
    /// url: Refers to a previously created button that allows the customer to visit the URL generated by appending the text parameter to the predefined prefix URL in the template.
    /// catalog: Refers to a previously created catalog button that allows for the customer to return a full product catalog.
    /// </summary>
    public string sub_type { get; set; }

    /// <summary>
    /// Required when type=button.
    /// Array of parameter objects with the content of the message.
    /// For components of type= button, see the button parameter object.
    /// </summary>
    public List<ParameterObject> parameters { get; set; }

    /// <summary>
    /// Required when type=button. Not used for the other types.
    /// Position index of the button.You can have up to 10 buttons using index values of 0 to 9.
    /// </summary>
    public string index { get; set; }

    public ComponentObject() { }
    public ComponentObject(string type, params ParameterObject[] parameters)
    {
        this.type = type;
        if (!string.IsNullOrEmpty(sub_type)) this.sub_type = sub_type;
        if (!string.IsNullOrEmpty(index)) this.index = index;
        this.parameters = parameters.ToList();
    }
}

public class CurrencyObject
{
    /// <summary>
    /// Required.
    /// Default text if localization fails.
    /// </summary>
    public string fallback_value { get; set; }

    /// <summary>
    /// Required.
    /// Currency code as defined in ISO 4217.
    /// </summary>
    public string code { get; set; }

    /// <summary>
    /// Required.
    /// Amount multiplied by 1000.
    /// </summary>
    public decimal amount_1000 { get; set; }
}

public class DateTimeObject
{
    /// <summary>
    /// Required.
    /// Default text.For Cloud API, we always use the fallback value, and we do not attempt to localize using other optional fields.
    /// </summary>
    public string fallback_value { get; set; }
}

public class ParameterObject
{
    /// <summary>
    /// Required.
    /// Describes the parameter type.Supported values:
    /// currency
    /// date_time
    /// document
    /// image
    /// text
    /// video
    /// For text-based templates, the only supported parameter types are currency, date_time, and text.
    /// </summary>
    public string type { get; set; }

    /// <summary>
    /// Required when type=text.
    /// The message’s text.Character limit varies based on the following included component type.
    /// For the header component type:
    /// 60 characters
    /// For the body component type:
    /// 1024 characters if other component types are included
    /// 32768 characters if body is the only component type included
    /// </summary>
    public string text { get; set; }

    /// <summary>
    /// Required when type=currency.
    /// A currency object.
    /// </summary>
    public CurrencyObject currency { get; set; }

    /// <summary>
    /// Required when type=date_time.
    /// A date_time object.
    /// </summary>
    public DateTimeObject date_time { get; set; }

    /// <summary>
    /// Required when type=image.
    /// A media object of type image.Captions not supported when used in a media template.
    /// </summary>
    public MediaObject image { get; set; }

    /// <summary>
    /// Required when type=document.
    /// A media object of type document.Only PDF documents are supported for media-based message templates.Captions not supported when used in a media template.
    /// </summary>
    public MediaObject document { get; set; }

    /// <summary>
    /// Required when type=video.
    /// A media object of type video.Captions not supported when used in a media template.
    /// </summary>
    public MediaObject video { get; set; }


    // from button parameter object
    /// <summary>
    /// Required for quick_reply buttons.
    /// Developer-defined payload that is returned when the button is clicked in addition to the display text on the button.
    /// See Callback from a Quick Reply Button Click for an example.
    /// </summary>
    public string payload { get; set; }
}

public class TextObject
{
    /// <summary>
    /// Required for text messages.
    /// The text of the text message which can contain URLs which begin with http:// or https:// and formatting. See available formatting options here.
    /// If you include URLs in your text and want to include a preview box in text messages(preview_url: true), make sure the URL starts with http:// or https:// —https:// URLs are preferred. You must include a hostname, since IP addresses will not be matched.
    /// Maximum length: 4096 characters
    /// </summary>
    public string body { get; set; }

    /// <summary>
    /// Optional. Cloud API only.
    /// Set to true to have the WhatsApp Messenger and WhatsApp Business apps attempt to render a link preview of any URL in the body text string. URLs must begin with http:// or https://. If multiple URLs are in the body text string, only the first URL will be rendered.
    /// If preview_url is omitted, or if unable to retrieve a preview, a clickable link will be rendered instead.
    /// On-Premises API users, use preview_url in the top-level message payload instead.See Parameters.
    /// </summary>
    public bool preview_url { get; set; }
}

public class ReactionObject
{
    /// <summary>
    /// Required.
    /// The WhatsApp Message ID(wamid) of the message on which the reaction should appear.The reaction will not be sent if:
    /// The message is older than 30 days
    /// The message is a reaction message
    /// The message has been deleted
    /// If the ID is of a message that has been deleted, the message will not be delivered.
    /// </summary>
    public string message_id { get; set; }

    /// <summary>
    /// Required.
    /// Emoji to appear on the message.
    /// All emojis supported by Android and iOS devices are supported.
    /// Rendered-emojis are supported.
    /// If using emoji unicode values, values must be Java- or JavaScript-escape encoded.
    /// Only one emoji can be sent in a reaction message
    /// Use an empty string to remove a previously sent emoji.
    /// </summary>
    public string emoji { get; set; }
}
