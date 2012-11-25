using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SlXnaApp1.Json
{
    [DataContract]
    public class UsersInfoResponse
    {
        #region Public properties

        [DataMember(Name = "response")]
        public IList<UserInfo> UserInfos { get; set; }

        #endregion
    }
}
