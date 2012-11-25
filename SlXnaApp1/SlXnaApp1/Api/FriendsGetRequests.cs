using System;
using System.Collections.Generic;
using SlXnaApp1.Json;
using System.Diagnostics;

namespace SlXnaApp1.Api
{
    public class FriendsGetRequests : APIRequest
    {
        /// <param name="need_mutual">Set to 1 if need to return 20 common (mutual) friends.
        /// Otherwise value will not be considered.</param>
        /// <param name="callback"></param>
        public FriendsGetRequests(int need_mutual,
            Action<IList<FriendRequestEntity>> callbackWithMessages)
            : base("friends.getRequests")
        {
            base.AddParameter("need_messages", "1");

            if (need_mutual == 1)
                base.AddParameter("need_mutual", need_mutual.ToString());

            base.SetSuccessHandler(_ParseResponse);

            _callbackWithMessages = callbackWithMessages;
        }

        private void _ParseResponse(string response)
        {
            try
            {
                var friendRequests = SerializeHelper.Deserialise<FriendRequestsWithMessagesResponse>(response);

                if (friendRequests.FriendRequests != null)
                    _callbackWithMessages(friendRequests.FriendRequests);
            }
            catch (Exception)
            {
                Debug.WriteLine("Parse response from FriendsGetRequests _callbackWithMessages failed.");
                _callbackWithMessages(new List<FriendRequestEntity>());
            }
        }

        private Action<IList<FriendRequestEntity>> _callbackWithMessages;
    }
}
