using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SlXnaApp1.Json
{
    [DataContract]
    public class Update
    {
        #region Public properties

        [DataMember(Order=1)]
        public int Type
        {
            get;
            set;
        }

        /// <summary>
        /// Message, chat or user id.
        /// </summary>
        [DataMember(Order = 2)]
        public int FirstId
        {
            get;
            set;
        }

        [DataMember(Order = 3)]
        public int SecondId
        {
            get;
            set;
        }

        [DataMember(Order = 4, IsRequired = false)]
        public int ThirdId
        {
            get;
            set;
        }

        [DataMember(Order = 5, IsRequired = false)]
        public int Timestamp
        {
            get;
            set;
        }

        [DataMember(Order = 6, IsRequired = false)]
        public string Subject
        {
            get;
            set;
        }

        [DataMember(Order = 7, IsRequired = false)]
        public string Text
        {
            get;
            set;
        }

        [DataMember(Order = 8, IsRequired = false)]
        public IList<AttachmentsUpdate> Attachments
        {
            get;
            set;
        }

        #endregion
    }
}
