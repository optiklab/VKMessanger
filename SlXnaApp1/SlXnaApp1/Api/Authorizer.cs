using System;
using System.Collections.Generic;
using System.Net;
using SlXnaApp1.Infrastructure;
using SlXnaApp1.Json;
using TestViKeyUtility;

namespace SlXnaApp1.Api
{
    internal static class AuthorizeHelper
    {
        public static string ClientSecret
        {
            get
            {
                return CLIENT_SECRET;
            }
        }

        public static string ClientId
        {
            get
            {
                return CLIENT_ID;
            }
        }

        private const string CLIENT_ID = "2993778";
        private const string CLIENT_SECRET = "opaque";
    }

    internal class Authorizer
    {
        #region Public methods

        public void Authorize(string login, string password,
            string captchaSid, string captchaKey, Action<ErrorCode, Dictionary<string, string>> handler)
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
                handler(ErrorCode.AUTH_SUCCESS, new Dictionary<string, string>());

            _handler = handler;

            _Authorize(login, password, captchaSid, captchaKey);
        }

        #endregion

        #region Private methods

        private void _Authorize(string login, string password, string captchaSid, string captchaKey)
        {
            login = HttpUtility.UrlEncode(login);
            password = HttpUtility.UrlEncode(password);
            string request = string.Format(
              @"grant_type=password&scope=nohttps,notify,friends,photos,audio,video,docs,status,messages,notification&client_id={0}&client_secret={1}&username={2}&password={3}",
              AuthorizeHelper.ClientId, AuthorizeHelper.ClientSecret, login, password);

            // Check if need to show captcha...
            if (captchaSid != null && captchaKey != null)
            {
                request = request + "&captcha_sid=" +
                    HttpUtility.UrlEncode(captchaSid) + "&captcha_key=" +
                    HttpUtility.UrlEncode(captchaKey);
            }

            VKWebClient client = new VKWebClient();
            client.SendRequest(@"https://oauth.vk.com/token", request, _ParseResponse);
        }

        private void _ParseResponse(string response)
        {
            var outputValues = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(response))
                _handler(ErrorCode.INCORRECT_RESPONSE, outputValues);
            else if (response.Contains("error"))
            {
                AuthError result = SerializeHelper.Deserialise<AuthError>(response);

                if (result.error == "need_captcha")
                {
                    outputValues.Add("captcha_sid", result.captcha_sid);
                    outputValues.Add("captcha_img", result.captcha_img);

                    _handler(ErrorCode.NEED_CAPTCHA, outputValues);
                }
                else if (result.error == "invalid_client")
                    _handler(ErrorCode.REAUTH_INVALID_CLIENT, outputValues);
                else
                    _handler(ErrorCode.UNKNOWN_ERROR, outputValues);
            }
            else
            {
                AuthSucceeded result = SerializeHelper.Deserialise<AuthSucceeded>(response);

                outputValues.Add("user_id", result.user_id);
                outputValues.Add("access_token", result.access_token);
                outputValues.Add("secret", result.secret);

                _handler(ErrorCode.AUTH_SUCCESS, outputValues);
            }
        }

        #endregion

        #region Private fields

        private Action<ErrorCode, Dictionary<string, string>> _handler;

        #endregion
    }
}
