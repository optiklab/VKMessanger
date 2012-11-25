using System.Windows;
using SlXnaApp1.Entities;

namespace SlXnaApp1.Views
{
    public class FriendsCategoryTemplateSelector : DataTemplateSelector
    {
        public DataTemplate IsHints
        {
            get;
            set;
        }

        public DataTemplate IsCategory
        {
            get;
            set;
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            PeopleInGroup<FriendViewModel> friendsCategory = item as PeopleInGroup<FriendViewModel>;

            if (friendsCategory != null)
            {
                if (string.IsNullOrEmpty(friendsCategory.Key))
                {
                    return IsHints;
                }
                else
                {
                    return IsCategory;
                }
            }

            return base.SelectTemplate(item, container);
        }
    }
}
