using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SlXnaApp1.Json
{
    [DataContract]
    public class FriendRequestsWithMessagesResponse
    {
        #region Constructor

        public FriendRequestsWithMessagesResponse()
        {
            FriendRequests = new List<FriendRequestEntity>();
        }

        #endregion

        #region Public properties

        [DataMember(Name = "response")]
        public IList<FriendRequestEntity> FriendRequests { get; set; }

        #endregion
    }
}
