using System;
using System.Collections.Generic;
using SlXnaApp1.Json;

namespace SlXnaApp1.Infrastructure
{
    /// <summary>
    /// List of main application events.
    /// </summary>

    public class MessageDeletedEventArgs : EventArgs
    {
        public MessageDeletedEventArgs(int messageId)
        {
            MessageId = messageId;
        }

        public int MessageId { get; private set; }
    }

    public class MessageFlagsChangedEventArgs : EventArgs
    {
        public MessageFlagsChangedEventArgs(int messageId, int flags)
        {
            MessageId = messageId;
            Flags = flags;
        }

        public int MessageId { get; private set; }
        public int Flags { get; private set; }
    }

    public class MessageSetFlagsEventArgs : EventArgs
    {
        public MessageSetFlagsEventArgs(int messageId, int mask, int userId)
        {
            MessageId = messageId;
            Mask = mask;
            UserId = userId;
        }

        public int MessageId { get; private set; }
        public int Mask { get; private set; }
        public int UserId { get; private set; }
    }

    public class MessageRemoveFlagsEventArgs : EventArgs
    {
        public MessageRemoveFlagsEventArgs(int messageId, int mask, int userId)
        {
            MessageId = messageId;
            Mask = mask;
            UserId = userId;
        }

        public int MessageId { get; private set; }
        public int Mask { get; private set; }
        public int UserId { get; private set; }
    }

    public class AddMessageEventArgs : EventArgs
    {
        //public AddMessageEventArgs(string messagesIds, int lastMessageSenderId, string lastMessage)
        public AddMessageEventArgs(int messageId, int flags, int fromId, int timestamp,
            string subject, string text, AttachmentsUpdate attachment)
        {
            MessageId = messageId;
            Flags = flags;
            FromId = fromId;
            Timestamp = timestamp;
            Subject = subject;
            Text = text;
            Attachment = attachment;
        }

        public int MessageId { get; private set; }
        public int Flags { get; private set; }
        public int FromId { get; private set; }
        public int Timestamp { get; private set; }
        public string Subject { get; private set; }
        public string Text { get; private set; }
        public AttachmentsUpdate Attachment { get; private set; }
    }

    public class AddMessagesEventArgs : EventArgs
    {
        public AddMessagesEventArgs(IList<Message> messages)
        {
            if (messages == null)
                Messages = new List<Message>();
            else
                Messages = messages;
        }

        public IList<Message> Messages { get; private set; }
    }


    public class UserOnlineEventArgs : EventArgs
    {
        public UserOnlineEventArgs(int userId)
        {
            UserId = Math.Abs(userId);
        }

        public int UserId { get; private set; }
    }

    public class UserOfflineEventArgs : EventArgs
    {
        public UserOfflineEventArgs(int userId, int flags)
        {
            UserId = Math.Abs(userId);
            Flags = flags;
        }

        public int UserId { get; private set; }
        public int Flags { get; private set; }
    }

    public class ChatChangedEventArgs : EventArgs
    {
        public ChatChangedEventArgs(int chatId, int self)
        {
            ChatId = chatId;
            Self = self;
        }

        public int ChatId { get; private set; }
        public int Self { get; private set; }
    }

    public class StartTypingInChatEventArgs : EventArgs
    {
        public StartTypingInChatEventArgs(int userId, int flags)
        {
            UserId = userId;
            Flags = flags;
        }

        public int UserId { get; private set; }
        public int Flags { get; private set; }
    }

    public class StartTypingInGroupChatEventArgs : EventArgs
    {
        public StartTypingInGroupChatEventArgs(int userId, int chatId)
        {
            UserId = userId;
            ChatId = chatId;
        }

        public int UserId { get; private set; }
        public int ChatId { get; private set; }
    }

    public class UserCalledEventArgs : EventArgs
    {
        public UserCalledEventArgs(int userId, int callId)
        {
            UserId = userId;
            CallId = callId;
        }

        public int UserId { get; private set; }
        public int CallId { get; private set; }
    }

    public delegate void MessageDeletedEventHandler(Object sender, MessageDeletedEventArgs e);
    public delegate void MessageFlagsChangedEventHandler(Object sender, MessageFlagsChangedEventArgs e);
    public delegate void MessageSetFlagsEventHandler(Object sender, MessageSetFlagsEventArgs e);
    public delegate void MessageRemoveFlagsEventHandler(Object sender, MessageRemoveFlagsEventArgs e);
    public delegate void AddMessageEventHandler(Object sender, AddMessageEventArgs e);
    public delegate void AddMessagesEventHandler(Object sender, AddMessagesEventArgs e);
    public delegate void UserOnlineEventHandler(Object sender, UserOnlineEventArgs e);
    public delegate void UserOfflineEventHandler(Object sender, UserOfflineEventArgs e);
    public delegate void ChatChangedEventHandler(Object sender, ChatChangedEventArgs e);
    public delegate void StartTypingInChatEventHandler(Object sender, StartTypingInChatEventArgs e);
    public delegate void StartTypingInGroupChatEventHandler(Object sender, StartTypingInGroupChatEventArgs e);
    public delegate void UserCalledEventHandler(Object sender, UserCalledEventArgs e);
}
