using System.Runtime.Serialization;

namespace SlXnaApp1.Json
{
    [DataContract]
    public class SentPhoto
    {
        [DataMember(Name = "server")]
        public string Server { get; set; }

        [DataMember(Name = "photo")]
        public string Photo { get; set; }

        [DataMember(Name = "hash")]
        public string Hash { get; set; }
    }
}
