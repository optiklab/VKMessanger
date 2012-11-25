using System;
using SlXnaApp1.Json;
using System.Diagnostics;

namespace SlXnaApp1.Api
{
    public class MessagesMarkAsRead : APIRequest
    {
        public MessagesMarkAsRead(string mids, Action<bool> callback)
            : base("messages.markAsRead")
        {
            if (string.IsNullOrEmpty(mids))
            {
                Debug.Assert(false);
            }

            base.AddParameter("mids", mids);

            base.SetSuccessHandler(_ParseResponse);

            _callback = callback;
        }

        private void _ParseResponse(string response)
        {
            try
            {
                SuccessEntity succeed = SerializeHelper.Deserialise<SuccessEntity>(response);

                _callback(succeed.Succeed == 1 ? true : false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Parse response from MessagesMarkAsRead failed:" + ex.Message);

                _callback(false);
            }
        }

        private Action<bool> _callback;
    }
}
