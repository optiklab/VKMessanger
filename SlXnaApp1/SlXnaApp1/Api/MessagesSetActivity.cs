using System;
using SlXnaApp1.Json;
using System.Diagnostics;

namespace SlXnaApp1.Api
{
    public class MessagesSetActivity : APIRequest
    {
        public MessagesSetActivity(int uid, int chat_id, Action<bool> callback)
            : base("messages.setActivity")
        {
            // uid or chat_id should be assigned anyway.
            if (uid == -1 && chat_id > -1)
                base.AddParameter("chat_id", chat_id.ToString());
            else if (chat_id == -1 && uid > -1)
                base.AddParameter("uid", uid.ToString());

            base.AddParameter("type", "typing");

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
            catch
            {
                Debug.WriteLine("Parse response from MessagesSetActivity failed.");

                _callback(false);
            }
        }

        private Action<bool> _callback;
    }
}

