using System.Runtime.Serialization;

namespace SlXnaApp1.Json
{
    [DataContract(Name = "response")]
    public class MessageId
    {
        [DataMember(Name = "response")]
        public int Mid { get; set; }
    }
}
