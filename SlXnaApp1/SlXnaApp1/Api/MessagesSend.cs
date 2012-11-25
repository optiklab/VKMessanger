using System;
using SlXnaApp1.Json;
using System.Diagnostics;
using System.Net;

namespace SlXnaApp1.Api
{
    public class MessagesSend : APIRequest
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="chat_id"></param>
        /// <param name="message"></param>
        /// <param name="attachment"></param>
        /// <param name="forward_messages"></param>
        /// <param name="title"></param>
        /// <param name="type">0 - обычное сообщение, 1 - сообщение из чата</param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="guid"></param>
        /// <param name="callback"></param>
        public MessagesSend(int uid, int chat_id, string message, string attachment, string forward_messages, string title,
            int type, string latitude, string longitude, string guid, Action<int> callback)
            : base("messages.send")
        {
            // uid or chat_id should be assigned anyway.
            if (uid == -1 && chat_id > -1)
                base.AddParameter("chat_id", chat_id.ToString());
            else if (chat_id == -1 && uid > -1)
                base.AddParameter("uid", uid.ToString());

            // uid or chat_id should be assigned anyway.
            if (!string.IsNullOrEmpty(attachment))
                base.AddParameter("attachment", attachment);

            if (!string.IsNullOrEmpty(message))
                base.AddParameter("message", message);

            base.AddParameter("forward_messages", forward_messages); // comma delimited forward message ids: 123, 431, 544

            if (!string.IsNullOrEmpty(title))
                base.AddParameter("title", title);

            base.AddParameter("type", type.ToString());//0 - common message, 1 - chat message (default is 0).

            if (!string.IsNullOrEmpty(latitude) && !string.IsNullOrEmpty(longitude))
            {
                base.AddParameter("lat", latitude.Replace(",", "."));//optional
                base.AddParameter("long", longitude.Replace(",", "."));//optional
            }

            base.AddParameter("guid", guid);// unique id to be sure about sending message only once

            base.SetSuccessHandler(_ParseResponse);

            _callback = callback;
        }

        private void _ParseResponse(string response)
        {
            try
            {
                MessageId mid = SerializeHelper.Deserialise<MessageId>(response);

                _callback(mid.Mid);
            }
            catch
            {
                Debug.WriteLine("Parse response from MessagesSend failed.");

                _callback(-1);
            }
        }

        private Action<int> _callback;
    }
}
