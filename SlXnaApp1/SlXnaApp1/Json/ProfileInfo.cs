using System.Runtime.Serialization;

namespace SlXnaApp1.Json
{
    [DataContract]
    public class ProfileInfo
    {
        #region Common parameters

        [DataMember(Name = "albums")]
        public int AlbumsCount { get; set; }

        [DataMember(Name = "videos")]
        public int VideosCount { get; set; }

        [DataMember(Name = "audios")]
        public int AudiosCount { get; set; }

        [DataMember(Name = "notes")]
        public int NotesCount { get; set; }

        [DataMember(Name = "friends")]
        public int FriendsCount { get; set; }

        [DataMember(Name = "groups")]
        public int GroupsCount { get; set; }

        [DataMember(Name = "online_friends")]
        public int OnlineFriendsCount { get; set; }

        [DataMember(Name = "mutual_friends")]
        public int MutualFriendsCount { get; set; }

        [DataMember(Name = "user_videos")]
        public int UserVideosCount { get; set; }

        [DataMember(Name = "followers")]
        public int FollowersCount { get; set; }

        #endregion
    }
}
