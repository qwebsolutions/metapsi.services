using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Metapsi.WhatsApp;

public class TextNotification
{
    public string FromPhoneNumber { get; set; }
    public string Text { get; set; }
    public string Timestamp { get; set; } = DateTime.UtcNow.Roundtrip();
}

public class MediaNotification
{
    public string FromPhoneNumber { get; set; }
    public string Caption { get; set; }
    public string MediaId { get; set; }
    public string FileName { get; set; }
    public string MimeType { get; set; }
    public string Timestamp { get; set; } = DateTime.UtcNow.Roundtrip();
}

public class ButtonReplyNotification
{
    public string FromPhoneNumber { get; set; }
    public string ContextMessageId { get; set; }
    public string ContextPhoneNumber { get; set; }
    public string ButtonId { get; set; }
    public string Timestamp { get; set; } = DateTime.UtcNow.Roundtrip();
}

public class ListReplyNotification
{
    public string FromPhoneNumber { get; set; }
    public string ContextMessageId { get; set; }
    public string ContextPhoneNumber { get; set; }
    public string ItemId { get; set; }
    public string ItemTitle { get; set; }
    public string ItemDescription { get; set; }
    public string Timestamp { get; set; } = DateTime.UtcNow.Roundtrip();
}

public class StatusNotification
{
    public string FromPhoneNumber { get; set; }
    public string MessageId { get; set; }
    public string Status { get; set; }
    public string Timestamp { get; set; } = DateTime.UtcNow.Roundtrip();
}

public static class WhatsAppServiceContractsExtensions
{
    public static List<Webhooks.ChangeObject> Changes(this NotificationPayloadObject notification)
    {
        List<Webhooks.ChangeObject> changes = new();

        if (notification.entry != null)
        {
            foreach (var entry in notification.entry)
            {
                if (entry != null)
                {
                    if (entry.changes != null)
                    {
                        changes.AddRange(entry.changes);
                    }
                }
            }
        }

        return changes;
    }

    public static List<Webhooks.StatusObject> Statuses(this NotificationPayloadObject notification)
    {
        List<Webhooks.StatusObject> statuses = new List<Webhooks.StatusObject>();

        foreach (var change in notification.Changes())
        {
            if (change.value != null)
            {
                if (change.value.statuses != null)
                {
                    statuses.AddRange(change.value.statuses);
                }
            }
        }


        return statuses;
    }

    public static StatusNotification ToNotification(this Webhooks.StatusObject statusObject)
    {
        var userPhone = statusObject.recipient_id;
        var offset = DateTimeOffset.FromUnixTimeSeconds(long.Parse(statusObject.timestamp));
        var messageId = statusObject.id;

        return new StatusNotification()
        {
            FromPhoneNumber = userPhone,
            Timestamp = offset.UtcDateTime.Roundtrip(),
            MessageId = messageId,
            Status = statusObject.status
        };
    }

    public static List<Webhooks.MessageObject> Messages(this NotificationPayloadObject notification)
    {
        List<Webhooks.MessageObject> messages = new List<Webhooks.MessageObject>();
        foreach (var change in notification.Changes())
        {
            if (change.value != null)
            {
                if (change.value.messages != null)
                {
                    messages.AddRange(change.value.messages);
                }
            }
        }

        return messages;
    }

    public static IEnumerable<TextNotification> TextNotifications(this NotificationPayloadObject notification)
    {
        return notification.Messages().Where(x => x.type == "text").Select(x =>
        {
            var messageText = x.text.body;
            var offset = DateTimeOffset.FromUnixTimeSeconds(long.Parse(x.timestamp));

            return new TextNotification()
            {
                FromPhoneNumber = x.from,
                Text = x.text.body,
                Timestamp = offset.UtcDateTime.Roundtrip()
            };
        });
    }

    public static IEnumerable<ButtonReplyNotification> ButtonReplyNotifications(this NotificationPayloadObject notification)
    {
        return notification.Messages().Where(x => x.interactive != null).Where(x => x.interactive.button_reply != null).Select(
            x => new ButtonReplyNotification()
            {
                FromPhoneNumber = x.from,
                ButtonId = x.interactive.button_reply.id,
                ContextMessageId = x.context.id,
                ContextPhoneNumber = x.context.from,
                Timestamp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(x.timestamp)).UtcDateTime.Roundtrip()
            });
    }

    public static IEnumerable<ListReplyNotification> ListReplyNotifications(this NotificationPayloadObject notification)
    {
        return notification.Messages().Where(x => x.interactive != null).Where(x => x.interactive.list_reply != null).Select(
            x => new ListReplyNotification()
            {
                FromPhoneNumber = x.from,
                ItemId = x.interactive.list_reply.id,
                ItemTitle = x.interactive.list_reply.title,
                ItemDescription = x.interactive.list_reply.description,
                ContextMessageId = x.context.id,
                ContextPhoneNumber = x.context.from,
                Timestamp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(x.timestamp)).UtcDateTime.Roundtrip()
            });
    }

    public static IEnumerable<MediaNotification> MediaNotifications(this NotificationPayloadObject notification)
    {
        List<MediaNotification> mediaNotifications = new List<MediaNotification>();

        mediaNotifications.AddRange(notification.Messages().Where(x => x.audio != null).Select(x => new MediaNotification()
        {
            MediaId = x.audio.id,
            MimeType = x.audio.mime_type,
            FromPhoneNumber = x.from,
            Timestamp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(x.timestamp)).UtcDateTime.Roundtrip()
        }));
        mediaNotifications.AddRange(notification.Messages().Where(x => x.document != null).Select(x => new MediaNotification()
        {
            Caption = x.document.caption,
            FileName = x.document.filename,
            MediaId = x.document.id,
            MimeType = x.document.mime_type,
            FromPhoneNumber = x.from,
            Timestamp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(x.timestamp)).UtcDateTime.Roundtrip()
        }));

        mediaNotifications.AddRange(notification.Messages().Where(x => x.image != null).Select(x => new MediaNotification()
        {
            Caption = x.image.caption,
            MediaId = x.image.id,
            MimeType = x.image.mime_type,
            FromPhoneNumber = x.from,
            Timestamp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(x.timestamp)).UtcDateTime.Roundtrip()
        }));

        mediaNotifications.AddRange(notification.Messages().Where(x => x.video != null).Select(x => new MediaNotification()
        {
            Caption = x.video.caption,
            MediaId = x.video.id,
            MimeType = x.video.mime_type,
            FileName = x.video.filename,
            FromPhoneNumber = x.from,
            Timestamp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(x.timestamp)).UtcDateTime.Roundtrip()
        }));

        return mediaNotifications;
    }
}