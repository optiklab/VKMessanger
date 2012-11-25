using System;
using System.Collections.Generic;
using SlXnaApp1.Json;
using System.Diagnostics;

namespace SlXnaApp1.Api
{
    public class MessagesGet : APIRequest
    {
        public MessagesGet(int uid, int chat_id, int offset, int count, Action<bool, IList<Message>> callback)
            : base(METHOD_NAME)
        {
            // uid or chat_id should be assigned anyway.
            if (uid == -1 && chat_id > -1)
                base.AddParameter(CHAT_ID, chat_id.ToString());
            else if (chat_id == -1 && uid > -1)
                base.AddParameter(UID, uid.ToString());
            else
                Debug.Assert(false);

            // Can't get more than 100 per request.
            if (count < 0 || count > 100)
                Debug.Assert(false);

            if (offset < 0)
                Debug.Assert(false);

            base.AddParameter(OFFSET, offset.ToString());
            base.AddParameter(COUNT, count.ToString());
            base.AddParameter(FIELDS, LIST_OF_FIELDS);

            base.SetSuccessHandler(_ParseResponse);

            _callback = callback;
        }

        private void _ParseResponse(string response)
        {
            try
            {
                if (response == null)
                    _callback(true, new List<Message>());
                else
                {
                    response = SerializeHelper.FixResponseArrayString(response);

                    MessagesResponse messages = SerializeHelper.Deserialise<MessagesResponse>(response);

                    if (messages.Messages != null)
                        _callback(true, messages.Messages);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Parse response from MessagesGet failed: " + ex.Message);

                _callback(true, new List<Message>());
            }
        }

        #region Private constants

        private const string METHOD_NAME = "messages.getHistory";
        private const string CHAT_ID = "chat_id";
        private const string UID = "uid";
        private const string COUNT = "count";
        private const string OFFSET = "offset";
        private const string PHOTO_SIZES = "photo_sizes";
        private const string FIELDS = "fields";
        private const string LIST_OF_FIELDS = "first_name,last_name";//,photo_rec,photo_medium_rec";

        #endregion

        #region Private fields

        private Action<bool, IList<Message>> _callback;

        #endregion
    }
}
