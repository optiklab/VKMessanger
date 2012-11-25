using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SlXnaApp1.Json
{
    [DataContract]
    public class FriendRequestsResponse
    {
        #region Public properties

        [DataMember(Name = "response")]
        public IList<int> Uids { get; set; }

        #endregion
    }
}
