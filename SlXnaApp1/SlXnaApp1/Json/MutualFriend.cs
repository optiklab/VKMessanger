using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SlXnaApp1.Json
{
    [DataContract]
    public class MutualFriend
    {
        #region Public properties

        [DataMember(Name = "count")]
        public int Count { get; set; }

        [DataMember(Name = "users")]
        public IList<int> Uids { get; set; }

        #endregion
    }
}
