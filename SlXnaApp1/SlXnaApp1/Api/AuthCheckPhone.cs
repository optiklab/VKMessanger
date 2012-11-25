using System;
using SlXnaApp1.Json;
using System.Diagnostics;

namespace SlXnaApp1.Api
{
    public class AuthCheckPhone : APIRequest
    {
        /// <param name="phone">Required.</param>
        /// <param name="callback"></param>
        public AuthCheckPhone(string phone, Action<bool> callback)
            : base("auth.checkPhone")
        {
            base.AddParameter("phone", phone);
            base.AddParameter("client_id", AuthorizeHelper.ClientId);
            base.AddParameter("client_secret", AuthorizeHelper.ClientSecret);

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
                    SuccessEntity isOk = SerializeHelper.Deserialise<SuccessEntity>(response);

                    _callback(isOk.Succeed == 1 ? true : false);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Parse response from AuthCheckPhone failed." + ex.Message);
                _callback(false);
            }
        }

        private Action<bool> _callback;
    }
}
