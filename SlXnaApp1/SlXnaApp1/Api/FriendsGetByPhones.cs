using System;
using System.Collections.Generic;
using System.Diagnostics;
using SlXnaApp1.Json;

namespace SlXnaApp1.Api
{
    public class FriendsGetByPhones : APIRequest
    {
        /// <param name="phones">Required.</param>
        /// <param name="callback"></param>
        public FriendsGetByPhones(string phones, Action<IList<UserInfo>> callback)
            : base("friends.getByPhones")
        {
            if (string.IsNullOrEmpty(phones))
                Debug.Assert(false);

            //string[] phonesList = phones.Split(',');
            //if (phonesList.Length > 1000)
            //    Debug.Assert(false);

            base.AddParameter("phones", phones);
            base.AddParameter("fields", "first_name,last_name,contacts,photo");
            //uid, first_name, last_name, nickname, screen_name, sex, bdate (birthdate), city,
            //country, timezone, photo, photo_medium, photo_big, has_mobile, rate, contacts, education, online, counters
            //see... UserInfo class

            base.SetSuccessHandler(_ParseResponse);

            _callback = callback;
        }

        private void _ParseResponse(string response)
        {
            try
            {
                var usersInfo = SerializeHelper.Deserialise<UsersInfoResponse>(response);

                if (usersInfo.UserInfos != null)
                    _callback(usersInfo.UserInfos);
            }
            catch (Exception)
            {
                _callback(new List<UserInfo>());
            }
        }

        private Action<IList<UserInfo>> _callback;
    }
}