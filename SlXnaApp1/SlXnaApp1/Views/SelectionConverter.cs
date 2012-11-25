using System;
using System.Collections.Generic;
using System.Windows.Data;
using System.Windows.Documents;

namespace SlXnaApp1.Views
{
    public class SelectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            List<Inline> inlines = new List<Inline>();
            if (null != value)
            {
                string text = (string)value;

                string query = App.Current.EntityService.SearchQuery;

                int index = text.ToLower().IndexOf(query.ToLower());

                string begin = text.Substring(0, index);
                string selection = text.Substring(index, query.Length);
                string end = text.Substring(index + query.Length - 1);

                inlines.Add(new Run() { Text = begin, Foreground = App.Current.GrayBrush });
                inlines.Add(new Run() { Text = selection, Foreground = App.Current.BlueBrush });
                inlines.Add(new Run() { Text = end, Foreground = App.Current.GrayBrush });
            }

            return inlines;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    } 
}
