using System;
using SlXnaApp1.Json;
using System.Diagnostics;

namespace SlXnaApp1.Api
{
    public class MessagesMarkAsNew : APIRequest
    {
        public MessagesMarkAsNew(string mids, Action<bool> callback)
            : base("messages.markAsNew")
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

                _callback(succeed.Succeed == 1 ? true : false); ;
            }
            catch (Exception)
            {
                Debug.WriteLine("Parse response from MessagesMarkAsNew failed.");

                _callback(false);
            }
        }

        private Action<bool> _callback;
    }
}
