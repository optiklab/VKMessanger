using System;
using SlXnaApp1.Json;
using System.Diagnostics;

namespace SlXnaApp1.Api
{
    public class MessagesDeleteDialog : APIRequest
    {
        public MessagesDeleteDialog(int uid, int chat_id, Action<bool> callback)
            : base("messages.deleteDialog")
        {
            // uid or chat_id should be assigned anyway.
            if (uid == -1 && chat_id > -1)
                base.AddParameter("chat_id", chat_id.ToString());
            else if (chat_id == -1 && uid > -1)
                base.AddParameter("uid", uid.ToString());
            else
                throw new Exception("");

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
                Debug.WriteLine("Parse response from MessagesDeleteDialog failed.");

                _callback(false);
            }
        }

        private Action<bool> _callback;
    }
}
