using System.Windows;
using SlXnaApp1.Json;
using SlXnaApp1.Entities;

namespace SlXnaApp1.Views
{
    public class MessageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate IsOutMessage
        {
            get;
            set;
        }

        public DataTemplate IsInMessage
        {
            get;
            set;
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            MessageViewModel message = item as MessageViewModel;
            if (message != null)
            {
                if (message.IsOut)
                {
                    return IsOutMessage;
                }
                else
                {
                    return IsInMessage;
                }
            }

            return base.SelectTemplate(item, container);
        }
    }
}
