using System.Runtime.Serialization;
using SlXnaApp1.Entities;

namespace SlXnaApp1.Json
{
    [DataContract(Name = "doc")]
    public class DocAttachment
    {
        #region Public properties

        [DataMember(Name = "did")]
        public int Did { get; set; }

        [DataMember(Name = "owner_id")]
        public int OwnerId { get; set; }

        [DataMember(Name = "size")]
        public int Size { get; set; }

        [DataMember(Name = "ext")]
        public string Ext { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "url")]
        public string Url { get; set; }

        #endregion
    }
}
