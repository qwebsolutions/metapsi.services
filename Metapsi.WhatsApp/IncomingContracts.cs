using System;

namespace Metapsi.WhatsApp;

public class IncomingTextMessage
{
    public string PhoneNumber { get; set; }
    public string Text { get; set; }
    public string Timestamp { get; set; } = DateTime.UtcNow.Roundtrip();
}

public class IncomingButtonReplyMessage
{
    public string PhoneNumber { get; set; }
    public string RelatedMessageId { get; set; }
    public string ButtonId { get; set; }
    public string Timestamp { get; set; } = DateTime.UtcNow.Roundtrip();
}

public class IncomingListReplyMessage
{
    public string PhoneNumber { get; set; }
    public string RelatedMessageId { get; set; }
    public string ItemId { get; set; }
    public string ItemTitle { get; set; }
    public string ItemDescription { get; set; }
    public string Timestamp { get; set; } = DateTime.UtcNow.Roundtrip();
}

public class IncomingStatusUpdate
{
    public string PhoneNumber { get; set; }
    public string MessageId { get; set; }
    public string Status { get; set; }
    public string Timestamp { get; set; } = DateTime.UtcNow.Roundtrip();
}