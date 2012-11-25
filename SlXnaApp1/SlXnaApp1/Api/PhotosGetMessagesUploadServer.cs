using System;
using SlXnaApp1.Json;
using System.Diagnostics;

namespace SlXnaApp1.Api
{
    public class PhotosGetMessagesUploadServer : APIRequest
    {
        public PhotosGetMessagesUploadServer(Action<string> callback)
            : base("photos.getMessagesUploadServer")
        {
            base.SetSuccessHandler(_ParseResponse);

            _callback = callback;
        }

        private void _ParseResponse(string response)
        {
            try
            {
                var uploadServer = SerializeHelper.Deserialise<MessagesUploadServerResponse>(response);

                _callback(uploadServer.MessagesUploadServer.UploadUrl);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Parse response from PhotosGetMessagesUploadServer failed." + ex.Message);

                _callback(string.Empty);
            }
        }

        private Action<string> _callback;
    }
}
