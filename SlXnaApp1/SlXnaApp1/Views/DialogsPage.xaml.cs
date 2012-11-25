using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Microsoft.Phone.Shell;
using SlXnaApp1.Infrastructure;
using SlXnaApp1.Json;

namespace SlXnaApp1.Views
{
    public partial class DialogsPage : Microsoft.Phone.Controls.PhoneApplicationPage
    {
        #region Constructor

        public DialogsPage()
        {
            InitializeComponent();

            _CreateBar();

            DialogsPanel.ItemsSource = App.Current.EntityService.Dialogs;

            _UpdateNotificationButtons();
            App.Current.EntityService.StateCounter.PropertyChanged += StateCounter_PropertyChanged;
            App.Current.EntityService.StateCounter.UnreadMids.CollectionChanged += UnreadMids_CollectionChanged;
        }

        #endregion

        #region Event handlers

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _UpdateNotificationButtons();

            _ClearBackHistory();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            App.Current.EntityService.CloseCurrentChat();

            //App.Current.EntityService.StateCounter.PropertyChanged -= StateCounter_PropertyChanged;
            //App.Current.EntityService.StateCounter.UnreadMids.CollectionChanged -= UnreadMids_CollectionChanged;
        }

        void StateCounter_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _UpdateNotificationButtons();
        }

        void UnreadMids_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _UpdateNotificationButtons();
        }

        private void onSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                try
                {
                    string id;

                    Dialog dialog = (Dialog)e.AddedItems[0];// DialogsPanel.SelectedValue;

                    // Reset selection of ListBox 
                    ((ListBox)sender).SelectedIndex = -1;

                    if (dialog.IsConference)
                        id = dialog.ChatId.ToString(); // Id is chatId in this case, not a message id
                    else
                        id = dialog.Uid.ToString();

                    if (dialog.IsConference)
                        NavigationService.Navigate(new Uri(@"/Views/GroupChatPage.xaml?id=" + id, UriKind.Relative));
                    else
                        NavigationService.Navigate(new Uri(@"/Views/ChatPage.xaml?id=" + id, UriKind.Relative));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("DialogsPage onSelectionChanged failed" + ex.Message);
                }
            }
        }

        private void FriendsPageTitle_Tap(object sender, GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/FriendsPage.xaml", UriKind.Relative));
        }

        private void ContactsPageTitle_Tap(object sender, GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/ContactsPage.xaml", UriKind.Relative));
        }

        private void ShowRequests_Tap(object sender, GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/FriendRequestsPage.xaml", UriKind.Relative));
        }

        private void DialogsPanel_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            // Is last item visible?
            int index = DialogsPanel.Items.Count - 1;

            if (WindowsPhoneHelpers.IsItemVisible(DialogsPanel, index))
                App.Current.EntityService.LoadMoreDialogs();
        }

        private void CreateChat_Click(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                NavigationService.Navigate(new Uri("/Views/AddChatPage.xaml", UriKind.Relative));
            });
        }

        private void RefreshDialogs_Click(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    App.Current.EntityService.UpdateDialogs();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("RefreshDialogs_Click failed:" + ex.Message);
                }
            });
        }

        private void SearchDialogs_Click(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    NavigationService.Navigate(new Uri("/Views/SearchDialogsPage.xaml", UriKind.Relative));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("SearchDialogs_Click failed:" + ex.Message);
                }
            });
        }

        private void Settings_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri(@"/Views/SettingsPage.xaml", UriKind.Relative));
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
                new Uri("/Images/Appbar_Icons/appbar.add.rest.png", UriKind.Relative));
            appBarButton.Text = AppResources.AddDialog;
            this.ApplicationBar.Buttons.Add(appBarButton);
            appBarButton.Click += CreateChat_Click;

            var appBarButton1 = new ApplicationBarIconButton(
                new Uri("/Images/Appbar_Icons/appbar.refresh.rest.png", UriKind.Relative));
            appBarButton1.Text = AppResources.RefreshButton;
            this.ApplicationBar.Buttons.Add(appBarButton1);
            appBarButton1.Click += RefreshDialogs_Click;

            var appBarButton2 = new ApplicationBarIconButton(
                new Uri("/Images/Appbar_Icons/appbar.feature.search.rest.png", UriKind.Relative));
            appBarButton2.Text = AppResources.SearchButton;
            this.ApplicationBar.Buttons.Add(appBarButton2);
            appBarButton2.Click += SearchDialogs_Click;

            var appBarMenuItem = new ApplicationBarMenuItem(AppResources.Settings);
            this.ApplicationBar.MenuItems.Add(appBarMenuItem);
            appBarMenuItem.Click += Settings_Click;
        }

        private void _ClearBackHistory()
        {
            try
            {
                // Clear back stack history.
                while (this.NavigationService.BackStack.Any())
                {
                    this.NavigationService.RemoveBackEntry();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("_ClearBackHistory failed: " + ex.Message);
            }
        }

        #endregion

        #region Private constants

        #endregion

        #region Private fields

        #endregion
    }
}