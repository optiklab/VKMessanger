using System;
using SlXnaApp1.Json;
using System.Diagnostics;

namespace SlXnaApp1.Api
{
    public class AccountSetSilenceMode : APIRequest
    {
        public AccountSetSilenceMode(string token, int seconds, Action<bool> callback)
            : base("account.setSilenceMode")
        {
            base.AddParameter("token", token);
            base.AddParameter("time", seconds.ToString());

            base.SetSuccessHandler(_ParseResponse);

            _callback = callback;
        }

        private void _ParseResponse(string response)
        {
            try
            {
                if (response == null)
                    _callback(false);
                else
                {
                    SuccessEntity succeed = SerializeHelper.Deserialise<SuccessEntity>(response);

                    _callback(succeed.Succeed == 1 ? true : false);
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("Parse response from AccountSetSilenceMode failed.");

                _callback(false);
            }
        }

        private Action<bool> _callback;
    }
}
