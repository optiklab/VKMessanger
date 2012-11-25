using System.Runtime.Serialization;
using SlXnaApp1.Json;

namespace SlXnaApp1.Entities
{
    [DataContract()]
    public class Attachment
    {
        #region Public properties

        public virtual AttachmentType AttachType
        {
            get
            {
                return AttachmentType.None;
            }
        }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "video")]
        public VideoAttachment VideoAttachment { get; set; }

        [DataMember(Name = "audio")]
        public AudioAttachment AudioAttachment { get; set; }

        [DataMember(Name = "doc")]
        public DocAttachment DocAttachment { get; set; }

        [DataMember(Name = "photo")]
        public PhotoAttachment PhotoAttachment { get; set; }

        #endregion
    }
}
