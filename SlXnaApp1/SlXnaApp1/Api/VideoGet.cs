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
using System.Diagnostics;
using SlXnaApp1.Json;
using System.Collections.Generic;

namespace SlXnaApp1.Api
{
    public class VideoGet : APIRequest
    {
        public VideoGet(string vid, int uid, Action<string> callback)
            : base("friends.get")
        {
            base.AddParameter("videos", vid);
            base.AddParameter("uid", uid.ToString());
            base.AddParameter("width", "320");
            base.AddParameter("count", "1");
            base.AddParameter("offset", "0");

            base.SetSuccessHandler(_ParseResponse);

            _callback = callback;
        }

        private void _ParseResponse(string response)
        {
            try
            {
                var str = SerializeHelper.FixResponseArrayString(response);
                var att = SerializeHelper.Deserialise<List<VideoAttachment>>(str);

                if (att != null && att.Count > 0)
                {
                    _callback(att[0].Player);
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("Parse response from FriendsGet failed.");
                _callback(string.Empty);
            }
        }

        private Action<string> _callback;
    }
}
