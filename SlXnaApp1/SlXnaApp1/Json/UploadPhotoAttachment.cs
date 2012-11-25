using System.Runtime.Serialization;
using System.Collections.Generic;

namespace SlXnaApp1.Json
{
    [DataContract]
    public class UploadPhotoAttachmentResponse
    {
        [DataMember(Name = "response")]
        public IList<UploadPhotoAttachment> UploadPhotoAttachment { get; set; }
    }

    [DataContract]
    public class UploadPhotoAttachment
    {
        #region Public properties

        [DataMember(Name = "id")]
        public string Id { get; set; }

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

        [DataMember(Name = "src_xbig")]
        public string SourceExtraBig { get; set; }

        [DataMember(Name = "width")]
        public int Width { get; set; }

        [DataMember(Name = "height")]
        public int Height { get; set; }

        [DataMember(Name = "text")]
        public string Text { get; set; }

        [DataMember(Name = "created")]
        public long Created { get; set; }

        #endregion
    }
}
