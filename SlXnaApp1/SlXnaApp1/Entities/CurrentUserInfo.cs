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
using SlXnaApp1.Json;

namespace SlXnaApp1.Entities
{
    public class CurrentUserInfo: INotifyPropertyChanged
    {
        #region Constructor

        public CurrentUserInfo(UserInfo userInfo, BitmapImage photo)
        {
            if (userInfo != null)
            {
                _userInfo = userInfo;
                _fullName = userInfo.FullName;
            }

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

        public UserInfo UserInfo
        {
            get
            {
                return _userInfo;
            }
            set
            {
                if (_userInfo != value)
                {
                    _userInfo = value;
                    OnPropertyChanged("UserInfo");

                    FullName = _userInfo.FullName;
                }
            }
        }

        public string FullName
        {
            get
            {
                return _fullName;
            }
            set
            {
                if (_fullName != value)
                {
                    _fullName = value;
                    OnPropertyChanged("FullName");
                }
            }
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

        private UserInfo _userInfo;
        private string _fullName;
        private BitmapImage _photo;

        #endregion
    }
}
