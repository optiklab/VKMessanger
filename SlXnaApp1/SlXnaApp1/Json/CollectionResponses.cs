using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SlXnaApp1.Json
{
    [DataContract]
    public class FriendsResponse
    {
        #region Public properties

        [DataMember(Name = "response")]
        public IList<Friend> Friends { get; set; }

        #endregion
    }

    [DataContract]
    public class SearchResponse
    {
        #region Public properties

        [DataMember(Name = "response")]
        public IList<SearchResult> SearchResults { get; set; }

        #endregion
    }
}
