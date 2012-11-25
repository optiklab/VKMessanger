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
    public class MessagesGetLongPollHistory : APIRequest
    {
        /// <param name="ts">Required.</param>
        /// <param name="max_msg_id">Not required.</param>
        public MessagesGetLongPollHistory(Int64 ts, int max_msg_id, Action<LongPollHistoryResponse> callback)
            : base("messages.getLongPollHistory")
        {
            if (ts <= 0)
                Debug.Assert(false);

            base.AddParameter("ts", ts.ToString());
            base.AddParameter("max_msg_id", max_msg_id.ToString());

            base.SetSuccessHandler(_ParseResponse);

            _callback = callback;
        }

        private void _ParseResponse(string response)
        {
            try
            {
                response = _FixResponseArrayString(response);

                LongPollHistoryResponse history = SerializeHelper.Deserialise<LongPollHistoryResponse>(response);

                _callback(history);
            }
            catch
            {
                Debug.WriteLine("Parse response from MessagesGetLongPollHistory failed.");

                _callback(null);
            }
        }

        private string _FixResponseArrayString(string response)
        {
            Debug.Assert(!string.IsNullOrEmpty(response));

            int messagesPlace = response.IndexOf(MESSAGES_TOKEN);

            if (messagesPlace == -1)
                return response; // Don't need to fix.

            int commaIndex = response.IndexOf(",", messagesPlace + MESSAGES_TOKEN_LENGTH);
            int start = messagesPlace + MESSAGES_TOKEN_LENGTH;

            if (commaIndex == -1)
            {
                int bracketIndex = response.IndexOf("]", messagesPlace + MESSAGES_TOKEN_LENGTH);
                response = response.Remove(start, bracketIndex - start);
            }
            else
            {
                response = response.Remove(start, commaIndex - start);
            }

            return response;
        }

        #region Private constants

        private const int MESSAGES_TOKEN_LENGTH = 12;
        private const string MESSAGES_TOKEN = "\"messages\":[";

        #endregion

        #region Private fields

        private Action<LongPollHistoryResponse> _callback;

        #endregion
    }
}
