using System.Runtime.Serialization;

namespace SlXnaApp1.Json
{
    [DataContract]
    public class AuthConfirmResponse
    {
        [DataMember(Name = "response")]
        public AuthConfirmEntity AuthConfirmEntity
        {
            get;
            set;
        }

    }

    [DataContract(Name = "response")]
    public class AuthConfirmEntity
    {
        #region Public properties

        [DataMember(Name = "uid")]
        public int Uid
        {
            get;
            set;
        }

        [DataMember(Name = "success")]
        public int Success
        {
            get;
            set;
        }

        #endregion
    }
}
