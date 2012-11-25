using System.Runtime.Serialization;

namespace SlXnaApp1.Json
{
    [DataContract]
    public class ActivityStatusResponse
    {
        #region Public properties

        [DataMember(Name = "response")]
        public ActivityStatus ActivityStatus
        {
            get;
            set;
        }

        #endregion
    }

    [DataContract(Name="response")]
    public class ActivityStatus
    {
        #region Public properties

        /// <summary>
        /// !!!!!!!!!!!!! WARNNING !!!!!!!!!!!!! Online always returns 1 even when appeared status Offline.
        /// </summary>
        [DataMember(Name = "online")]
        public int Online
        {
            get;
            set;
        }

        [DataMember(Name = "time")]
        public int Time
        {
            get;
            set;
        }

        #endregion
    }
}
