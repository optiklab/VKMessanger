using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SlXnaApp1.Json
{
    [DataContract]
    public class MessagesResponse
    {
        #region Public properties

        [DataMember(Name = "response")]
        public IList<Message> Messages { get; set; }

        #endregion
    }
}
