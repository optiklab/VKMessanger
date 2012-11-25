using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace SlXnaApp1.Entities
{
    public class StateCounter : INotifyPropertyChanged
    {
        public StateCounter(int requests, List<int> unread)
        {
            CountOfRequests = requests;

            UnreadMids = new ObservableCollection<int>();

            if (unread != null)
            {
                foreach (var id in unread)
                {
                    UnreadMids.Add(id);
                }
            }
        }

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

        public int CountOfRequests
        {
            get
            {
                return _countOfRequests;
            }
            set
            {
                if (_countOfRequests != value)
                {
                    _countOfRequests = value;
                    OnPropertyChanged("CountOfRequests");
                }
            }
        }

        public ObservableCollection<int> UnreadMids { get; set; }

        #endregion

        #region Private fields

        private int _countOfRequests;

        #endregion
    }
}
