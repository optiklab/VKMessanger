using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Collections.ObjectModel;
using SlXnaApp1.Entities;
using System.Diagnostics;
using System.Windows.Navigation;
using System.Threading;
using SlXnaApp1.Api;
using SlXnaApp1.Cache;
using SlXnaApp1.Infrastructure;
using System.Windows.Media.Imaging;

namespace SlXnaApp1.Views
{
    public partial class GlobalSearchPage : PhoneApplicationPage
    {
        #region Constructor

        public GlobalSearchPage()
        {
            InitializeComponent();

            _results = new ObservableCollection<FriendViewModel>();
            _photosToLoad = new Dictionary<FriendViewModel, AvatarLoadItem>();
            _imageCache = new ImageCache();

            FriendsPanel.ItemsSource = _results;
        }

        #endregion

        #region Event handlers

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_userTypingTimer != null)
            {
                _userTypingTimer.Dispose();
                _userTypingTimer = null;
            }

            if (!string.IsNullOrEmpty(SearchTextBox.Text))
            {
                _workQ = SearchTextBox.Text;

                _userTypingTimer = new Timer(state =>
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        // Start search after 1 second of silence.
                        var op = new UsersSearch(_workQ, results =>
                        {
                            Dispatcher.BeginInvoke(() =>
                            {
                                try
                                {
                                    _results.Clear();
                                    _photosToLoad.Clear();

                                    foreach (var result in results)
                                    {
                                        try
                                        {
                                            var model = new FriendViewModel(result.Uid,
                                                result.FullName, string.Empty, result.FirstName, result.LastName,
                                                result.IsOnline, -1, result.PhotoBig, App.Current.EntityService.DefaultAvatar);
                                            _results.Add(model);

                                            // Try to load image from cache.
                                            string filename = CommonHelper.DoDigest(result.PhotoBig);
                                            BitmapImage image = _imageCache.GetItem(filename);
                                            if (image != null)
                                                model.Photo = image;
                                            else // ...if it doesn't exists - load from web.
                                                _photosToLoad.Add(model, new AvatarLoadItem(_photosToLoad.Count, result.PhotoBig, filename));
                                        }
                                        catch (Exception ex)
                                        {
                                            Debug.WriteLine("UsersSearch model creating failed: " + ex.Message);
                                            // If some fields is empty =).
                                        }
                                    }

                                    if (results.Count > 0)
                                        SearchDescription.Visibility = System.Windows.Visibility.Collapsed;
                                    else
                                        SearchDescription.Visibility = System.Windows.Visibility.Visible;

                                    _LoadPhotos();
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine("UsersSearch failed: " + ex.Message);
                                    // If some inter-process error
                                }
                            });
                        });
                        op.Execute();
                    });
                }, null, 1000, -1);
            }
            else
            {
                SearchDescription.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void onSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var friend = FriendsPanel.SelectedItem as FriendViewModel;

            if (friend != null)
            {
                App.Current.EntityService.FoundGlobalUser = friend;
                NavigationService.Navigate(new Uri(@"/Views/FriendRequestPage.xaml", UriKind.Relative));

                FriendsPanel.SelectedItem = null;
            }
        }

        private void _LoadPhotos()
        {
            try
            {
                var service = new AsyncAvatarsLoader();

                // From map of attachment view model and load items get collection of avatar load items...
                var photosToLoad = _photosToLoad.Select(x => x.Value);

                if (photosToLoad.Any())
                    service.LoadAvatars(photosToLoad.ToList(), _UpdatePhoto, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("LoadPhotos failed " + ex.Message);
            }
        }

        private void _UpdatePhoto(int id)
        {
            Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    var loadItem = _photosToLoad.FirstOrDefault(x => x.Value.Uid == id); // We can use id just like [id]...but this is really bad =)

                    if (loadItem.Key != null && loadItem.Value != null)
                    {
                        BitmapImage image = _imageCache.GetItem(loadItem.Value.FileName);

                        if (image != null && image.PixelHeight > 0 && image.PixelWidth > 0)
                        {
                            loadItem.Key.Photo = image;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("_UpdatePhoto failed on GlobalSearchPage: " + ex.Message);
                }
            });
        }

        #endregion

        #region Private fields

        private Timer _userTypingTimer = null;
        private ObservableCollection<FriendViewModel> _results;
        private string _workQ;
        private ImageCache _imageCache;
        private IDictionary<FriendViewModel, AvatarLoadItem> _photosToLoad;

        #endregion
    }
}