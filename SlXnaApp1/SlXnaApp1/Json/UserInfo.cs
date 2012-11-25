using System.Runtime.Serialization;
using System.Windows.Media.Imaging;
using SlXnaApp1.Entities;
using System.Windows;

namespace SlXnaApp1.Json
{
    [DataContract]
    public class UserInfo : ISearchable
    {
        #region Common parameters

        [DataMember(Name = "uid")]
        public int Uid { get; set; }

        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }

        [DataMember(Name = "first_name")]
        public string FirstName { get; set; }

        [DataMember(Name = "last_name")]
        public string LastName { get; set; }

        [DataMember(Name = "nickname")]
        public string Nickname { get; set; }

        [DataMember(Name = "activity")]
        public string Activity { get; set; }

        /// <summary>
        /// 1 - women
        /// 2 - men
        /// 0 - none
        /// </summary>
        [DataMember(Name = "sex")]
        public int Sex { get; set; }

        /// <summary>
        /// 1 - not married
        /// 2 - friend
        /// 3 - engaged
        /// 4 - married
        /// 5 - difficult
        /// 6 - active search
        /// 7 - in love
        /// </summary>
        [DataMember(Name = "relation")]
        public string Relation { get; set; }

        /// <summary>
        /// "23.11.1981" or "21.9"
        /// </summary>
        [DataMember(Name = "bdate")]
        public string Birthdate { get; set; }

        [DataMember(Name = "city")]
        public string City { get; set; }

        /// <summary>
        /// Country id to get full name with separate request "getCountries".
        /// </summary>
        [DataMember(Name = "country")]
        public string Country { get; set; }

        [DataMember(Name = "timezone")]
        public string Timezone { get; set; }

        /// <summary>
        /// 50 pixels photo or default.
        /// </summary>
        [DataMember(Name = "photo")]
        public string Photo { get; set; }

        /// <summary>
        /// 100 pixels photo or default.
        /// </summary>
        [DataMember(Name = "photo_medium")]
        public string PhotoMedium { get; set; }

        /// <summary>
        /// 200 pixels photo or default.
        /// </summary>
        [DataMember(Name = "photo_big")]
        public string PhotoBig { get; set; }

        /// <summary>
        /// 50 pixels square photo or default.
        /// </summary>
        [DataMember(Name = "photo_rec")]
        public string PhotoRec { get; set; }

        /// <summary>
        /// 100 pixels square photo or default.
        /// </summary>
        [DataMember(Name = "photo_medium_rec")]
        public string PhotoMediumRec { get; set; }

        [DataMember(Name = "rate")]
        public string Rate { get; set; }

        [DataMember(Name = "last_seen")]
        public string LastSeenTime { get; set; }

        #endregion

        #region Contacts parameter adds

        [DataMember(Name = "has_mobile")]
        public bool HasMobile  { get; set; }

        [DataMember(Name = "home_phone")]
        public string HomePhone { get; set; }

        [DataMember(Name = "mobile_phone")]
        public string MobilePhone { get; set; }

        /// <summary>
        /// Actual verified phone number.
        /// </summary>
        [DataMember(Name = "phone")]
        public string VerifiedPhone { get; set; }

        #endregion

        #region Education parameter adds

        [DataMember(Name = "university")]
        public string University { get; set; }

        [DataMember(Name = "university_name")]
        public string UniversityName { get; set; }

        [DataMember(Name = "faculty")]
        public string Faculty { get; set; }

        [DataMember(Name = "faculty_name")]
        public string FaculteName { get; set; }

        [DataMember(Name = "graduation")]
        public string Graduation { get; set; }

        //[DataMember(Name = "universities")]
        //public string Universities { get; set; }

        #endregion

        #region Counters + only one Uid in parameters adds

        /// <summary>
        /// "getProfiles" specific.
        /// </summary>
        [DataMember(Name = "counters")]
        public ProfileInfo Counters { get; set; }

        #endregion

        #region Other

        [DataMember(Name = "online")]
        public bool IsOnline { get; set; }

        // relatives
        // Returns list of objects with fields uid and type, where
        // type can be one of grandchild, grandparent, child, sibling, parent.

        // interests
        // movies
        // tv
        // books
        // games
        // about

        // connections
        // Returns keys: twitter, facebook, facebook_name, skype, livejounal.

        #endregion

        #region UI Props

        public BitmapImage ImagePhoto
        {
            get
            {
                if (_photo == null)
                    return App.Current.EntityService.DefaultAvatar;

                return _photo;
            }
            set
            {
                _photo = value;
            }
        }

        public Visibility IsOnlineFlagVisibility
        {
            get
            {
                if (IsOnline)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        #endregion

        #region Private fields

        private BitmapImage _photo;

        #endregion
    }
}
