using System.Runtime.Serialization;

namespace SlXnaApp1.Json
{
    [DataContract(Name = "response")]
    public class SuccessEntity
    {
        [DataMember(Name = "response")]
        public int Succeed { get; set; }
    }
}
