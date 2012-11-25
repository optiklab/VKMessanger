using System.Windows;
using SlXnaApp1.Entities;
using SlXnaApp1.Json;

namespace SlXnaApp1.Views
{
    public class SearchResultCategoryTemplateSelector : DataTemplateSelector
    {
        public DataTemplate IsFriends
        {
            get;
            set;
        }

        public DataTemplate IsContacts
        {
            get;
            set;
        }

        public DataTemplate IsOtherUsers
        {
            get;
            set;
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FriendViewModel friendsCategory = item as FriendViewModel;

            if (friendsCategory != null)
                return IsFriends;
            else
            {
                PhoneContact contactCategory = item as PhoneContact;

                if (contactCategory != null)
                    return IsContacts;
                else
                {
                    UserInfo info = item as UserInfo;

                    if (info != null)
                        return IsOtherUsers;
                }
            }

            return base.SelectTemplate(item, container);
        }
    }
}
