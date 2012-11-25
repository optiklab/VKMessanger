using System.Runtime.Serialization;
using System.Collections.Generic;

namespace SlXnaApp1.Json
{
    [DataContract]
    public class SearchResult
    {
        #region Public properties

        [DataMember(Name = "type")]
        public string Type
        {
            get;
            set;
        }

        [DataMember(Name = "uid")]
        public int Uid
        {
            get;
            set;
        }

        [DataMember(Name = "first_name")]
        public string FirstName
        {
            get;
            set;
        }

        [DataMember(Name = "last_name")]
        public string LastName
        {
            get;
            set;
        }

        [DataMember(Name = "chat_id")]
        public int ChatId
        {
            get;
            set;
        }

        [DataMember(Name = "title")]
        public string Title
        {
            get;
            set;
        }

        [DataMember(Name = "users")]
        public List<int> Users
        {
            get;
            set;
        }

        [DataMember(Name = "email")]
        public string Email
        {
            get;
            set;
        }

        #endregion
    }
}
