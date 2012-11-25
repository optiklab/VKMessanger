using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using SlXnaApp1.Infrastructure;
using SlXnaApp1.Json;
using SlXnaApp1.Services;
using System.Diagnostics;

namespace SlXnaApp1.Api
{
    public class LongPollServerRequest
    {
        public LongPollServerRequest(string server, string key, Int64 ts, Modes mode,
            Action<UpdatesResponse> successHandler, Action<int> failHandler)
        {
            if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(key) || ts <= 0)
                Debug.Assert(false);

            _server = server;
            _key = key;
            _ts = ts;
            _mode = mode;
            _successHandler = successHandler;
            _failHandler = failHandler;
        }

        #region Public method

        public void Execute()
        {
            VKWebClient client = new VKWebClient();
            string request = String.Format(@"act=a_check&key={0}&ts={1}&wait=25&mode={2}", _key, _ts, (int)_mode);
            client.SendRequest(_server, request, _ParseResponse);
        }

        #endregion

        #region Private method

        private void _ParseResponse(string response)
        {
            try
            {
                if (response == null || response.Contains("failed"))
                {
// TODO Get actual code
                    _failHandler(2);
                }
                else
                {
                    // Ok
                    UpdatesResponse updates = SerializeHelper.Deserialise<UpdatesResponse>(response);

                    _successHandler(updates);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Parse response from LongPollServerRequest failed." + ex.Message + "\n" + ex.StackTrace);

                _failHandler(1);
            }
        }

        #endregion

        #region Private fields

        private string _server;
        private string _key;
        private Int64 _ts;
        private Modes _mode;
        private Action<UpdatesResponse> _successHandler;
        private Action<int> _failHandler;

        #endregion
    }
}
