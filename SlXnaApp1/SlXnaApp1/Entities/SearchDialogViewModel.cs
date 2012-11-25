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
using System.ComponentModel;
using System.Windows.Media.Imaging;
using SlXnaApp1.Infrastructure;

namespace SlXnaApp1.Entities
{
    public class SearchDialogViewModel : INotifyPropertyChanged
    {
        #region Constructor

        public SearchDialogViewModel(int uid, int chatId, string fullName, string body,
            int time, bool isOut, bool isOnline, BitmapImage photo)
        {
            Uid = uid;
            ChatId = chatId;
            FullName = fullName;
            Message = body;
            Time = CommonHelper.GetFormattedDate(time);
            _isOut = isOut;
            _isOnline = isOnline;
            _photo = photo;
        }

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// PropertyChanged event.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// OnPropertyChanged event handler.
        /// </summary>
        /// <param name="propertyName">Changed property.</param>
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this,
                    new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Public properties

        public string Time
        {
            get;
            private set;
        }

        public string IsOut
        {
            get
            {
                if (_isOut)
                    return AppResources.SentMessage;
                else
                    return AppResources.IncomeMessage;
            }
        }

        public string Message
        {
            get;
            private set;
        }

        public int ChatId
        {
            get;
            private set;
        }

        public int Uid
        {
            get;
            private set;
        }

        public Visibility IsOnlineFlagVisibility
        {
            get
            {
                if (_isOnline)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        public string FullName
        {
            get;
            private set;
        }

        public BitmapImage Photo
        {
            get
            {
                return _photo;
            }
            set
            {
                if (_photo != value)
                {
                    _photo = value;
                    OnPropertyChanged("Photo");
                }
            }
        }

        #endregion

        #region Private fields

        private bool _isOut;
        private bool _isOnline;
        private BitmapImage _photo;

        #endregion
    }
}
