using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Microsoft.Phone.Shell;
using SlXnaApp1.Entities;
using System.Linq;
using System.Collections.Generic;

namespace SlXnaApp1.Views
{
    public partial class FriendsPage : Microsoft.Phone.Controls.PhoneApplicationPage
    {
        #region Constructor

        public FriendsPage()
        {
            InitializeComponent();

            _CreateBar();

            _categorizedModel = new FriendByFirstName(App.Current.EntityService.Friends.ToList());
            friends.ItemsSource = _categorizedModel;

            FriendRequestsNumberTextBlock.Text = App.Current.EntityService.StateCounter.CountOfRequests.ToString();
            MessagesNumberTextBlock.Text = App.Current.EntityService.StateCounter.UnreadMids.Count.ToString();
            App.Current.EntityService.StateCounter.PropertyChanged += StateCounter_PropertyChanged;
            App.Current.EntityService.StateCounter.UnreadMids.CollectionChanged += UnreadMids_CollectionChanged;
        }

        #endregion

        #region Event handlers

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _UpdateNotificationButtons();

            //_ClearBackHistory();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            //App.Current.EntityService.StateCounter.PropertyChanged -= StateCounter_PropertyChanged;
            //App.Current.EntityService.StateCounter.UnreadMids.CollectionChanged -= UnreadMids_CollectionChanged;
        }

        void UnreadMids_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _UpdateNotificationButtons();
        }

        void StateCounter_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _UpdateNotificationButtons();
        }

        private void onSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FriendViewModel friend = friends.SelectedItem as FriendViewModel;

            if (friend != null)
            {
                string id = friend.Uid.ToString();

                NavigationService.Navigate(new Uri(@"/Views/ChatPage.xaml?id=" + id, UriKind.Relative));

                friends.SelectedItem = null;
            }
        }

        private void ContactsPageTitle_Tap(object sender, GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/ContactsPage.xaml", UriKind.Relative));
        }

        private void DialogsPageTitle_Tap(object sender, GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/DialogsPage.xaml", UriKind.Relative));
        }

        private void Refresh_Click(object sender, EventArgs e)
        {
            try
            {
                App.Current.EntityService.UpdateFriends();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Refresh_Click failed:" + ex.Message);
            }
        }

        private void Search_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri(@"/Views/SearchPage.xaml", UriKind.Relative));
        }

        private void ShowUnreadMessages_Tap(object sender, GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/DialogsPage.xaml", UriKind.Relative));
        }

        private void ShowRequests_Tap(object sender, GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/FriendRequestsPage.xaml", UriKind.Relative));
        }

        private void Image_Tap(object sender, GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri(@"/Views/SettingsPage.xaml", UriKind.Relative));
        }

        #endregion

        #region Private methods

        private void _UpdateNotificationButtons()
        {
            FriendRequestsNumberTextBlock.Text = App.Current.EntityService.StateCounter.CountOfRequests.ToString();
            MessagesNumberTextBlock.Text = App.Current.EntityService.StateCounter.UnreadMids.Count.ToString();

            if (App.Current.EntityService.StateCounter.UnreadMids.Count == 0)
                UnreadMessagesButton.Visibility = System.Windows.Visibility.Collapsed;
            else
                UnreadMessagesButton.Visibility = System.Windows.Visibility.Visible;
        }

        public void _CreateBar()
        {
            this.ApplicationBar = new ApplicationBar();

            var appBarButton = new ApplicationBarIconButton(
                new Uri("/Images/Appbar_Icons/appbar.feature.search.rest.png", UriKind.Relative));
            appBarButton.Text = AppResources.SearchButton;
            this.ApplicationBar.Buttons.Add(appBarButton);
            appBarButton.Click += Search_Click;

            var appBarButton1 = new ApplicationBarIconButton(
                new Uri("/Images/Appbar_Icons/appbar.refresh.rest.png", UriKind.Relative));
            appBarButton1.Text = AppResources.RefreshButton;
            this.ApplicationBar.Buttons.Add(appBarButton1);
            appBarButton1.Click += Refresh_Click;
        }

        #endregion

        #region Private constants

        #endregion

        #region Private fields

        private FriendByFirstName _categorizedModel;

        #endregion
    }
}