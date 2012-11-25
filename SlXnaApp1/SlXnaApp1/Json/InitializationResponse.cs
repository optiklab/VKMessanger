using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SlXnaApp1.Json
{
    [DataContract]
    public class InitializationResponse
    {
        #region Public properties

        [DataMember(Name = "response")]
        public InitializationRResponse InitializationRResponse { get; set; }

        #endregion
    }

    [DataContract(Name = "response")]
    public class InitializationRResponse
    {
        #region Public properties

        [DataMember(Name = "a")]
        public IList<Message> Messages { get; set; }

        [DataMember(Name = "p")]
        public IList<UserInfo> Profiles { get; set; }

        [DataMember(Name = "p2")]
        public IList<UserInfo> ChatProfiles { get; set; }

        #endregion
    }
}
