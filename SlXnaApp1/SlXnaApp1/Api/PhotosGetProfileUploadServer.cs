using System;
using System.Diagnostics;
using SlXnaApp1.Json;

namespace SlXnaApp1.Api
{
    public class PhotosGetProfileUploadServer : APIRequest
    {
        public PhotosGetProfileUploadServer(Action<string> callback)
            : base("photos.getProfileUploadServer")
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
                Debug.WriteLine("Parse response from PhotosGetProfileUploadServer failed." + ex.Message);

                _callback(string.Empty);
            }
        }

        private Action<string> _callback;
    }
}
