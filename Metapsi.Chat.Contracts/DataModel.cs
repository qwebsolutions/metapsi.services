using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Metapsi.Chat;

public class ConversationMetadata
{
    public string Key { get; set; }
    public string Value { get; set; }
}

public class Conversation
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public List<ConversationMetadata> Metadata { get; set; } = new();
}

public static class ConversationExtensions
{
    public static void Add(this List<ConversationMetadata> conversationMetadata, string key, string value)
    {
        conversationMetadata.Add(new ConversationMetadata() { Key = key, Value = value });
    }
}

public class ConversationEndpoint
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string MessagingGroupId { get; set; }
    public string UserId { get; set; }
}

/// <summary>
/// Text message from any endpoint
/// </summary>
public class ConversationMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FromEndpointId { get; set; }
    public string ChatGroupId { get; set; }
    public string Timestamp { get; set; } = DateTime.UtcNow.Roundtrip();
    public string MessageText { get; set; }
}

public enum ChatUpdateType
{
    NewMessage,
    MessageRemoved,
    MessageEdited,
    UserJoined,
    UserLeft
}

/// <summary>
/// Used for triggering refresh
/// </summary>
public class ChatUpdatedEvent
{
    public string MessagingGroupId { get; set; }
    public ChatUpdateType ChangeType { get; set; }
    public string RelatedId { get; set; }
}