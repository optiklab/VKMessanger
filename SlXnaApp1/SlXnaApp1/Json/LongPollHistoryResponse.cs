using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SlXnaApp1.Json
{
    [DataContract]
    public class LongPollHistoryResponse
    {
        [DataMember(Name = "response")]
        public LongPollHistory LongPollHistory { get; set; }
    }

    [DataContract(Name = "response")]
    public class LongPollHistory
    {
        #region Public properties

        [DataMember(Name = "history")]
        public IList<IList<object>> History { get; set; }

        [DataMember(Name = "messages")]
        public IList<Message> Messages { get; set; }

        #endregion
    }
}
