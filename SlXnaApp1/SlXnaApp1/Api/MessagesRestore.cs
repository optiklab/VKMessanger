using System;
using SlXnaApp1.Json;
using System.Diagnostics;

namespace SlXnaApp1.Api
{
    public class MessagesRestore : APIRequest
    {
        public MessagesRestore(string mid, Action<bool> callback)
            : base("messages.restore")
        {
            base.AddParameter("mid", mid);

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
                Debug.WriteLine("Parse response from MessagesRestore failed.");

                _callback(false);
            }
        }

        private Action<bool> _callback;
    }
}
