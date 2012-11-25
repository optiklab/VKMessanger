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
using SlXnaApp1.Entities;
using System.Windows.Navigation;
using System.Diagnostics;
using SlXnaApp1.Api;
using Microsoft.Phone.Shell;
using System.Collections.ObjectModel;
using SlXnaApp1.Infrastructure;

namespace SlXnaApp1.Views
{
    public partial class AddChatPage : PhoneApplicationPage
    {
        #region Constructor

        public AddChatPage()
        {
            InitializeComponent();

            _CreateBar();

            _friends = App.Current.EntityService.Friends;
            FriendsSelectionPanel.ItemsSource = _friends;
        }

        #endregion

        #region Event handlers

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = true;
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

        private void Create_Click(object sender, EventArgs e)
        {
            var selected = _friends.Where(x => x.IsSelected);

            if (selected != null && selected.Any())
            {
                string uids = string.Empty;
                foreach (var user in selected)
                {
                    uids = uids + user.Uid.ToString() + ",";
                }

                uids = uids.TrimEnd('"');

                string text = NameTextBox.Text;

                if (!string.IsNullOrEmpty(uids) &&
                    !string.IsNullOrEmpty(text))
                {
                    GlobalIndicator.Instance.Text = AppResources.AddingChatPage;
                    GlobalIndicator.Instance.IsLoading = true;

                    ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = false;

                    var op = new MessagesCreateChat(uids, text, id =>
                    {
                        Dispatcher.BeginInvoke(() =>
                        {
                            try
                            {
                                if (id > -1)
                                {
                                    try
                                    {
                                        NavigationService.Navigate(new Uri(@"/Views/GroupChatPage.xaml?id=" + id + "&title=" + text, UriKind.Relative));
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.WriteLine("Create_Click failed:" + ex.Message + Environment.NewLine + ex.StackTrace);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show(AppResources.CantCreateChat);
                                    ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = true;
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("Create_Click failed in AddChatPage: " + ex.Message);
                            }

                            GlobalIndicator.Instance.Text = string.Empty;
                            GlobalIndicator.Instance.IsLoading = false;
                        });
                    });
                    op.Execute();
                }
                else
                {
                    MessageBox.Show(AppResources.CreateChatError);
                }
            }
            else
            {
                MessageBox.Show(AppResources.CreateChatError);
            }
        }

        #endregion

        #region Private methods

        public void _CreateBar()
        {
            this.ApplicationBar = new ApplicationBar();

            var appBarButton = new ApplicationBarIconButton(
                new Uri("/Images/Appbar_Icons/appbar.add.rest.png", UriKind.Relative));
            appBarButton.Text = AppResources.CreateChat;
            this.ApplicationBar.Buttons.Add(appBarButton);
            appBarButton.Click += Create_Click;
        }

        #endregion

        #region Private fields

        private ObservableCollection<FriendViewModel> _friends = new ObservableCollection<FriendViewModel>();

        #endregion
    }
}