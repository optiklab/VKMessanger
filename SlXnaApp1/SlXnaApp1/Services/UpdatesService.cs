using System;
using System.Collections.Generic;
using System.Diagnostics;
using SlXnaApp1.Infrastructure;
using SlXnaApp1.Json;

namespace SlXnaApp1.Services
{
    /// <summary>
    /// Class is responsible for notifying signed entities about some VK EVENTS like NEW MESSAGE, CHANGED FLAGS, and so on.
    /// </summary>
    public class UpdatesService
    {
        #region Public events

        /// <summary>
        /// Events for notifying.
        /// </summary>
        public event MessageDeletedEventHandler MessageDeleted;
        public event MessageFlagsChangedEventHandler MessageFlagsChanged;
        public event MessageSetFlagsEventHandler MessageSetFlags;
        public event MessageRemoveFlagsEventHandler MessageRemovedFlags;
        public event AddMessageEventHandler MessageAdded;
        public event AddMessagesEventHandler MessagesAdded;
        public event UserOnlineEventHandler UserBecomeOnline;
        public event UserOfflineEventHandler UserBecomeOffline;
        public event ChatChangedEventHandler ChatChanged;
        public event StartTypingInGroupChatEventHandler TypingInGroupChatStarted;
        public event StartTypingInChatEventHandler TypingInChatStarted;
        public event UserCalledEventHandler UserCalled;

        #endregion

        #region Public methods

        public void PutUpdates(IList<IList<object>> updates)
        {
            string midsToRequest = string.Empty;
            IList<object> lastMessageUpdate = null;

            foreach (var update in updates)
            {
                if (update == null)
                    continue;

                int type = -1;
                if (update.Count > 0)
                    type = (int)update[0];

                try
                {
                    if (type == 0 && MessageDeleted != null)
                        MessageDeleted(this, new MessageDeletedEventArgs((int)update[1]));
                    else if (type == 1 && MessageFlagsChanged != null)
                        MessageFlagsChanged(this, new MessageFlagsChangedEventArgs((int)update[1], (int)update[2]));
                    else if (type == 2 && MessageSetFlags != null)
                    {
                        if (update.Count == 3)
                            MessageSetFlags(this, new MessageSetFlagsEventArgs((int)update[1], (int)update[2], (int)update[3]));
                        else
                            MessageSetFlags(this, new MessageSetFlagsEventArgs((int)update[1], (int)update[2], -1));
                    }
                    else if (type == 3 && MessageRemovedFlags != null)
                    {
                        if (update.Count == 3)
                            MessageRemovedFlags(this, new MessageRemoveFlagsEventArgs((int)update[1], (int)update[2], (int)update[3]));
                        else
                            MessageRemovedFlags(this, new MessageRemoveFlagsEventArgs((int)update[1], (int)update[2], -1));
                    }
                    else if (type == 4 && MessageAdded != null)
                    {
                        MessageAdded(this, new AddMessageEventArgs((int)update[1], (int)update[2], -1, -1, null, null, null));
                        //MessageAdded(this, new AddMessageEventArgs((int)update[1], (int)update[2], (int)update[3],
                        //    (int)update[4], (string)update[5], (string)update[6], null));
                        midsToRequest += (int)update[1] + ",";
                        lastMessageUpdate = update;
                    }
                    else if (type == 8 && UserBecomeOnline != null)
                        UserBecomeOnline(this, new UserOnlineEventArgs((int)update[1]));
                    else if (type == 9 && UserBecomeOffline != null)
                        UserBecomeOffline(this, new UserOfflineEventArgs((int)update[1], (int)update[2]));
                    else if (type == 51 && ChatChanged != null)
                        ChatChanged(this, new ChatChangedEventArgs((int)update[1], (int)update[2]));
                    else if (type == 61 && TypingInChatStarted != null)
                        TypingInChatStarted(this, new StartTypingInChatEventArgs((int)update[1], 1));
                    else if (type == 62 && TypingInGroupChatStarted != null)
                        TypingInGroupChatStarted(this, new StartTypingInGroupChatEventArgs((int)update[1], (int)update[2]));
                    else if (type == 70 && UserCalled != null)
                        UserCalled(this, new UserCalledEventArgs((int)update[1], (int)update[2]));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("PutUpdates event raising failed: " + ex.Message + Environment.NewLine + ex.StackTrace);
                }
            }

            // Set notification only about last added message.
            if (lastMessageUpdate != null)
            {
                // Since we have messages to update, we need to request details.
                App.Current.EntityService.UpdateMessagesById(midsToRequest);
            }
        }

        public void PutHistory(LongPollHistory history)
        {
            if (history.History != null && history.History.Count > 0)
            {
                foreach (var update in history.History)
                {
                    int type = -1;
                    if (update.Count > 0)
                        type = (int)update[0];

                    if (type == 0 && MessageDeleted != null)
                        MessageDeleted(this, new MessageDeletedEventArgs((int)update[1]));
                    else if (type == 1 && MessageFlagsChanged != null)
                        MessageFlagsChanged(this, new MessageFlagsChangedEventArgs((int)update[1], (int)update[2]));
                    else if (type == 2 && MessageSetFlags != null)
                    {
                        if (update.Count == 3)
                            MessageSetFlags(this, new MessageSetFlagsEventArgs((int)update[1], (int)update[2], (int)update[3]));
                        else
                            MessageSetFlags(this, new MessageSetFlagsEventArgs((int)update[1], (int)update[2], -1));
                    }
                    else if (type == 3 && MessageRemovedFlags != null)
                    {
                        if (update.Count == 3)
                            MessageRemovedFlags(this, new MessageRemoveFlagsEventArgs((int)update[1], (int)update[2], (int)update[3]));
                        else
                            MessageRemovedFlags(this, new MessageRemoveFlagsEventArgs((int)update[1], (int)update[2], -1));
                    }
                    else if (type == 51 && ChatChanged != null)
                        ChatChanged(this, new ChatChangedEventArgs((int)update[1], (int)update[2]));
                    else if (type == 70 && UserCalled != null)
                        UserCalled(this, new UserCalledEventArgs((int)update[1], (int)update[2]));
// NOTE. Specially not handled type 4. See description for Messages section.
                }
            }

            if (history.Messages != null && history.Messages.Count > 0) //IList<Message>
            {
                MessagesAdded(this, new AddMessagesEventArgs(history.Messages));

                // Get latest dialogs list.
                App.Current.EntityService.UpdateDialogs();
            }
        }

        #endregion

        #region Private methods


        #endregion

        #region Public properties

        public int UnreadMessages { get; set; }

        public int Requests { get; set; }

        #endregion
    }
}
