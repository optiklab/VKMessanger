using System;
using System.Diagnostics;
using System.Linq;
using SlXnaApp1.Json;

namespace SlXnaApp1.Api
{
    public class PhotosSaveMessagesPhoto : APIRequest
    {
        public PhotosSaveMessagesPhoto(string server, string photo, string hash, Action<UploadPhotoAttachment> callback)
            : base("photos.saveMessagesPhoto")
        {
            base.AddParameter("server", server);
            base.AddParameter("photo", photo);
            base.AddParameter("hash", hash);

            base.SetSuccessHandler(_ParseResponse);

            _callback = callback;
        }

        private void _ParseResponse(string response)
        {
            try
            {
                var photoAttachment = SerializeHelper.Deserialise<UploadPhotoAttachmentResponse>(response);

                var res = photoAttachment.UploadPhotoAttachment.FirstOrDefault();

                _callback(res);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Parse response from PhotosSaveMessagesPhoto failed:" + ex.Message);

                _callback(null);
            }
        }

        private Action<UploadPhotoAttachment> _callback;
    }
}
