using System.Runtime.Serialization;

namespace SlXnaApp1.Json
{
    [DataContract]
    public class UploadProfilePhotoResponse
    {
        [DataMember(Name = "response")]
        public UploadProfilePhoto UploadProfilePhoto { get; set; }
    }

    [DataContract(Name = "response")]
    public class UploadProfilePhoto
    {
        #region Public properties

        [DataMember(Name = "photo_hash")]
        public string Hash { get; set; }

        [DataMember(Name = "photo_src")]
        public string Source { get; set; }

        [DataMember(Name = "photo_src_big")]
        public string SourceBig { get; set; }

        [DataMember(Name = "photo_src_small")]
        public string SourceSmall { get; set; }

        #endregion
    }
}
