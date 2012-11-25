using System.Runtime.Serialization;

namespace SlXnaApp1.Json
{
    [DataContract]
    public class APIError
    {
        [DataMember(Name = "error")]
        public ErrorDescription ErrorDescription { get; set; }
    }

    [DataContract(Name = "error")]
    public class ErrorDescription
    {
        [DataMember(Name = "error_code")]
        public int error_code { get; set; }

        [DataMember(Name = "error_msg")]
        public string error_msg { get; set; }

        [DataMember(Name = "captcha_sid")]
        public string captcha_sid { get; set; }

        [DataMember(Name = "captcha_img")]
        public string captcha_img { get; set; }
    }
}
