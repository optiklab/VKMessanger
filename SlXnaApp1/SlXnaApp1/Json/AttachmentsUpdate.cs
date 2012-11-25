using System;
using System.Runtime.Serialization;

namespace SlXnaApp1.Json
{
    [DataContract]
    public class AttachmentsUpdate
    {
        #region Public properties

        [DataMember(Order=1)]
        public string AttachType
        {
            get;
            set;
        }

        [DataMember(Order=2)]
        public string Attach
        {
            get;
            set;
        }

        [DataMember(Name="fwd")]
        public string Forward
        {
            get;
            set;
        }

        [DataMember(Name="from")]
        public string From
        {
            get;
            set;
        }

        #endregion
    }
}
