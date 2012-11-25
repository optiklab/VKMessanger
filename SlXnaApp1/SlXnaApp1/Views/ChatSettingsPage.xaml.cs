using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using SlXnaApp1.Entities;
using SlXnaApp1.Json;
using System.Collections.Generic;
using System.Diagnostics;
using SlXnaApp1.Api;
using System.Windows.Navigation;
using System.Net;
using Microsoft.Phone.Shell;
using System.Windows;
using SlXnaApp1.Infrastructure;

namespace SlXnaApp1.Views
{
    public partial class ChatSettingsPage : Microsoft.Phone.Controls.PhoneApplicationPage
    {
        #region Constructor

        public ChatSettingsPage()
        {
            InitializeComponent();

            _CreateBar();
        }

        #endregion

        #region Event handlers

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string id = string.Empty;
            string name = string.Empty;

            try
            {
                if (NavigationContext.QueryString.TryGetValue("id", out id) &&
                    NavigationContext.QueryString.TryGetValue("name", out name))
                {
                    _currentId = Convert.ToInt32(id);
                    _currentName = HttpUtility.HtmlDecode(name);

                    NameTextBox.Text = _currentName;

                    // Get chat friends only
                    var op = new MessagesGetChatUsers(_currentId, friends =>
                    {
                        Dispatcher.BeginInvoke(() =>
                        {
                            try
                            {
                                // Update list of friends of this group chat.
                                var listedFriends = App.Current.EntityService.Friends.Where(x => friends.FirstOrDefault(y => y.Uid == x.Uid) != null);
                                var nonFriends = App.Current.EntityService.OtherUsers.Where(x => friends.FirstOrDefault(y => y.Uid == x.Uid) != null);

                                _friends.Clear();
                                foreach (var item in listedFriends)
                                    _friends.Add(item);

                                // Add non-friends info if it is.
                                foreach (var item in nonFriends)
                                {
                                    var user = App.Current.EntityService.OtherUsers.FirstOrDefault(x => x.Uid == item.Uid);
                                    _friends.Add(new FriendViewModel(item.Uid, item.FullName, string.Empty, item.FirstName, item.LastName,
                                        item.IsOnline, -1, item.Photo, user.ImagePhoto));
                                }

                                FriendsPanel.ItemsSource = _friends;
                                FriendsSelectionPanel.ItemsSource = _friends;
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("ChatSettingsPage -> OnNavigatedTo -> MessagesGetChatUsers callback failed: " + ex.Message);
                            }
                        });
                    });
                    op.Execute();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("OnNavigatedTo in ChatSettingsPage failed: " + ex.Message);
            }
        }

        private void NameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_currentName != NameTextBox.Text)
            {
                // Make save button enabled.
                var saveButton = this.ApplicationBar.Buttons[1] as ApplicationBarIconButton;

                if (saveButton != null)
                    saveButton.IsEnabled = true;
            }
            else
            {
                // Make save button disabled.
                var saveButton = this.ApplicationBar.Buttons[1] as ApplicationBarIconButton;

                if (saveButton != null)
                    saveButton.IsEnabled = false;
            }
        }

        private void AddPerson_Click(object sender, EventArgs e)
        {
            // Redesign current layout to let user select friend from the list.
            // NOTE. I don't use separate View to leave from BackStack navigation problems.
            NameTextBox.Visibility = System.Windows.Visibility.Collapsed;
            NameTextBoxLabel.Visibility = System.Windows.Visibility.Collapsed;
            SelectUsersTitle.Visibility = System.Windows.Visibility.Visible;

            FriendsSelectionPanel.Visibility = System.Windows.Visibility.Collapsed;
            FriendsPanel.Visibility = System.Windows.Visibility.Visible;

            FriendsPanel.ItemsSource = App.Current.EntityService.Friends;
            FriendsPanel.SelectionChanged += onSelectionChanged;

            this.ApplicationBar.IsVisible = false;
        }

        private void onSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FriendViewModel friend = FriendsPanel.SelectedItem as FriendViewModel;

            if (friend != null)
            {
                var op = new MessagesAddChatUser(_currentId, friend.Uid, isOk =>
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        if (!isOk)
                            MessageBox.Show(AppResources.CantAddThisUser);

                        try
                        {
                            // Redesign current layout back to normal view (from friend selection panel).
                            NameTextBox.Visibility = System.Windows.Visibility.Visible;
                            NameTextBoxLabel.Visibility = System.Windows.Visibility.Visible;
                            SelectUsersTitle.Visibility = System.Windows.Visibility.Collapsed;

                            FriendsPanel.ItemsSource = _friends;

                            FriendsPanel.SelectionChanged -= onSelectionChanged;

                            this.ApplicationBar.IsVisible = true;

                            _friends.Add(friend);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("onSelectionChanged failed in ChatSettingsPage: " + ex.Message);
                        }
                    });
                });
                op.Execute();
            }
        }

        private void SaveChanges_Click(object sender, EventArgs e)
        {
            GlobalIndicator.Instance.Text = AppResources.SavingChanges;
            GlobalIndicator.Instance.IsLoading = true;

            string title = NameTextBox.Text;

            var op = new MessagesEditChat(_currentId, title, isOk =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    if (isOk)
                    {
                        var dialog = App.Current.EntityService.Dialogs.FirstOrDefault(x => x.ChatId == _currentId && x.IsConference);
                        if (dialog != null)
                            dialog.Title = title;
                    }
                    else
                    {
                        MessageBox.Show(AppResources.CanNotSaveThisChange);
                    }

                    try
                    {
                        // Make save button disabled.
                        var saveButton = this.ApplicationBar.Buttons[1] as ApplicationBarIconButton;

                        if (saveButton != null)
                            saveButton.IsEnabled = false;
                    }
                    catch
                    { }

                    GlobalIndicator.Instance.Text = string.Empty;
                    GlobalIndicator.Instance.IsLoading = false;
                });
            });

            op.Execute();
        }

        private void Change_Click(object sender, EventArgs e)
        {
            FriendsSelectionPanel.Visibility = System.Windows.Visibility.Visible;
            FriendsPanel.Visibility = System.Windows.Visibility.Collapsed;

            _CreateBarForSelectedUsers();
        }

        private void LeaveChat_Click(object sender, EventArgs e)
        {
            try
            {
                var op = new MessagesRemoveChatUser(App.Current.EntityService.CurrentUser.UserInfo.Uid, _currentId, isOk =>
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        if (!isOk)
                        {
                            MessageBox.Show(AppResources.CantRemoveThisUser);
                        }
                        else
                        {
                            try
                            {
                                App.Current.EntityService.UpdateDialogs();
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("LeaveChat_Click->UpdateDialogs failed in ChatSettingsPage: " + ex.Message);
                            }

                            NavigationService.Navigate(new Uri(@"/Views/DialogsPage.xaml", UriKind.Relative));
                        }
                    });
                });
                op.Execute();


            }
            catch (Exception ex)
            {
                Debug.WriteLine("LeaveChat_Click failed:" + ex.Message);
            }
        }

        private void IsSelectedCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var fvm = (sender as CheckBox).DataContext as FriendViewModel;

            if (fvm != null)
            {
                fvm.IsSelected = true;
            }
        }

        private void IsSelectedCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var fvm = (sender as CheckBox).DataContext as FriendViewModel;

            if (fvm != null)
            {
                fvm.IsSelected = false;
            }
        }

        private void RemoveChatFriend_Click(object sender, EventArgs e)
        {
            var selected = _friends.Where(x => x.IsSelected);

            if (selected != null && selected.Any())
            {
                foreach (var user in selected)
                {
                    var op = new MessagesRemoveChatUser(user.Uid, _currentId, isOk =>
                    {
                        Dispatcher.BeginInvoke(() =>
                        {
                            if (isOk)
                            {
                                try
                                {
                                    _friends.Remove(user);
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine("RemoveChatFriend_Click failed:" + ex.Message + Environment.NewLine + ex.StackTrace);
                                }
                            }
                            else
                            {
                                MessageBox.Show(AppResources.CantRemoveThisUser);
                            }
                        });
                    });
                    op.Execute();
                }
            }
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            FriendsSelectionPanel.Visibility = System.Windows.Visibility.Collapsed;
            FriendsPanel.Visibility = System.Windows.Visibility.Visible;

            _CreateBar();
        }

        #endregion

        #region Private methods

        public void _CreateBar()
        {
            this.ApplicationBar = new ApplicationBar();

            var appBarButton = new ApplicationBarIconButton(
                new Uri("/Images/Appbar_Icons/appbar.add.rest.png", UriKind.Relative));
            appBarButton.Text = AppResources.AddPerson;
            this.ApplicationBar.Buttons.Add(appBarButton);
            appBarButton.Click += AddPerson_Click;

            var appBarButton1 = new ApplicationBarIconButton(
                new Uri("/Images/Appbar_Icons/appbar.save.rest.png", UriKind.Relative));
            appBarButton1.Text = AppResources.SaveChanges;
            appBarButton1.IsEnabled = false;
            this.ApplicationBar.Buttons.Add(appBarButton1);
            appBarButton1.Click += SaveChanges_Click;

            var appBarButton2 = new ApplicationBarIconButton(
                new Uri("/Images/Appbar_Icons/appbar.manage.rest.png", UriKind.Relative));
            appBarButton2.Text = AppResources.Change;
            this.ApplicationBar.Buttons.Add(appBarButton2);
            appBarButton2.Click += Change_Click;

            var appBarMenuItem = new ApplicationBarMenuItem(AppResources.LeaveChat);
            this.ApplicationBar.MenuItems.Add(appBarMenuItem);
            appBarMenuItem.Click += LeaveChat_Click;
        }

        public void _CreateBarForSelectedUsers()
        {
            this.ApplicationBar = new ApplicationBar();

            var appBarButton = new ApplicationBarIconButton(
                new Uri("/Images/Appbar_Icons/appbar.minus.rest.png", UriKind.Relative));
            appBarButton.Text = AppResources.RemovePerson;
            this.ApplicationBar.Buttons.Add(appBarButton);
            appBarButton.Click += RemoveChatFriend_Click;

            var appBarButton1 = new ApplicationBarIconButton(
                new Uri("/Images/Appbar_Icons/appbar.cancel.rest.png", UriKind.Relative));
            appBarButton1.Text = AppResources.CancelButton;
            this.ApplicationBar.Buttons.Add(appBarButton1);
            appBarButton1.Click += Cancel_Click;
        }

        #endregion

        #region Private constants

        #endregion

        #region Private fields

        private int _currentId;
        private string _currentName;
        private ObservableCollection<FriendViewModel> _friends = new ObservableCollection<FriendViewModel>();

        #endregion
    }
}