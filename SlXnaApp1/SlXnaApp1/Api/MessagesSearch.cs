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
using System.Collections.Generic;
using SlXnaApp1.Json;
using System.Diagnostics;

namespace SlXnaApp1.Api
{
    public class MessagesSearch : APIRequest
    {
        public MessagesSearch(string query, Action<IList<Message>> callback)
            : base("messages.search")
        {
            base.AddParameter("q", query);
            base.AddParameter("fields", "first_name,last_name,photo_rec");

            base.SetSuccessHandler(_ParseResponse);

            _callback = callback;
        }

        private void _ParseResponse(string response)
        {
            try
            {
                if (response == null)
                    _callback(new List<Message>());
                else
                {
                    response = SerializeHelper.FixResponseArrayString(response);

                    MessagesResponse messages = SerializeHelper.Deserialise<MessagesResponse>(response);

                    if (messages.Messages != null)
                        _callback(messages.Messages);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Parse response from MessagesSearch failed: " + ex.Message);

                _callback(new List<Message>());
            }
        }

        private Action<IList<Message>> _callback;
    }
}
