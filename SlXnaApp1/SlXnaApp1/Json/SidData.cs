using System.Runtime.Serialization;

namespace SlXnaApp1.Json
{
    [DataContract]
    public class SidDataResponse
    {
        #region Public properties

        [DataMember(Name = "response")]
        public SidData SidData
        {
            get;
            set;
        }

        #endregion
    }

    [DataContract(Name = "response")]
    public class SidData
    {
        #region Public properties

        [DataMember(Name = "sid")]
        public string Sid
        {
            get;
            set;
        }

        #endregion
    }
}
