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
using SlXnaApp1.Json;
using System.Diagnostics;

namespace SlXnaApp1.Api
{
    public class MessagesGetLongPollServer : APIRequest
    {
        public MessagesGetLongPollServer(Action<LongPollServerInfo> callback)
            : base("messages.getLongPollServer")
        {
            base.SetSuccessHandler(_ParseResponse);

            _callback = callback;
        }

        private void _ParseResponse(string response)
        {
            try
            {
                if (response == null)
                    _callback(null);
                else
                {
                    LongPollServerResponse serverInfo = SerializeHelper.Deserialise<LongPollServerResponse>(response);

                    _callback(serverInfo.LongPollServerInfo);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Parse response from MessagesGetLongPollServer failed." + ex.Message + "\n" + ex.StackTrace);

                _callback(null);
            }
        }

        private Action<LongPollServerInfo> _callback;
    }
}
