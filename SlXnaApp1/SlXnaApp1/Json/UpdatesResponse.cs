using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SlXnaApp1.Json
{
    [DataContract]
    public class UpdatesResponse
    {
        #region Public properties

        [DataMember(Name = "ts")]
        public int Ts { get; set; }

        [DataMember(Name = "updates")]
        public IList<IList<object>> Updates { get; set; }

        #endregion
    }
}
