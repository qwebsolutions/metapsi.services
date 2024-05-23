using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Metapsi.Chat;

/// <summary>
/// Groups endpoints (user perspectives) and messages
/// </summary>
public class Conversation
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
}

/// <summary>
/// The perspective a particular user has of a conversation
/// </summary>
public class UserConversationEndpoint
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string JoinedTimestamp = DateTime.UtcNow.Roundtrip();
    public string ConversationId { get; set; }
    public string UserId { get; set; }
}

/// <summary>
/// Text message from any endpoint
/// </summary>
public class Message
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Timestamp { get; set; } = DateTime.UtcNow.Roundtrip();
    public string FromEndpointId { get; set; }
    public string MessageText { get; set; }
}

//public enum ChatUpdateType
//{
//    NewMessage,
//    MessageRemoved,
//    MessageEdited,
//    UserJoined,
//    UserLeft
//}

///// <summary>
///// Used for triggering refresh
///// </summary>
//public class ChatUpdatedEvent
//{
//    public string MessagingGroupId { get; set; }
//    public ChatUpdateType ChangeType { get; set; }
//    public string RelatedId { get; set; }
//}