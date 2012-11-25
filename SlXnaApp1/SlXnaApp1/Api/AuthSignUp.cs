using System;
using SlXnaApp1.Json;
using System.Diagnostics;

namespace SlXnaApp1.Api
{
    public class AuthSignUp : APIRequest
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="validatedPhone">Required.</param>
        /// <param name="firstName">Required.</param>
        /// <param name="lastName">Required.</param>
        /// <param name="sex">Not required.
        /// 1 - women
        /// 2 - man</param>
        /// <param name="password">Not required. Not less than 6 symbols length.</param>
        /// <param name="voice">Not required.
        /// 1 if need to call to phone and say the code by voice, instead of SMS. 
        /// 0 (default) for sending SMS.</param>
        /// <param name="sid">Not required. Session id is useful for second call of the method, if SMS not delivered.</param>
        /// <param name="testMode">Not required.
        /// 1 - test mode (new users will not be registered, but phone will not be checked for second call).
        /// 0 - work mode as default.</param>
        /// <param name="callback"></param>
        public AuthSignUp(string validatedPhone, string firstName, string lastName,
            string password, string sid, Action<SidDataResponse> callback)
            : base("auth.signup")
        {
            if (string.IsNullOrEmpty(validatedPhone) ||
                string.IsNullOrEmpty(firstName) ||
                string.IsNullOrEmpty(lastName))
            {
                throw new ArgumentNullException("One of required parameters not set");
            }

            base.AddParameter("phone", validatedPhone);
            base.AddParameter("first_name", firstName);
            base.AddParameter("last_name", lastName);
            base.AddParameter("client_id", AuthorizeHelper.ClientId);
            base.AddParameter("client_secret", AuthorizeHelper.ClientSecret);
            //base.AddParameter("testMode", "1");

            if (!string.IsNullOrEmpty(sid))
                base.AddParameter("sid", sid);

            if (!string.IsNullOrEmpty(password))
            {
                if (password.Length < 6)
                    throw new ArgumentException("password length should be not less than 6");

                base.AddParameter("password", password);
            }

            base.SetSuccessHandler(_ParseResponse);

            _callback = callback;
        }

        private void _ParseResponse(string response)
        {
            try
            {
                SidDataResponse sid = SerializeHelper.Deserialise<SidDataResponse>(response);

                _callback(sid);
            }
            catch (Exception)
            {
                Debug.WriteLine("Parse response from Auth Sign Up failed.");

                _callback(null);
            }
        }

        private Action<SidDataResponse> _callback;
    }
}
