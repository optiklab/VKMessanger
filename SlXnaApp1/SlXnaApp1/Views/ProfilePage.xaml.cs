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
using Microsoft.Phone.Shell;
using System.Windows.Navigation;
using SlXnaApp1.Entities;
using System.Diagnostics;
using Microsoft.Phone.Tasks;
using SlXnaApp1.Api;

namespace SlXnaApp1.Views
{
    public partial class ProfilePage : PhoneApplicationPage
    {
        #region Constructor

        public ProfilePage()
        {
            InitializeComponent();

            _CreateBar();
        }

        #endregion

        #region Protected methods

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string synced = string.Empty;

            try
            {
                if (NavigationContext.QueryString.TryGetValue("synced", out synced))
                {
                    int isSynced = Convert.ToInt32(synced);

                    if (isSynced == 1)
                    {
                        TitlePanel1.Visibility = System.Windows.Visibility.Visible;
                        TitlePanel2.Visibility = System.Windows.Visibility.Collapsed;
                    }
                    else
                    {
                        TitlePanel2.Visibility = System.Windows.Visibility.Visible;
                        TitlePanel1.Visibility = System.Windows.Visibility.Collapsed;
                    }
                }

                _model = App.Current.EntityService.CurrentPhoneContact;

                if (_model != null)
                    this.DataContext = (object)_model;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("OnNavigatedTo in ProfilePage failed: " + ex.Message);
            }
        }

        private void SendMessageButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri(@"/Views/ChatPage.xaml?id=" + _model.Uid, UriKind.Relative));
        }

        private void CallButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!string.IsNullOrEmpty(_model.VerifiedPhone))
            {
                var callTask = new PhoneCallTask();

                callTask.PhoneNumber = _model.VerifiedPhone;
                callTask.DisplayName = _model.ContactName;

                callTask.Show();
            }
        }

        private void SendInvitationButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!string.IsNullOrEmpty(_model.VerifiedPhone))
            {
                var composeSMS = new SmsComposeTask();

                composeSMS.To = _model.VerifiedPhone;

                composeSMS.Show();
            }
        }

        private void Remove_Click(object sender, EventArgs e)
        {
            if (_model.Uid > 0)
            {
                var op = new FriendsDelete(_model.Uid, result =>
                {
                    App.Current.EntityService.DeleteFriend(_model.Uid);

                    NavigationService.Navigate(new Uri(@"/Views/FriendsPage.xaml", UriKind.Relative));
                });
                op.Execute();
            }
        }

        #endregion

        #region Private methods

        public void _CreateBar()
        {
            this.ApplicationBar = new ApplicationBar();

            var appBarMenuItem = new ApplicationBarMenuItem(AppResources.DeleteFriendButton);
            this.ApplicationBar.MenuItems.Add(appBarMenuItem);
            appBarMenuItem.Click += Remove_Click;
        }

        #endregion

        #region Private fields

        private PhoneContact _model;

        #endregion
    }
}