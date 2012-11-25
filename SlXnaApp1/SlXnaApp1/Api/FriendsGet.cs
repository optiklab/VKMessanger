using System;
using System.Collections.Generic;
using SlXnaApp1.Json;
using System.Diagnostics;

namespace SlXnaApp1.Api
{
    public class FriendsGet : APIRequest
    {
        public FriendsGet(Action<IList<Friend>> callback)
            : base("friends.get")
        {
            base.AddParameter("order", "hints");
            base.AddParameter("fields", "first_name,last_name,contacts,photo_rec,photo_medium_rec,has_mobile,mobile_phone,home_phone");//photo_medium_rec (100), photo_rec (50)

            base.SetSuccessHandler(_ParseResponse);

            _callback = callback;
        }

        private void _ParseResponse(string response)
        {
            try
            {
                var friends = SerializeHelper.Deserialise<FriendsResponse>(response);

                if (friends.Friends != null)
                {
                    int order = 0;
                    foreach (var friend in friends.Friends)
                    {
                        friend.HintOrder = order++;
                    }

                    _callback(friends.Friends);
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("Parse response from FriendsGet failed.");
                _callback(new List<Friend>());
            }
        }

        private Action<IList<Friend>> _callback;
    }
}
