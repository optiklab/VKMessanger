using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Media.Imaging;
using SlXnaApp1.Infrastructure;
using System.Windows.Media;
using System.Windows;

namespace SlXnaApp1.Json
{
    // Contract is used to save this entity in cache (storage).
    [DataContract]
    public class Dialog : INotifyPropertyChanged
    {
        #region Constructor

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="users">All users from dialogs. Is need to find out friends related to this dialog.</param>
        /// <param name="photo"></param>
        public Dialog(Message message, IList<UserInfo> users)
        {
            bool isConference = message.Chatid > 0 ? true : false;

            if (isConference)
                _chatid = message.Chatid;
            else
            {
                _chatid = message.Mid;
                _mid = message.Mid;
            }

            _uid = message.Uid;
            _isConference = isConference;
            _isRead = message.IsRead;
            _attachmentCount = 1;

            if (string.IsNullOrEmpty(message.Body))
            {
                if (message.Attachments != null && message.Attachments.Any())
                {
                    var att = message.Attachments.FirstOrDefault();

                    if (att != null)
                        _lastMessage = att.Type;
                }
                else if (message.FwdMessages != null && message.FwdMessages.Any())
                {
                    _lastMessage = AppResources.ForwardedContentLabel;
                }
                else if (message.GeoLocation != null)
                {
                    _lastMessage = AppResources.LocContentLabel;
                }
                else
                {
                    _lastMessage = "<>";
                }
            }
            else
                _lastMessage = CommonHelper.GetFormattedMessage(message.Body);

            _time = message.Date;
            IsOut = message.IsOut;
            _dateTime = CommonHelper.GetFormattedDialogTime(_time);

            if (isConference)
            {
                _title = message.Title;
                _isOnline = false;
            }
            else
            {
                UserInfo user = users.FirstOrDefault(x => x.Uid == message.Uid);

                if (user != null)
                {
                    _title = user.FullName;
                    _isOnline = user.IsOnline;
                    _photoUri = user.Photo;
                }
            }

            _chatActive = message.ChatActive;
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

        [DataMember(Name = "AttachmentCount")]
        public int AttachmentCount
        {
            get
            {
                return _attachmentCount;
            }
            set
            {
                if (_attachmentCount != value)
                {
                    _attachmentCount = value;
                    OnPropertyChanged("AttachmentCount");
                }
            }
        }

        [DataMember(Name = "LastMessage")]
        public string LastMessage
        {
            get
            {
                return _lastMessage;
            }
            set
            {
                if (_lastMessage != value)
                {
                    _lastMessage = value;
                    OnPropertyChanged("LastMessage");
                }
            }
        }

        [DataMember(Name = "ChatActive")]
        public string ChatActive
        {
            get
            {
                return _chatActive;
            }
            set
            {
                if (_chatActive != value)
                {
                    _chatActive = value;
                    OnPropertyChanged("ChatActive");
                }
            }
        }

        [DataMember(Name = "ChatId")]
        public int ChatId
        {
            get
            {
                return _chatid;
            }
            set
            {
                if (_chatid != value)
                {
                    _chatid = value;
                    OnPropertyChanged("ChatId");
                }
            }
        }

        [DataMember(Name = "Mid")]
        public int Mid
        {
            get
            {
                return _mid;
            }
            set
            {
                if (_mid != value)
                {
                    _mid = value;
                    OnPropertyChanged("Mid");
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

        [DataMember(Name = "Title")]
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged("Title");
                }
            }
        }

        [DataMember(Name = "IsRead")]
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
                    _isRead = value;
                    OnPropertyChanged("IsRead");
                    OnPropertyChanged("LastMessageColor");
                }
            }
        }

        [DataMember(Name = "Time")]
        public int Time
        {
            get
            {
                return _time;
            }
            set
            {
                if (_time != value)
                {
                    _time = value;
                    OnPropertyChanged("Time");
                }
            }
        }

        [DataMember(Name = "DateTimeUI")]
        public string DateTimeUI
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
                    OnPropertyChanged("DateTimeUI");
                }
            }
        }

        [DataMember(Name = "IsConference")]
        public bool IsConference
        {
            get
            {
                return _isConference;
            }
            set
            {
                if (_isConference != value)
                {
                    _isConference = value;
                    OnPropertyChanged("IsConference");
                }
            }
        }

        [DataMember(Name = "IsOnline")]
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

        [DataMember(Name = "IsOut")]
        public bool IsOut
        {
            get
            {
                return _isLastMessageOut;
            }
            set
            {
                if (_isLastMessageOut != value)
                {
                    _isLastMessageOut = value;
                    OnPropertyChanged("IsOut");
                    OnPropertyChanged("IsOnlineFlagVisibility");
                }
            }
        }

        //[DataMember(Name = "PhotoUri")]
        //public string PhotoUri
        //{
        //    get
        //    {
        //        return _photoUri;
        //    }
        //    set
        //    {
        //        if (_photoUri != value)
        //        {
        //            _photoUri = value;
        //            OnPropertyChanged("PhotoUri");
        //        }
        //    }
        //}

        #endregion

        #region Public properties for only UI purposes

        public SolidColorBrush LastMessageColor
        {
            get
            {
                if (_isRead || _isLastMessageOut)
                    return App.Current.GrayBrush;
                else
                    return App.Current.BlueBrush;
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

        public BitmapImage Photo
        {
            get
            {
                if (_photo == null)
                    return App.Current.EntityService.DefaultAvatar;

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

        #region Public methods

        public IList<int> GetLastUserIds()
        {
            return CommonHelper.SplitIntegersByComma(_chatActive);
        }

        #endregion

        #region Private constants

        private const int LAST_MESSAGE_UI_LENGTH = 30;

        private const string LAST_MESSAGE_UI_FINISH = "...";

        #endregion

        #region Private fields

        private BitmapImage _photo;
        private int _attachmentCount;
        private bool _isConference;
        private string _lastMessage;
        private string _chatActive;
        private int _chatid;
        private int _mid;
        private int _uid;
        private string _title;
        private bool _isRead;
        private string _dateTime;
        private int _time;
        private bool _isOnline; // work only for dialog (not chat).
        private bool _isLastMessageOut;
        private string _photoUri;

        #endregion
    }
}
