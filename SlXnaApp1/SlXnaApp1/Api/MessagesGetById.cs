using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SlXnaApp1.Json;

namespace SlXnaApp1.Api
{
    public class MessagesGetById : APIRequest
    {
        public MessagesGetById(string mids, Action<IList<Message>> callback)
            : base("messages.getById")
        {
            if (string.IsNullOrEmpty(mids))
                Debug.Assert(false);

            mids = mids.TrimEnd(',');
            string[] midList = mids.Split(',');

            Debug.Assert(midList.Length < 100);

            if (midList.Length == 1)
                base.AddParameter("mid", mids);
            else
                base.AddParameter("mids", mids);

            //base.AddParameter("preview_length", "0");

            base.SetSuccessHandler(_ParseResponse);

            _callback = callback;
        }

        private void _ParseResponse(string response)
        {
            try
            {
                response = SerializeHelper.FixResponseArrayString(response);

                MessagesResponse messages = SerializeHelper.Deserialise<MessagesResponse>(response);

                if (messages.Messages != null)
                    _callback(messages.Messages);
            }
            catch (Exception)
            {
                Debug.WriteLine("Parse response from MessagesGetById failed.");

                _callback(new List<Message>());
            }
        }

        #region Private fields

        private Action<IList<Message>> _callback;

        #endregion
    }
}
