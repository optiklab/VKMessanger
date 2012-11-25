using System.Runtime.Serialization;
using System.Collections.Generic;

namespace SlXnaApp1.Json
{
    [DataContract]
    public class StatResponse
    {
        #region Public properties

        [DataMember(Name = "response")]
        public StatRResponse StatRResponse { get; set; }

        #endregion
    }

    [DataContract(Name = "response")]
    public class StatRResponse
    {
        #region Public properties

        [DataMember(Name = "a")]
        public IList<Message> Messages { get; set; }

        [DataMember(Name = "p")]
        public IList<FriendRequestEntity> FriendRequests { get; set; }

        #endregion
    }
}
