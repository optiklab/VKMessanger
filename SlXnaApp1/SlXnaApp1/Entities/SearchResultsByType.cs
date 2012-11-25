using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using SlXnaApp1.Json;
using System.Linq;

namespace SlXnaApp1.Entities
{
    public class SearchResultsByType : List<PeopleInGroup<ISearchable>>
    {
        public SearchResultsByType(IList<FriendViewModel> friends,
            IList<PhoneContact> contacts, IList<UserInfo> possible)
        {
            var groups = new Dictionary<string, PeopleInGroup<ISearchable>>();

            // Create groups.
            var friendsG = new PeopleInGroup<ISearchable>(AppResources.FriendsGroup);
            this.Add(friendsG);
            groups[AppResources.FriendsGroup] = friendsG;

            var contactsG = new PeopleInGroup<ISearchable>(AppResources.ContactsGroup);
            this.Add(contactsG);
            groups[AppResources.ContactsGroup] = contactsG;

            var possibleG = new PeopleInGroup<ISearchable>(AppResources.OtherUsersGroup);
            this.Add(possibleG);
            groups[AppResources.OtherUsersGroup] = possibleG;

            // Fill groups.
            groups[AppResources.FriendsGroup].AddRange(friends.ToArray());
            groups[AppResources.ContactsGroup].AddRange(contacts.ToArray());
            groups[AppResources.OtherUsersGroup].AddRange(possible.ToArray());
        }
    }
}
