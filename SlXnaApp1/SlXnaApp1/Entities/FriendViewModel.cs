using System;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Windows;
using SlXnaApp1.Api;
using SlXnaApp1.Infrastructure;

namespace SlXnaApp1.Entities
{
    public class FriendViewModel : INotifyPropertyChanged, ISearchable
    {
        #region Constructor

        public FriendViewModel(int uid, string fullName, string contactName, string firstName, string lastName,
            bool isOnline, int hintOrder, string photoUri, BitmapImage photo)
        {
            _hintOrder = hintOrder;
            _uid = uid;
            _fullName = fullName;
            _contactName = contactName;
            _firstName = firstName;
            _lastName = lastName;
            _isOnline = isOnline;
            _photo = photo;
            _isSelected = false;
            PhotoUri = photoUri;
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

        public string PhotoUri { get; private set; }

        public string Message { get; set; }

        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }

        public int Uid
        {
            get
            {
                return _uid;
            }
            set
            {
                if (_uid != value)
                {
                    _uid = value;
                    OnPropertyChanged("Uid");
                }
            }
        }

        public bool IsOnline
        {
            get
            {
                return _isOnline;
            }
            set
            {
                if (_isOnline != value)
                {
                    _isOnline = value;
                    OnPropertyChanged("IsOnline");
                    OnPropertyChanged("IsOnlineFlagVisibility");
                }
            }
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

        public string VerifiedPhone
        {
            get
            {
                return _mobilePhone;
            }
            set
            {
                if (_mobilePhone != value)
                {
                    _mobilePhone = value;
                    OnPropertyChanged("VerifiedPhone");
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

        public string FirstName
        {
            get
            {
                return _firstName;
            }
            set
            {
                if (_firstName != value)
                {
                    _firstName = value;
                    OnPropertyChanged("FirstName");
                }
            }
        }

        public string LastName
        {
            get
            {
                return _lastName;
            }
            set
            {
                if (_lastName != value)
                {
                    _lastName = value;
                    OnPropertyChanged("LastName");
                }
            }
        }

        public string ContactName
        {
            get
            {
                return _contactName;
            }
            set
            {
                if (_contactName != value)
                {
                    _contactName = value;
                    OnPropertyChanged("ContactName");
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

        public int HintOrder
        {
            get { return _hintOrder; }
            set { _hintOrder = value; }
        }

        #endregion

        #region Private fields

        private string _mobilePhone;
        private string _fullName;
        private string _firstName;
        private string _lastName;
        private string _contactName;
        private BitmapImage _photo;
        private bool _isOnline;
        private int _uid;
        private bool _isSelected;
        private int _hintOrder;

        #endregion
    }
}
