using System;
using System.Runtime.Serialization;

namespace SlXnaApp1.Json
{
    [DataContract]
    public class LongPollServerResponse
    {
        [DataMember(Name = "response")]
        public LongPollServerInfo LongPollServerInfo
        {
            get;
            set;
        }
    }

    [DataContract(Name = "response")]
    public class LongPollServerInfo
    {
        #region Public properties

        [DataMember(Name = "ts")]
        public int Ts
        {
            get;
            set;
        }

        [DataMember(Name = "key")]
        public string Key
        {
            get;
            set;
        }

        [DataMember(Name = "server")]
        public string Server
        {
            get;
            set;
        }

        #endregion
    }
}
