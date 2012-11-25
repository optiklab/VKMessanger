using System;
using SlXnaApp1.Json;
using System.Diagnostics;

namespace SlXnaApp1.Api
{
    /// <summary>
    /// 
    /// </summary>
    public class AuthConfirm : APIRequest
    {
        /// <summary>
        /// Finishes registering started with method "AuthSignUp".
        /// </summary>
        /// <param name="validatedPhone">Required.</param>
        /// <param name="password">Required.</param>
        /// <param name="codeFromSMS">Required.</param>
        /// <param name="callback"></param>
        public AuthConfirm(string validatedPhone, string password, string codeFromSMS, Action<int, bool> callback)
            : base("auth.confirm")
        {
            if (string.IsNullOrEmpty(validatedPhone) || string.IsNullOrEmpty(codeFromSMS))
            {
                throw new ArgumentNullException("One of required parameters not set");
            }

            base.AddParameter("phone", validatedPhone);
            base.AddParameter("code", codeFromSMS);
            //base.AddParameter("testMode", "1");

            if (!string.IsNullOrEmpty(password))
            {
                if (password.Length < 6)
                    throw new ArgumentException("password length should be not less than 6");

                base.AddParameter("password", password);
            }

            base.AddParameter("client_id", AuthorizeHelper.ClientId);
            base.AddParameter("client_secret", AuthorizeHelper.ClientSecret);

            base.SetSuccessHandler(_ParseResponse);

            _callback = callback;
        }

        private void _ParseResponse(string response)
        {
            try
            {
                AuthConfirmResponse authConfirm = SerializeHelper.Deserialise<AuthConfirmResponse>(response);

                _callback(authConfirm.AuthConfirmEntity.Uid, authConfirm.AuthConfirmEntity.Success == 1 ? true : false);
            }
            catch (Exception)
            {
                Debug.WriteLine("Parse response from AuthConfirm failed.");
                _callback(-1, false);
            }
        }

        private Action<int, bool> _callback;
    }
}
