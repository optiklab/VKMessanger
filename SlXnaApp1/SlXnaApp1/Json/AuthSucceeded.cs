using System.Runtime.Serialization;

namespace TestViKeyUtility
{
    [DataContract]
    public class AuthSucceeded
    {
        [DataMember]
        public string user_id { get; set; }

        [DataMember]
        public string access_token { get; set; }

        [DataMember]
        public string secret { get; set; }
    }
}
