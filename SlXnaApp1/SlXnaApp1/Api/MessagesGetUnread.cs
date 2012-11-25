using System;
using System.Collections.Generic;
using System.Diagnostics;
using SlXnaApp1.Json;
using System.Linq;

namespace SlXnaApp1.Api
{
    public class MessagesGetUnread : APIRequest
    {
        public MessagesGetUnread(Action<List<int>> callback)
            : base("messages.get")
        {
            base.AddParameter("count", "100");
            base.AddParameter("filters", "1"); // unread
            base.AddParameter("preview_length", "1"); // to minimize response

            base.SetSuccessHandler(_ParseResponse);

            _callback = callback;
        }

        private void _ParseResponse(string response)
        {
            try
            {
                if (response == null)
                    _callback(new List<int>());
                else
                {
                    response = SerializeHelper.FixResponseArrayString(response);

                    var messages = SerializeHelper.Deserialise<MessagesResponse>(response);

                    if (messages.Messages != null)
                    {
                        var mids = messages.Messages.Select(x => x.Mid);
                        _callback(mids.ToList());
                    }
                    else
                        _callback(new List<int>());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Parse response: " + response + " from MessagesGetUnread failed:" + ex.Message);

                _callback(new List<int>());
            }
        }

        private Action<List<int>> _callback;
    }
}
