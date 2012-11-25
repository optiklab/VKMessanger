using System.Runtime.Serialization;
using SlXnaApp1.Entities;

namespace SlXnaApp1.Json
{
    [DataContract(Name="video")]
    public class VideoAttachment
    {
        #region Public properties

        [DataMember(Name = "vid")]
        public int Vid { get; set; }

        [DataMember(Name = "owner_id")]
        public int OwnerId { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "duration")]
        public int Duration { get; set; }

        [DataMember(Name = "image")]
        public string Image { get; set; }

        [DataMember(Name = "image_big")]
        public string ImageBig { get; set; }

        [DataMember(Name = "image_small")]
        public string ImageSmall { get; set; }

        [DataMember(Name = "views")]
        public int Views { get; set; }

        [DataMember(Name = "date")]
        public int Date { get; set; }

        [DataMember(Name = "player")]
        public string Player { get; set; }

        #endregion
    }
}
