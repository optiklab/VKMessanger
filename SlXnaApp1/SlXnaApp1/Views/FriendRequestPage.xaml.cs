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
using System.Diagnostics;
using System.Windows.Navigation;
using SlXnaApp1.Entities;
using SlXnaApp1.Api;

namespace SlXnaApp1.Views
{
    public partial class FriendRequestPage : PhoneApplicationPage
    {
        public FriendRequestPage()
        {
            InitializeComponent();
        }

        #region Event handlers

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string id = string.Empty;

            try
            {
                if (NavigationContext.QueryString.TryGetValue("id", out id))
                {
                    _id = Convert.ToInt32(id);

                    // From friend requests.
                    var friend = App.Current.EntityService.FriendsRequests.FirstOrDefault(x => x.Uid == _id);

                    // From mutual friends.
                    if (friend == null)
                    {
                        _isMutual = true;
                        friend = App.Current.EntityService.FriendsMutual.FirstOrDefault(x => x.Uid == _id);
                        DeleteButton.IsEnabled = false;
                    }

                    if (friend != null)
                    {
                        _friend = friend;
                        this.DataContext = (object)_friend;
                    }
                }
                else
                {
                    // From global search page.
                    if (App.Current.EntityService.FoundGlobalUser != null)
                    {
                        _friend = App.Current.EntityService.FoundGlobalUser;
                        _id = _friend.Uid;
                        this.DataContext = (object)_friend;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("OnNavigatedTo in FriendRequestPage failed: " + ex.Message);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        private void AddButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var op = new FriendsAdd(_id, string.Empty, result =>
            {
                int res = result;

                Debug.WriteLine("FriendsAdd" + _id.ToString() + " " + res.ToString());

                Dispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        if (_isMutual)
                        {
                            var friend = App.Current.EntityService.FriendsMutual.FirstOrDefault(x => x.Uid == _id);

                            if (friend != null)
                                App.Current.EntityService.FriendsMutual.Remove(friend);
                        }
                        else
                        {
                            var friend = App.Current.EntityService.FriendsRequests.FirstOrDefault(x => x.Uid == _id);

                            if (friend != null)
                            {
                                App.Current.EntityService.FriendsRequests.Remove(friend);
                                App.Current.EntityService.StateCounter.CountOfRequests = App.Current.EntityService.FriendsRequests.Count;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Removing friend request from FriendsRequests failed: " + ex.Message);
                    }

                    App.Current.EntityService.UpdateFriends();

                    NavigationService.Navigate(new Uri(@"/Views/FriendsPage.xaml", UriKind.Relative));
                });
            });
            op.Execute();
        }

        private void DeleteButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var op = new FriendsDelete(_id,result =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        if (_isMutual)
                        {
                            var friend = App.Current.EntityService.FriendsMutual.FirstOrDefault(x => x.Uid == _id);

                            if (friend != null)
                                App.Current.EntityService.FriendsMutual.Remove(friend);
                        }
                        else
                        {
                            var friend = App.Current.EntityService.FriendsRequests.FirstOrDefault(x => x.Uid == _id);

                            if (friend != null)
                            {
                                App.Current.EntityService.FriendsRequests.Remove(friend);
                                App.Current.EntityService.StateCounter.CountOfRequests = App.Current.EntityService.FriendsRequests.Count;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("DeleteButton_Tap failed: " + ex.Message);
                    }

                    NavigationService.Navigate(new Uri(@"/Views/FriendsPage.xaml", UriKind.Relative));
                });
            });
            op.Execute();
        }

        #endregion

        #region Private fields

        private int _id;
        private FriendViewModel _friend;
        private bool _isMutual = false;

        #endregion
    }
}