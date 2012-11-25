using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SlXnaApp1.Json
{
    [DataContract]
    public class MessagesUploadServerResponse
    {
        [DataMember(Name = "response")]
        public MessagesUploadServer MessagesUploadServer { get; set; }
    }

    [DataContract(Name = "response")]
    public class MessagesUploadServer
    {
        #region Public properties

        [DataMember(Name = "upload_url")]
        public string UploadUrl { get; set; }

        [DataMember(Name = "aid")]
        public int Aid { get; set; }

        [DataMember(Name = "mid")]
        public int Mid { get; set; }

        #endregion
    }
}
