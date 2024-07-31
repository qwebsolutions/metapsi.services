using System.Collections.Generic;

namespace Metapsi.WhatsApp;

#region Inbound Messages

public class WhatsappApiMessage
{
    public string @object { get; set; }
    public List<WhatsappApiEntry> entry { get; set; } = new();
}

public class WhatsappApiEntry
{
    public string id { get; set; } = string.Empty;
    public List<WhatsappApiChange> changes { get; set; } = new();
}

public class WhatsappApiChange
{
    public WhatsappApiChangeValue value { get; set; } = new();
}

public class WhatsappApiChangeValue
{
    public WhatsappApiChangeMetadata metadata { get; set; }
    public List<WhatsappApiChangeContact> contacts { get; set; } = new();
    public List<WhatsappInboundMessage> messages { get; set; } = new();
    public List<WhatsappApiChangeStatus> statuses { get; set; } = new();
}

public class WhatsappApiChangeStatus
{
    public string id { get; set; }
    public string status { get; set; }
    public string timestamp { get; set; }
    public string recipient_id { get; set; }
}

public class WhatsappApiChangeStatusConversation
{
    public string id { get; set; }
    public string expiration_timestamp { get; set; }
    public WhatsappApiChangeStatusConversationOrigin origin { get; set; }
}

public class WhatsappApiChangeStatusConversationOrigin
{
    public string type { get; set; }
}

public class WhatsappApiChangeMetadata
{
    public string display_phone_number { get; set; }
    public string phone_number_id { get; set; }
}

public class WhatsappApiChangeContact
{
    public WhatsappProfile profile { get; set; }
}

public class WhatsappProfile
{
    public string name { get; set; }
}

public class WhatsappInboundMessage
{
    public string id { get; set; }
    public string from { get; set; } // phone number
    public string timestamp { get; set; }
    public string type { get; set; }
    public MessageText text { get; set; }
    public MessageMedia image { get; set; }
    public MessageMedia video { get; set; }
    public MessageMedia audio { get; set; }
    public MessageMedia document { get; set; }
    public InboundMessageContext context { get; set; } = new();
    public InboundInteractive interactive { get; set; }
}

public class InboundMessageContext
{
    public string id { get; set; }
}

public class MessageText
{
    public string body { get; set; }
}

public class MessageMedia
{
    public string filename { get; set; }
    public string caption { get; set; }
    public string mime_type { get; set; }
    public string sha256 { get; set; }
    public string id { get; set; }
}

public class InboundInteractive
{
    public string type { get; set; } = string.Empty;
    public InboundButtonReply button_reply { get; set; }
    public InboundListReply list_reply { get; set; }
}

public class InboundButtonReply
{
    public string id { get; set; }
}

public class InboundListReply
{
    public string id { get; set; }
    public string title { get; set; }
    public string description { get; set; }
}

#endregion

#region Outbound Messages

public interface IWhatsAppOutboundMessage
{
    string to { get; set; }
    string type { get; set; }
}

public class WhatsAppOutboundTextMessage : IWhatsAppOutboundMessage
{
    public string messaging_product { get; set; } = "whatsapp";
    public string recipient_type { get; set; } = "individual";
    public string to { get; set; } // phone number
    public string type { get; set; } = "text";

    public WhatsappOutboundMessageBody text { get; set; } = new();
}

public class WhatsappOutboundMessageBody
{
    public bool preview_url { get; set; } = false;
    public string body { get; set; }
}

public class WhatsAppOutboundListMessage : IWhatsAppOutboundMessage
{
    public string messaging_product { get; set; } = "whatsapp";
    public string recipient_type { get; set; } = "individual";
    public string to { get; set; } // phone number
    public string type { get; set; } = "interactive";
    public WhatsAppOutboundInteractiveList interactive { get; set; } = new();
}

public class WhatsAppOutboundInteractiveList
{
    public string type { get; set; } = "list";
    public WhatsappOutboundInteractiveMessageBody body { get; set; } = new();
    public WhatsAppListActionsContainer action { get; set; } = new();
}

public class WhatsappOutboundInteractiveMessageBody
{
    public string text { get; set; } = string.Empty;
}

public class WhatsAppListActionsContainer
{
    public string button { get; set; } = string.Empty;
    public List<WhatsAppListSection> sections { get; set; } = new();
}

public class WhatsAppListSection
{
    public string title { get; set; } = string.Empty;
    public List<WhatsAppListSectionRow> rows { get; set; } = new();
}

public class WhatsAppListSectionRow
{
    public string id { get; set; } = string.Empty;
    public string title { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
}


public class WhatsAppOutboundButtonMessage : IWhatsAppOutboundMessage
{
    public string messaging_product { get; set; } = "whatsapp";
    public string recipient_type { get; set; } = "individual";
    public string to { get; set; } // phone number
    public string type { get; set; } = "interactive";
    public WhatsAppOutboundButtonList interactive { get; set; } = new();
}

public class WhatsAppOutboundButtonList
{
    public string type { get; set; } = "button";
    public WhatsappOutboundInteractiveMessageBody body { get; set; } = new();
    public WhatsAppButtonActionsContainer action { get; set; } = new();
}

public class WhatsAppButtonActionsContainer
{
    public List<WhatsAppButtonActionButton> buttons { get; set; } = new();
}

public class WhatsAppButtonActionButton
{
    public string type { get; set; } = "reply";
    public WhatsAppButtonActionReply reply { get; set; } = new();
}

public class WhatsAppButtonActionReply
{
    public string id { get; set; } = string.Empty;
    public string title { get; set; } = string.Empty;
}


/*  "recipient_type": "individual",
  "to" : "whatsapp-id",
  "type": "interactive" 
  "interactive":{
    "type": "list" | "button",
    "header": {},
    "body": {},
    "footer": {},
    "action": {}
  }

"interactive":{
  "type": "list",
  "header": {
    "type": "text",
    "text": "your-header-content"
  },
  "body": {
    "text": "your-text-message-content"
  },
  "footer": {
    "text": "your-footer-content"
  },
  "action": {
    "button": "cta-button-content",
    "sections":[
      {
        "title":"your-section-title-content",
        "rows": [
          {
            "id":"unique-row-identifier",
            "title": "row-title-content",
            "description": "row-description-content",           
          }
        ]
      },
      {
        "title":"your-section-title-content",
        "rows": [
          {
            "id":"unique-row-identifier",
            "title": "row-title-content",
            "description": "row-description-content",           
          }
        ]
      },
      ...
    ]
  }
} 
 
 
 */

#endregion

#region Post results

public class WhatsappSendMessageResult
{
    public List<WhatsappSendTextMessageResultId> messages { get; set; } = new();
    public ErrorMessage error { get; set; } = new ErrorMessage();
}

public class ErrorMessage
{
    public string message { get; set; }
}

public class WhatsappSendTextMessageResultId
{
    public string id { get; set; }
}


#endregion

#region Get results

/*{
  "messaging_product": "whatsapp",
  "url": "<URL>",
  "mime_type": "<MIME_TYPE>",
  "sha256": "<HASH>",
  "file_size": "<FILE_SIZE>",
  "id": "<MEDIA_ID>"
}*/

public class MediaUrl
{
    public string messaging_product { get; set; }
    public string url { get; set; }
    public string mime_type { get; set; }
    public string sha256 { get; set; }
    public string file_size { get; set; }
    public string id { get; set; }
}

#endregion
