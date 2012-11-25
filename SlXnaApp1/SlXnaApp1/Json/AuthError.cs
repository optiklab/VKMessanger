using System.Runtime.Serialization;

namespace SlXnaApp1.Json
{
    [DataContract]
    public class AuthError
    {
        [DataMember]
        public string error { get; set; }

        [DataMember]
        public string captcha_sid { get; set; }

        [DataMember]
        public string captcha_img { get; set; }
    }
}
