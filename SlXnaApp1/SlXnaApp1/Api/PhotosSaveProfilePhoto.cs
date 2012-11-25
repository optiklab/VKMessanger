using System;
using System.Diagnostics;
using System.Linq;
using SlXnaApp1.Json;

namespace SlXnaApp1.Api
{
    public class PhotosSaveProfilePhoto : APIRequest
    {
        public PhotosSaveProfilePhoto(string server, string photo, string hash, Action<UploadProfilePhoto> callback)
            : base("photos.saveProfilePhoto")
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
                if (response == null)
                    _callback(null);
                else
                {
                    var photo = SerializeHelper.Deserialise<UploadProfilePhotoResponse>(response);

                    _callback(photo.UploadProfilePhoto);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Parse response from PhotosSaveProfilePhoto failed:" + ex.Message);

                _callback(null);
            }
        }

        private Action<UploadProfilePhoto> _callback;
    }
}
