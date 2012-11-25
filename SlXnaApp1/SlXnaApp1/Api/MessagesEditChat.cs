using System;
using SlXnaApp1.Json;
using System.Diagnostics;

namespace SlXnaApp1.Api
{
    public class MessagesEditChat : APIRequest
    {
        public MessagesEditChat(int chatid, string title, Action<bool> callback)
            : base("messages.editChat")
        {
            if (chatid <= 0 || string.IsNullOrEmpty(title))
            {
                Debug.Assert(false);
            }

            base.AddParameter("chat_id", chatid.ToString());
            base.AddParameter("title", title);

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
                Debug.WriteLine("Parse response from MessagesEditChat failed.");

                _callback(false);
            }
        }

        private Action<bool> _callback;
    }
}
