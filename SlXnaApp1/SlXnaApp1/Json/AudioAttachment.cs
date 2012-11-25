using SlXnaApp1.Entities;
using System.Runtime.Serialization;

namespace SlXnaApp1.Json
{
    [DataContract(Name = "audio")]
    public class AudioAttachment
    {
        #region Public properties

        [DataMember(Name = "aid")]
        public int Aid { get; set; }

        [DataMember(Name = "owner_id")]
        public int OwnerId { get; set; }

        [DataMember(Name = "performer")]
        public string Author { get; set; }

        [DataMember(Name = "duration")]
        public int Duration { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "url")]
        public string Url { get; set; }

        #endregion
    }
}
