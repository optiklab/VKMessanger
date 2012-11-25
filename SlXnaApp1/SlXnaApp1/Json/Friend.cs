using System.Runtime.Serialization;

namespace SlXnaApp1.Json
{
    [DataContract]
    public class Friend
    {
        #region Constructor

        public Friend()
        {
            FirstName = string.Empty;
            LastName = string.Empty;
            ContactName = string.Empty;
            //MobilePhone = string.Empty;
            //Phone = string.Empty;
            Photo = string.Empty;
        }

        #endregion

        #region Public properties

        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }

        [DataMember(Name="first_name")]
        public string FirstName { get; set; }

        [DataMember(Name="last_name")]
        public string LastName { get; set; }

        //[DataMember(Name="mobile_phone")]
        //public string MobilePhone { get; set; }

        public string ContactName { get; set; }

        [DataMember(Name="online")]
        public bool IsOnline { get; set; }

        [DataMember(Name = "online_mobile")]
        public string OnlineMobile { get; set; }

        [DataMember(Name = "online_app")]
        public string OnlineApp { get; set; }

        [DataMember(Name = "photo_rec")]
        public string Photo { get; set; }

        [DataMember(Name = "photo_medium_rec")]
        public string PhotoMedium { get; set; }

        [DataMember(Name="uid")]
        public int Uid { get; set; }

        [DataMember(Name = "has_mobile")]
        public bool HasMobile { get; set; }

        /// <summary>
        /// This is my own property - not VK!!!
        /// It is for filter out 5 first hints from other friends.
        /// </summary>
        [DataMember(Name = "hint_order")]
        public int HintOrder { get; set; }

        // TODO Think
        //[DataMember(Name = "lists")]

        #endregion
    }
}
