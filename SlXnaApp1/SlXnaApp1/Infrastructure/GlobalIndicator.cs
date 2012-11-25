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
using Microsoft.Phone.Shell;
using System.ComponentModel;
using Microsoft.Phone.Controls;
using System.Windows.Navigation;

namespace SlXnaApp1.Infrastructure
{
    public class GlobalIndicator : INotifyPropertyChanged
    {
        private ProgressIndicator _mangoIndicator;
 
        public void Initialize(PhoneApplicationFrame frame)
        {
            // If using AgFx:
            // DataManager.Current.PropertyChanged += OnDataManagerPropertyChanged;
 
            _mangoIndicator = new ProgressIndicator();
 
            frame.Navigated += OnRootFrameNavigated;
        }
 
        private void OnRootFrameNavigated(object sender, NavigationEventArgs e)
        {
            // Use in Mango to share a single progress indicator instance.
            var ee = e.Content;
            var pp = ee as PhoneApplicationPage;
            if (pp != null)
            {
                pp.SetValue(SystemTray.ProgressIndicatorProperty, _mangoIndicator);
            }
        }
 
        private void OnDataManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ("IsLoading" == e.PropertyName)
            {
                // if AgFx: IsDataManagerLoading = DataManager.Current.IsLoading;
                NotifyValueChanged();
            }
        }

        private static GlobalIndicator _in;
        public static GlobalIndicator Instance
        {
            get
            {
                if (_in == null)
                {
                    _in = new GlobalIndicator();
                }
 
                return _in;
            }
        }
 
        public bool IsDataManagerLoading { get; set; }
 
        public bool ActualIsLoading
        {
            get
            {
                return IsLoading || IsDataManagerLoading;
            }
        }
 
        private int _loadingCount;
 
        public bool IsLoading
        {
            get
            {
                return _loadingCount > 0;
            }
            set
            {
                bool loading = IsLoading;
                if (value)
                {
                    ++_loadingCount;
                }
                else
                {
                    --_loadingCount;
                }
 
                NotifyValueChanged();
            }
        }

        public string Text
        {
            get;
            set;
        }
 
        private void NotifyValueChanged()
        {
            if (_mangoIndicator != null)
            {
                _mangoIndicator.IsIndeterminate = _loadingCount > 0 || IsDataManagerLoading;

                _mangoIndicator.Text = Text;

                // for now, just make sure it's always visible.
                if (_mangoIndicator.IsVisible == false)
                {
                    _mangoIndicator.IsVisible = true;
                }
            }
        }
 
        public event PropertyChangedEventHandler PropertyChanged;
 
        protected virtual void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
