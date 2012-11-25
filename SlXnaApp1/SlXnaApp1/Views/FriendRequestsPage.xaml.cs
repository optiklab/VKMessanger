using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Phone.Shell;
using SlXnaApp1.Entities;
using SlXnaApp1.Infrastructure;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using SlXnaApp1.Cache;

namespace SlXnaApp1.Views
{
    public partial class FriendRequestsPage : Microsoft.Phone.Controls.PhoneApplicationPage
    {
        #region Constructor

        public FriendRequestsPage()
        {
            InitializeComponent();

            _photosToLoad = new Dictionary<FriendViewModel, AvatarLoadItem>();
            _imageCache = new ImageCache();

            _CreateBar();

            FriendRequestsPanel.ItemsSource = App.Current.EntityService.FriendsRequests;
            App.Current.EntityService.FriendsRequests.CollectionChanged += FriendsRequests_CollectionChanged;
            PossibleFriendsPanel.ItemsSource = App.Current.EntityService.FriendsMutual;

            if (App.Current.EntityService.FriendsRequests.Count == 0 ||
                App.Current.EntityService.FriendsRequests.Count != App.Current.EntityService.StateCounter.CountOfRequests)
            {
                GlobalIndicator.Instance.Text = AppResources.SearchFriendRequests;
                GlobalIndicator.Instance.IsLoading = true;

                TitlePanel.Text = string.Format(AppResources.FriendRequests, 0);

                App.Current.EntityService.GetFriendRequests(_LoadingFriendsInfoFinished);
            }
            else
            {
                TitlePanel.Text = string.Format(AppResources.FriendRequests,
                    App.Current.EntityService.FriendsRequests.Count);

                if (App.Current.EntityService.FriendsMutual.Any())
                    PeopleMayKnowLabel.Visibility = System.Windows.Visibility.Visible;
                else
                    PeopleMayKnowLabel.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        #endregion

        #region Event handlers

        private void onSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var friend = FriendRequestsPanel.SelectedItem as FriendViewModel;

            if (friend != null)
            {
                NavigationService.Navigate(new Uri(@"/Views/FriendRequestPage.xaml?id=" + friend.Uid, UriKind.Relative));

                FriendRequestsPanel.SelectedItem = null;
            }
            else
            {
                friend = PossibleFriendsPanel.SelectedItem as FriendViewModel;

                if (friend != null)
                    NavigationService.Navigate(new Uri(@"/Views/FriendRequestPage.xaml?id=" + friend.Uid, UriKind.Relative));

                PossibleFriendsPanel.SelectedItem = null;
            }
        }

        private void FriendsRequests_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            TitlePanel.Text = string.Format(AppResources.FriendRequests, App.Current.EntityService.FriendsRequests.Count);
        }

        private void Search_Click(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    NavigationService.Navigate(new Uri("/Views/GlobalSearchPage.xaml", UriKind.Relative));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Search_Click on FriendRequestsPage failed:" + ex.Message);
                }
            });
        }

        #endregion

        #region Private methods

        private void _LoadingFriendsInfoFinished()
        {
            Dispatcher.BeginInvoke(() =>
            {
                GlobalIndicator.Instance.Text = string.Empty;
                GlobalIndicator.Instance.IsLoading = false;
            });

            try
            {
                _photosToLoad.Clear();

                // Get uri's to load photos
                foreach (var request in App.Current.EntityService.FriendsRequests)
                {
                    // Try to load image from cache.
                    string filename = CommonHelper.DoDigest(request.PhotoUri);
                    BitmapImage image = _imageCache.GetItem(filename);
                    if (image != null)
                        request.Photo = image;
                    else // ...if it doesn't exists - load from web.
                        _photosToLoad.Add(request, new AvatarLoadItem(_photosToLoad.Count, request.PhotoUri, filename));
                }

                Dispatcher.BeginInvoke(() =>
                {
                    if (App.Current.EntityService.FriendsMutual.Any())
                        PeopleMayKnowLabel.Visibility = System.Windows.Visibility.Visible;
                    else
                        PeopleMayKnowLabel.Visibility = System.Windows.Visibility.Collapsed;
                });

                foreach (var request in App.Current.EntityService.FriendsMutual)
                {
                    // Try to load image from cache.
                    string filename = CommonHelper.DoDigest(request.PhotoUri);
                    BitmapImage image = _imageCache.GetItem(filename);
                    if (image != null)
                        request.Photo = image;
                    else // ...if it doesn't exists - load from web.
                        _photosToLoad.Add(request, new AvatarLoadItem(_photosToLoad.Count, request.PhotoUri, filename));
                }

                _LoadPhotos();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("LoadingFriendsInfoFinished failed:" + ex.Message);
            }
        }

        public void _CreateBar()
        {
            this.ApplicationBar = new ApplicationBar();

            var appBarButton = new ApplicationBarIconButton(
                new Uri("/Images/Appbar_Icons/appbar.feature.search.rest.png", UriKind.Relative));
            appBarButton.Text = AppResources.SearchButton;
            this.ApplicationBar.Buttons.Add(appBarButton);
            appBarButton.Click += Search_Click;
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

                    BitmapImage image = _imageCache.GetItem(loadItem.Value.FileName);

                    if (image != null && image.PixelHeight > 0 && image.PixelWidth > 0)
                    {
                        loadItem.Key.Photo = image;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("_UpdatePhoto failed in FriendrequestsPage: " + ex.Message);
                }
            });
        }

        #endregion

        #region Private constants

        #endregion

        #region Private fields

        private ImageCache _imageCache;
        private IDictionary<FriendViewModel, AvatarLoadItem> _photosToLoad;

        #endregion
    }
}