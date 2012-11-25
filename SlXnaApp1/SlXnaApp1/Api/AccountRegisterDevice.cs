using System;
using SlXnaApp1.Json;
using System.Diagnostics;
using SlXnaApp1.Infrastructure;

namespace SlXnaApp1.Api
{
    public class AccountRegisterDevice : APIRequest
    {
        public AccountRegisterDevice(string token, Action<bool> callback)
            : base("account.registerDevice")
        {
            base.AddParameter("token", token);
            base.AddParameter("device_model", WindowsPhoneHelpers.GetDeviceModel());

            string systemVersion = "Windows Phone " + WindowsPhoneHelpers.GetOSVersion();
            base.AddParameter("system_version", systemVersion);
            //base.AddParameter("no_text", "1");

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
                Debug.WriteLine("Parse response from AccountRegisterDevice failed.");

                _callback(false);
            }
        }

        private Action<bool> _callback;
    }
}
