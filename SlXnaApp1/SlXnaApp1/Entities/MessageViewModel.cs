using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Linq;
using System.Diagnostics;

namespace SlXnaApp1.Entities
{
    public class MessageViewModel : INotifyPropertyChanged
    {
        #region Constructor

        public MessageViewModel(int uid, int mid, string body, string dateTime, bool isOut, bool isSent, bool isRead,
            bool isDeleted, BitmapImage accountPhoto, ObservableCollection<AttachmentViewModel> attachments)
        {
            _isSelected = false;
            Mid = mid;
            Uid = uid;

            _body = body;
            _dateTime = dateTime;
            _photo = accountPhoto;

            _SetUserData(uid);

            _isOut = isOut;
            _isRead = isRead;
            _isDeleted = isDeleted;

            if (isSent && isOut && !isRead)
                _notReadVisibility = Visibility.Visible;
            else
                _notReadVisibility = Visibility.Collapsed;

            _isSent = isSent;

            _attachments = attachments;
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

        public bool IsOut
        {
            get
            {
                return _isOut;
            }
            set
            {
                if (_isOut != value)
                {
                    if (_isSent && value && !_isRead)
                        NotReadVisibility = Visibility.Visible;
                    else
                        NotReadVisibility = Visibility.Collapsed;

                    _isOut = value;
                    OnPropertyChanged("IsOut");
                }
            }
        }

        public bool IsDeleted
        {
            get
            {
                return _isDeleted;
            }
            set
            {
                if (_isDeleted != value)
                {
                    _isDeleted = value;
                    OnPropertyChanged("IsDeleted");
                    OnPropertyChanged("MessageVisibility");
                }
            }
        }

        public int Uid
        {
            get;
            private set;
        }

        public int Mid
        {
            get;
            private set;
        }

        public string FullName
        {
            get;
            private set;
        }

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
                    OnPropertyChanged("SelectedVisibility");
                }
            }
        }

        public bool IsRead
        {
            get
            {
                return _isRead;
            }
            set
            {
                if (_isRead != value)
                {
                    if (_isSent && _isOut && !value)
                        NotReadVisibility = Visibility.Visible;
                    else
                        NotReadVisibility = Visibility.Collapsed;

                    _isRead = value;
                    OnPropertyChanged("IsRead");
                }
            }
        }

        public bool IsSent
        {
            get
            {
                return _isSent;
            }
            set
            {
                if (_isSent != value)
                {
                    _isSent = value;
                    OnPropertyChanged("IsSent");
                    OnPropertyChanged("NotSentVisibility");
                }
            }
        }

        public string Body
        {
            get
            {
                return _body;
            }
            set
            {
                if (_body!= value)
                {
                    _body = value;
                    OnPropertyChanged("Body");
                }
            }
        }

        public string DateTime
        {
            get
            {
                return _dateTime;
            }
            set
            {
                if (_dateTime != value)
                {
                    _dateTime = value;
                    OnPropertyChanged("DateTime");
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

        public Visibility NotSentVisibility
        {
            get
            {
                if (_isSent)
                    return Visibility.Collapsed;
                else
                    return Visibility.Visible;
            }
        }

        public Visibility IsDeletedVisibility
        {
            get
            {
                if (_isDeleted)
                    return Visibility.Collapsed;
                else
                    return Visibility.Visible;
            }
        }

        public Visibility NotReadVisibility
        {
            get
            {
                return _notReadVisibility;
            }
            private set
            {
                if (_notReadVisibility != value)
                {
                    _notReadVisibility = value;
                    OnPropertyChanged("NotReadVisibility");
                }
            }
        }

        public Visibility SelectedVisibility
        {
            get
            {
                if (IsSelected)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        public ObservableCollection<AttachmentViewModel> Attachments
        {
            get
            {
                return _attachments;
            }
            set
            {
                if (_attachments != value)
                {
                    _attachments = value;
                    OnPropertyChanged("Attachments");
                }
            }
        }

        #endregion

        #region Private methods

        private void _SetUserData(int uid)
        {
            try
            {
                var friend = App.Current.EntityService.Friends.FirstOrDefault(x => x.Uid == uid);

                if (friend == null)
                {
                    if (uid == App.Current.EntityService.CurrentUser.UserInfo.Uid)
                    {
                        FullName = App.Current.EntityService.CurrentUser.FullName;

                        if (_photo == null)
                            _photo = App.Current.EntityService.CurrentUser.Photo;
                    }
                    else
                    {
                        // Sometimes messages may be sent by NON-FRIEND. In that case we may take photo from dialog.
                        var dialog = App.Current.EntityService.Dialogs.FirstOrDefault(x => x.Uid == uid && !x.IsConference);

                        if (dialog != null)
                        {
                            FullName = dialog.Title;

                            if (_photo == null)
                                _photo = dialog.Photo;
                        }
                    }
                }
                else
                {
                    FullName = friend.FullName;

                    if (_photo == null)
                        _photo = friend.Photo;
                }

                if (_photo == null)
                    _photo = App.Current.EntityService.DefaultAvatar;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("_SetUserData failed for uid =" + uid.ToString() + ": " + ex.Message);
            }
        }

        #endregion

        #region Private fields

        private bool _isSelected;
        private string _body;
        private bool _isOut;
        private bool _isRead;
        private bool _isDeleted;
        private bool _isSent;
        private string _dateTime;
        private BitmapImage _photo;
        private ObservableCollection<AttachmentViewModel> _attachments;
        private Visibility _notReadVisibility;

        #endregion
    }
}
