using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;

namespace SlXnaApp1.Entities
{
    public class GroupToBrushValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var groupFriends = value as PeopleInGroup<FriendViewModel>;
            var groupContacts = value as PeopleInGroup<PhoneContact>;
            object result = null;

            if (groupFriends != null)
            {
                if (groupFriends.Count == 0)
                    result = (SolidColorBrush)Application.Current.Resources["PhoneChromeBrush"];
                else
                    result = (SolidColorBrush)Application.Current.Resources["PhoneAccentBrush"];//BlueColorBrush"];
            }
            else if (groupContacts != null)
            {
                if (groupContacts.Count == 0)
                    result = (SolidColorBrush)Application.Current.Resources["PhoneChromeBrush"];
                else
                    result = (SolidColorBrush)Application.Current.Resources["PhoneAccentBrush"];//BlueColorBrush"];
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
