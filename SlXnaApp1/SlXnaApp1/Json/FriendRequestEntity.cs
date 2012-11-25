using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SlXnaApp1.Json
{
    [DataContract]
    public class FriendRequestEntity
    {
        #region Public properties

        [DataMember(Name = "uid")]
        public int Uid { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; }

        [DataMember(Name = "mutual")]
        public MutualFriend MutualFriends { get; set; }

        #endregion
    }
}
