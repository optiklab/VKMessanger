using System.Runtime.Serialization;
using SlXnaApp1.Entities;

namespace SlXnaApp1.Json
{
    [DataContract(Name = "photo")]
    public class PhotoAttachment
    {
        #region Public properties

        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "pid")]
        public int Pid { get; set; }

        [DataMember(Name = "aid")]
        public int Aid { get; set; }

        [DataMember(Name = "owner_id")]
        public int OwnerId { get; set; }

        [DataMember(Name = "src")]
        public string Source { get; set; }

        [DataMember(Name = "src_big")]
        public string SourceBig { get; set; }

        [DataMember(Name = "src_small")]
        public string SourceSmall { get; set; }

        [DataMember(Name = "created")]
        public long Created { get; set; }

        #endregion
    }
}
