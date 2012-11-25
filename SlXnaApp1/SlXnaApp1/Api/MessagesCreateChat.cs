using System;
using SlXnaApp1.Json;
using System.Diagnostics;

namespace SlXnaApp1.Api
{
    public class MessagesCreateChat : APIRequest
    {
        public MessagesCreateChat(string uids, string title, Action<int> callback)
            : base("messages.createChat")
        {
            if (string.IsNullOrEmpty(uids) || string.IsNullOrEmpty(title))
            {
                Debug.Assert(false);
            }

            base.AddParameter("uids", uids);
            base.AddParameter("title", title);

            base.SetSuccessHandler(_ParseResponse);

            _callback = callback;
        }

        private void _ParseResponse(string response)
        {
            try
            {
                SuccessEntity success = SerializeHelper.Deserialise<SuccessEntity>(response);

                _callback(success.Succeed);
            }
            catch (Exception)
            {
                Debug.WriteLine("Parse response from MessagesCreateChat failed.");

                _callback(-1);
            }
        }

        private Action<int> _callback;
    }
}
