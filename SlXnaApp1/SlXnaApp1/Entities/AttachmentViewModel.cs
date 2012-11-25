using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using SlXnaApp1.Infrastructure;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Threading;
using System.Windows.Threading;
using System.Diagnostics;

namespace SlXnaApp1.Entities
{
    public class AttachmentViewModel : INotifyPropertyChanged
    {
        #region Constructor

        public AttachmentViewModel(int mid, int owner_id, int a_id, AttachmentType type, string uri, string description,
            BitmapImage photo, IList<MessageViewModel> fwdMessages, Action handler)
        {
            OwnerId = owner_id;
            Aid = a_id;
            Mid = mid;
            _uri = uri;
            _description = description;
            _photo = photo;
            _messages = fwdMessages;
            _handler = handler;
            Type = type;
        }

        public AttachmentViewModel(int mid, int owner_id, int a_id, AttachmentType type, string uri, string description,
            BitmapImage photo, IList<MessageViewModel> fwdMessages, Action handler, string title, string author, int duration)
            : this(mid, owner_id, a_id, type, uri, description, photo, fwdMessages, handler)
        {
            _title = title;
            _author = author;

            TimeSpan ts = new TimeSpan(0, 0, duration);

            if (ts.Hours >= 1)
                Duration = String.Format("{0:HH:mm:ss}", ts);
            else
                Duration = String.Format("{0:MM:ss}", ts);
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

        #region Public methods

        public void ExecuteContent()
        {
            if (_handler != null)
                _handler();
        }

        #endregion

        #region Public properties

        // Simple info property
        public AttachmentType Type
        {
            get;
            set;
        }

        public int Mid
        {
            get;
            private set;
        }

        public int Aid
        {
            get;
            private set;
        }

        public int OwnerId
        {
            get;
            private set;
        }

        public Visibility MapPinVisibility
        {
            get
            {
                if (Type == AttachmentType.Location)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        public Visibility VideoPlayerVisibility
        {
            get
            {
                if (Type == AttachmentType.Video)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        public Visibility AudioPlayerVisibility
        {
            get
            {
                if (Type == AttachmentType.Audio)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        public Visibility PhotoVisibility
        {
            get
            {
                if (Type == AttachmentType.Audio || Type == AttachmentType.Document)
                    return Visibility.Collapsed;
                else
                    return Visibility.Visible;
            }
        }

        public Visibility DocumentVisibility
        {
            get
            {
                if (Type == AttachmentType.Document)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        public Visibility FwdVisibility
        {
            get
            {
                if (Type == AttachmentType.ForwardMessage)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        public BitmapImage AttachPhoto
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
                    OnPropertyChanged("AttachPhoto");
                }
            }
        }

        public string Duration
        {
            get;
            private set;
        }

        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged("Description");
                }
            }
        }

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

        public string Author
        {
            get
            {
                return _author;
            }
            set
            {
                if (_author != value)
                {
                    _author = value;
                    OnPropertyChanged("Author");
                }
            }
        }

        public string Uri
        {
            get
            {
                return _uri;
            }
            set
            {
                if (_uri != value)
                {
                    _uri = value;
                    OnPropertyChanged("Uri");
                }
            }
        }

        public IList<MessageViewModel> FwdMessages
        {
            get
            {
                return _messages;
            }
        }

        public double Progress
        {
            get
            {
                return _progress;
            }
            set
            {
                if (_progress != value)
                {
                    _progress = value;
                    OnPropertyChanged("Progress");
                }
            }
        }

        public double LoadingProgress
        {
            get
            {
                return _loadingProgress;
            }
            set
            {
                if (_loadingProgress != value)
                {
                    _loadingProgress = value;
                    OnPropertyChanged("LoadingProgress");
                }
            }
        }

        #endregion

        #region Private fields

        private string _uri;
        private string _description;
        private BitmapImage _photo;
        private IList<MessageViewModel> _messages;
        private Action _handler;
        private string _author;
        private string _title;
        private double _loadingProgress;
        private double _progress;

        #endregion
    }
}
