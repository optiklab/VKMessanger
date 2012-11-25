using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using SlXnaApp1.Json;
using System.Diagnostics;

namespace SlXnaApp1.Api
{
    public class UsersSearch : APIRequest
    {
        public UsersSearch(string query, Action<IList<UserInfo>> callback)
            : base("users.search")
        {
            base.AddParameter("q", query);
            base.AddParameter("count", "50");
            base.AddParameter("fields", "uid,first_name,last_name,nickname,city,education,online,photo,photo_medium,photo_big,has_mobile,mobile_phone,home_phone");
            //uid, first_name, last_name, nickname, screen_name, sex, bdate (birthdate), city,
            //country, timezone, photo, photo_medium, photo_big, has_mobile, rate, contacts, education, online, counters

            //base.AddParameter("name_case", name_case); //nom, gen, dat, acc, ins, abl. Default is nom.

            base.SetSuccessHandler(_ParseResponse);

            _callback = callback;
        }

        private void _ParseResponse(string response)
        {
            try
            {
                response = SerializeHelper.FixResponseArrayString(response);

                var usersInfo = SerializeHelper.Deserialise<UsersInfoResponse>(response);

                if (usersInfo.UserInfos != null)
                    _callback(usersInfo.UserInfos);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Parse response from UsersSearch failed. " + ex.Message);

                _callback(new List<UserInfo>());
            }
        }

        private Action<IList<UserInfo>> _callback;
    }
}
