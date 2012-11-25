using System;
using System.Collections.Generic;
using SlXnaApp1.Json;
using System.Diagnostics;

namespace SlXnaApp1.Api
{
    public class MessagesGetChatUsers : APIRequest
    {
        public MessagesGetChatUsers(int chat_id, Action<IList<Friend>> callback)
            : base("messages.getChatUsers")
        {
            if (chat_id <= 0)
                Debug.Assert(false);

            base.AddParameter("chat_id", chat_id.ToString());
            base.AddParameter("fields", "first_name,last_name,contacts,photo_rec");

            base.SetSuccessHandler(_ParseResponse);

            _callback = callback;
        }

        private void _ParseResponse(string response)
        {
            try
            {
                FriendsResponse friends = SerializeHelper.Deserialise<FriendsResponse>(response);

                if (friends.Friends != null)
                    _callback(friends.Friends);
            }
            catch (Exception)
            {
                Debug.WriteLine("Parse response from MessagesGetChatUsers failed.");

                _callback(new List<Friend>());
            }
        }

        private Action<IList<Friend>> _callback;
    }
}
