using System;
using SlXnaApp1.Json;
using System.Diagnostics;

namespace SlXnaApp1.Api
{
    public class MessagesGetLastActivity : APIRequest
    {
        public MessagesGetLastActivity(int uid, Action<ActivityStatus> callback)
            : base("messages.getLastActivity")
        {
            if (uid <= 0)
                Debug.Assert(false);

            base.AddParameter("uid", uid.ToString());

            base.SetSuccessHandler(_ParseResponse);

            _callback = callback;
        }

        private void _ParseResponse(string response)
        {
            try
            {
                ActivityStatusResponse activityStatus = SerializeHelper.Deserialise<ActivityStatusResponse>(response);

                _callback(activityStatus.ActivityStatus);
            }
            catch (Exception)
            {
                Debug.WriteLine("Parse response from MessagesGetLastActivity failed.");

                _callback(null);
            }
        }

        private Action<ActivityStatus> _callback;
    }
}
