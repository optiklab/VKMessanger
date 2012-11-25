using System.Runtime.Serialization;

namespace SlXnaApp1.Json
{
    [DataContract]
    public class SentAvPhoto
    {
        [DataMember(Name = "server")]
        public string Server { get; set; }

        [DataMember(Name = "photos")]
        public string Photos { get; set; }

        [DataMember(Name = "hash")]
        public string Hash { get; set; }
    }
}