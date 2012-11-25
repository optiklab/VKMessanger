using System;
using System.Collections.Generic;
using SlXnaApp1.Json;
using System.Diagnostics;

namespace SlXnaApp1.Api
{
    public class DialogsGet : APIRequest
    {
        public DialogsGet(int count,
                          int offset,
                          IList<Friend> friends,
                          Action<IList<Dialog>> callback) //TODO preview_length
            : base(METHOD_NAME)
        {
            // Can't get more than 200 per request.
            if (count < 0 || count > 200)
                Debug.Assert(false);

            if (offset < 0)
                Debug.Assert(false);

            base.AddParameter(COUNT, count.ToString());
            base.AddParameter(OFFSET, offset.ToString());
            base.AddParameter("preview_length", "30");

            base.SetSuccessHandler(_ParseResponse);

            _friends = friends;
            _callback = callback;
        }

        private void _ParseResponse(string response)
        {
            try
            {
                if (response == null)
                    _callback(new List<Dialog>());
                else
                {
                    response = SerializeHelper.FixResponseArrayString(response);
                    MessagesResponse messagesResponse = SerializeHelper.Deserialise<MessagesResponse>(response);

                    List<Dialog> dialogs = new List<Dialog>();

                    if (messagesResponse.Messages != null)
                    {
                        foreach (var message in messagesResponse.Messages)
                        {
                            //if (message != null)
                            //    dialogs.Add(new Dialog(message, _friends, App.Current.EntityService.DefaultAvatar));
                        }
                    }

                    _callback(dialogs);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Parse response from DialogsGet failed." + ex.Message + "\n" + ex.StackTrace);
                _callback(new List<Dialog>());
            }
        }

        #region Private constants

        private const string METHOD_NAME = "messages.getDialogs";
        private const string COUNT = "count";
        private const string OFFSET = "offset";

        #endregion

        #region Private fields

        private Action<IList<Dialog>> _callback;
        private IList<Friend> _friends;

        #endregion
    }
}
