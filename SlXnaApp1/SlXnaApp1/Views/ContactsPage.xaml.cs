using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Microsoft.Phone.Shell;
using Microsoft.Phone.UserData;
using SlXnaApp1.Api;
using SlXnaApp1.Cache;
using SlXnaApp1.Entities;
using SlXnaApp1.Json;
using SlXnaApp1.Infrastructure;

namespace SlXnaApp1.Views
{
    public partial class ContactsPage : Microsoft.Phone.Controls.PhoneApplicationPage
    {
        #region Constructor

        public ContactsPage()
        {
            InitializeComponent();

            _CreateBar();

            _contactsCache = new ContactsCache();

            _UpdateNotificationButtons();
            App.Current.EntityService.StateCounter.PropertyChanged += StateCounter_PropertyChanged;
            App.Current.EntityService.StateCounter.UnreadMids.CollectionChanged += UnreadMids_CollectionChanged;

            if (App.Current.IsFirstRun || App.Current.LastContactsSync == DateTime.MinValue) // Is first run or never synced
            {
                SyncContactsDescription.Visibility = Visibility.Visible;
                SyncContactsButton.Visibility = Visibility.Visible;

                InviteFriendButton.Visibility = Visibility.Collapsed;
                FriendsPanel.Visibility = Visibility.Collapsed;

                // Empty list.
            }
            else
            {
                // Build View Model.

                InviteFriendButton.Visibility = Visibility.Visible;
                FriendsPanel.Visibility = Visibility.Visible;

                SyncContactsDescription.Visibility = Visibility.Collapsed;
                SyncContactsButton.Visibility = Visibility.Collapsed;

                var contacts = _contactsCache.GetItems();

                App.Current.EntityService.Contacts.Clear();
                foreach (var contact in contacts)
                {
                    var friend = App.Current.EntityService.Friends.FirstOrDefault(x => x.Uid == contact.Uid);

                    if (friend != null)
                        contact.Photo = friend.Photo;
                    else
                    {
                        if (App.Current.EntityService.CurrentUser.UserInfo.Uid == contact.Uid)
                            contact.Photo = App.Current.EntityService.CurrentUser.Photo;
                        else
                            contact.Photo = App.Current.EntityService.DefaultAvatar;
                    }

                    App.Current.EntityService.Contacts.Add(contact);
                }

                _model = new ContactsByFirstName(App.Current.EntityService.Contacts.ToList());
                FriendsPanel.ItemsSource = _model;

                // In a background get info about new contacts appeared in contacts book...let it be one time per 5 days
                TimeSpan span = TimeSpan.FromTicks(DateTime.Now.Ticks - App.Current.LastContactsSync.Ticks);

                if (span.Days > 5)
                   _SearchAllContactsOnPhone(_UpdateContacts);
            }

            App.Current.IsFirstRun = false;
        }

        #endregion

        #region Event handlers

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _UpdateNotificationButtons();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        void UnreadMids_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _UpdateNotificationButtons();
        }

        void StateCounter_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _UpdateNotificationButtons();
        }

        private void SyncContactsButton_Tap(object sender, GestureEventArgs e)
        {
            _SearchAllContactsOnPhone(_UpdateContacts);
        }

        // Updates names of contacts which is already in application cache (synced earlier) with info from current contacts book.
        // If some contacts was not found in cache, they need to be synced - so we do it in a background.
        private void _UpdateContacts(object sender, ContactsSearchEventArgs e)
        {
            try
            {
                // Get contacts from contacts book.
                var contacts = e.Results;

                if (contacts == null)
                    return;

                string allNumbers = string.Empty;

// TODO. Probably, we should also REMOVE contacts from application, which was removed from phone...
// but this is weird case and I don't actually know what exactly to do.

                // Add contacts to the contacts cache if it is not in there yet,
                // and create string of phone numbers to request (if need).
                int watchDog = 0;

                var cache = _contactsCache.GetItems();

                foreach (var contact in contacts)
                {
                    // Get only mobile.
                    var phone = contact.PhoneNumbers.FirstOrDefault(x => x.Kind == PhoneNumberKind.Mobile);

                    if (phone == null || string.IsNullOrEmpty(phone.PhoneNumber))
                        continue;

                    // Try to find mobile phone in cache.
                    var cachedContact = cache.FirstOrDefault(x => _IsEquals(x.VerifiedPhone, _GetOnlyDigits(phone.PhoneNumber)));
                    var viewContact = App.Current.EntityService.Contacts.FirstOrDefault(x => _IsEquals(x.VerifiedPhone, _GetOnlyDigits(phone.PhoneNumber)));

                    if (cachedContact == null)
                    {
                        // Create application contact for it and save... and do request.
                        var phoneContact = new PhoneContact(contact.DisplayName, _GetOnlyDigits(phone.PhoneNumber), App.Current.EntityService.DefaultAvatar);
                        _contactsCache.AddItem(phoneContact);

                        if (viewContact == null)
                            App.Current.EntityService.Contacts.Add(phoneContact);

                        // Do not send more than 1000 per one request.

//TODO. Need functionality to re-request. But, does it really need? Who have more than 1000 contacts?

                        // Get string with phone numbers splitted by comma
                        string digits = _GetOnlyDigits(phone.PhoneNumber);
                        if (watchDog < 1000 && !string.IsNullOrEmpty(digits)) 
                        {
                            allNumbers += digits + ",";
                            watchDog += 1;
                        }
                    }
                    else
                    {
                        // Just update name info.
                        cachedContact.ContactName = contact.DisplayName;

                        if (viewContact != null)
                            viewContact.ContactName = contact.DisplayName;
                    }
                }

                _contactsCache.Save();

                // If need to do request info...do it.
                if (!string.IsNullOrEmpty(allNumbers))
                {
                    // Remove last comma.
                    allNumbers = allNumbers.Remove(allNumbers.Length - 1, 1);

                    GlobalIndicator.Instance.Text = AppResources.ContactsSyncing;
                    GlobalIndicator.Instance.IsLoading = true;

                    // Get info.
                    var op = new FriendsGetByPhones(allNumbers, _GetUserInfo);
                    op.Execute();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ContactsSearch failed: " + ex.Message);
            }
        }

        private void onSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PhoneContact contact = FriendsPanel.SelectedItem as PhoneContact;

            if (contact != null)
            {
                App.Current.EntityService.CurrentPhoneContact = contact;

                if (contact.NeedRequest)
                    NavigationService.Navigate(new Uri(@"/Views/ProfilePage.xaml?synced=0", UriKind.Relative));
                else
                    NavigationService.Navigate(new Uri(@"/Views/ProfilePage.xaml?synced=1", UriKind.Relative));
            }

            FriendsPanel.SelectedItem = null;
        }

        private void FriendsPageTitle_Tap(object sender, GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/FriendsPage.xaml", UriKind.Relative));
        }

        private void DialogsPageTitle_Tap(object sender, GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/DialogsPage.xaml", UriKind.Relative));
        }

        private void Search_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/SearchPage.xaml", UriKind.Relative));
        }

        private void Refresh_Click(object sender, EventArgs e)
        {
            try
            {
                _SearchAllContactsOnPhone(_UpdateContacts);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Refresh_Click failed:" + ex.Message);
            }
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

        private void _CreateBar()
        {
            this.ApplicationBar = new ApplicationBar();

            var appBarButton = new ApplicationBarIconButton(
                new Uri("/Images/Appbar_Icons/appbar.feature.search.rest.png", UriKind.Relative));
            appBarButton.Text = AppResources.SearchButton;
            appBarButton.Click += Search_Click;
            this.ApplicationBar.Buttons.Add(appBarButton);

            var appBarButton1 = new ApplicationBarIconButton(
                new Uri("/Images/Appbar_Icons/appbar.refresh.rest.png", UriKind.Relative));
            appBarButton1.Text = AppResources.RefreshButton;
            appBarButton1.Click += Refresh_Click;
            this.ApplicationBar.Buttons.Add(appBarButton1);
        }

        private void _SearchAllContactsOnPhone(Action<object, ContactsSearchEventArgs> handler)
        {
            if (App.Current.EntityService.CurrentUser != null &&
                App.Current.EntityService.CurrentUser.UserInfo.HasMobile) // Only user with registered phone may get friends by phones.
            {
                Contacts contacts = new Contacts();
                contacts.SearchCompleted += new EventHandler<ContactsSearchEventArgs>(handler);
                contacts.SearchAsync(String.Empty, FilterKind.None, null);
            }
        }

        /// <summary>
        /// Merges information we already have with information about verified phone numbers from VK.
        /// </summary>
        private void _GetUserInfo(IList<UserInfo> usersInfo)
        {
            try
            {
                foreach (var info in usersInfo)
                {
                    var friend = App.Current.EntityService.Friends.FirstOrDefault(x => x.Uid == info.Uid);

                    if (friend == null)
                        continue; // Skip.

                    // Save cool number.
                    friend.VerifiedPhone = info.VerifiedPhone;

                    // Compare phone numbers and update contact with existent contact info.
                    foreach (var contact in App.Current.EntityService.Contacts)
                    {
                        if (_IsEquals(contact.VerifiedPhone, friend.VerifiedPhone)) // Custom equality method for phone numbers.
                        {
                            contact.Photo = friend.Photo; // image in view model
                            contact.Uid = friend.Uid;
                            contact.FullName = friend.FullName;
                            contact.NeedRequest = false; // It was true by default.

                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("_GetUserInfo failed: " + ex.Message);
            }

            Dispatcher.BeginInvoke(() =>
            {
                // Save sync time.
                App.Current.LastContactsSync = DateTime.Now;

                // Push changes into view.
                GlobalIndicator.Instance.Text = string.Empty;
                GlobalIndicator.Instance.IsLoading = false;

                try
                {
                    _model = new ContactsByFirstName(App.Current.EntityService.Contacts.ToList());
                    FriendsPanel.ItemsSource = _model;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("_GetUserInfo failed in ContactsPage: " + ex.Message);
                }

                InviteFriendButton.Visibility = Visibility.Visible;
                FriendsPanel.Visibility = Visibility.Visible;

                SyncContactsDescription.Visibility = Visibility.Collapsed;
                SyncContactsButton.Visibility = Visibility.Collapsed;
            });
        }

        /// <summary>
        /// Compares 2 phone numbers.
        /// </summary>
        private bool _IsEquals(string phone1, string phone2)
        {
            if (_GetCommonPhoneNumberPart(phone1) == _GetCommonPhoneNumberPart(phone2))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Gets only common part of number (without code).
        /// </summary>
        private string _GetCommonPhoneNumberPart(string phoneNumber)
        {
            string number = _GetOnlyDigits(phoneNumber);

            if (number.Length == 10)
                return phoneNumber;
            else if (phoneNumber.Length == 11)
                return phoneNumber.Remove(0, 1); // remove code
            else
                return number;
        }

        /// <summary>
        /// Removes all non-digit symbols from phone number.
        /// </summary>
        private string _GetOnlyDigits(string phoneNumber)
        {
            if (phoneNumber == null)
                return string.Empty;

            var items = phoneNumber.ToCharArray();
            string number = string.Empty;

            foreach (var item in items)
            {
                if (Char.IsDigit(item))
                    number += item;
            }

            return number;
        }

        #endregion

        #region Private fields

        private ContactsCache _contactsCache;
        private ContactsByFirstName _model;

        #endregion
    }
}