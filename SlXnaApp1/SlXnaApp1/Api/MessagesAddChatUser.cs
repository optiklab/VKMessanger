using System;
using SlXnaApp1.Json;
using System.Diagnostics;

namespace SlXnaApp1.Api
{
    public class MessagesAddChatUser : APIRequest
    {
        public MessagesAddChatUser(int chat_id, int uid, Action<bool> callback)
            : base("messages.addChatUser")
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
                SuccessEntity success = SerializeHelper.Deserialise<SuccessEntity>(response);

                _callback(success.Succeed == 1 ? true : false);
            }
            catch (Exception)
            {
                Debug.WriteLine("Parse response from MessagesAddChatUser failed.");

                _callback(false);
            }
        }

        private Action<bool> _callback;
    }
}
