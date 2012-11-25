using System;
using SlXnaApp1.Json;
using System.Diagnostics;

namespace SlXnaApp1.Api
{
    public class MessagesRemoveChatUser : APIRequest
    {
        public MessagesRemoveChatUser(int uid, int chat_id, Action<bool> callback)
            : base("messages.removeChatUser")
        {
            if (chat_id <= 0 || uid <= 0)
            {
                Debug.Assert(false);
            }

            base.AddParameter("chat_id", chat_id.ToString());
            base.AddParameter("uid", uid.ToString());

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
            catch (Exception)
            {
                Debug.WriteLine("Parse response from MessagesRemoveChatUser failed.");

                _callback(false);
            }
        }

        private Action<bool> _callback;
    }
}
