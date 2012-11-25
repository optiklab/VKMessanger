using System;
using SlXnaApp1.Json;
using System.Diagnostics;

namespace SlXnaApp1.Api
{
    public class AccountUnregisterDevice : APIRequest
    {
        public AccountUnregisterDevice(string token, Action<bool> callback)
            : base("account.unregisterDevice")
        {
            base.AddParameter("token", token);

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
                Debug.WriteLine("Parse response from AccountUnregisterDevice failed.");

                _callback(false);
            }
        }

        private Action<bool> _callback;
    }
}
