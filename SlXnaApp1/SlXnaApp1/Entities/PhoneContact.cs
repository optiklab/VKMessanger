using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows.Media.Imaging;
using System.Windows;

namespace SlXnaApp1.Entities
{
    // Contract is used to save this entity in cache (storage).
    [DataContract]
    public class PhoneContact : INotifyPropertyChanged, ISearchable
    {
        #region Constructor

        public PhoneContact(string contactName, string mobilePhone, BitmapImage photo)
        {
            _contactName = contactName;
            _mobilePhone = mobilePhone;
            _photo = photo;
            _needRequest = true;
            _fullName = string.Empty;
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

        [DataMember(Name = "NeedRequest")]
        public bool NeedRequest
        {
            get
            {
                return _needRequest;
            }
            set
            {
                if (_needRequest != value)
                {
                    _needRequest = value;
                    OnPropertyChanged("NeedRequest");
                }
            }
        }

        [DataMember(Name = "Uid")]
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

        [DataMember(Name = "FullName")]
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

        [DataMember(Name = "VerifiedPhone")]
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

        [DataMember(Name = "ContactName")]
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

        #endregion

        #region Private fields

        private string _fullName;
        private string _contactName;
        private BitmapImage _photo;
        private int _uid;
        private string _mobilePhone;
        private bool _needRequest;

        #endregion
    }
}
