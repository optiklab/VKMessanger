using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using SlXnaApp1.Entities;
using SlXnaApp1.Infrastructure;
using SlXnaApp1.Json;

namespace SlXnaApp1.Views
{
    public partial class SearchPage : PhoneApplicationPage
    {
        #region Constructor

        public SearchPage()
        {
            InitializeComponent();

            _invisibleContacts = App.Current.EntityService.Contacts;

            _friends = new ObservableCollection<FriendViewModel>();
            _contacts = new ObservableCollection<PhoneContact>();
            _others = new ObservableCollection<UserInfo>();

            _model = new SearchResultsByType(_friends, _contacts, _others);

            friends.ItemsSource = _model;

            _helper = new TransliteHelper();
        }

        #endregion

        #region Event handlers

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            App.Current.EntityService.SearchQuery = SearchTextBox.Text;
            string q = SearchTextBox.Text.ToLower();
            string translite = _helper.ReverseString(q);

            if (!string.IsNullOrEmpty(q))
            {
                SearchDescription.Visibility = System.Windows.Visibility.Collapsed;

                var friendsSel = App.Current.EntityService.Friends.Where(x => x.FullName.ToLower().StartsWith(q) ||
                    x.FullName.ToLower().StartsWith(translite));

                _friends.Clear();
                foreach (var friend in friendsSel)
                {
                    _friends.Add(friend);
                }

                var contacts = _invisibleContacts.Where(x => x.FullName.ToLower().StartsWith(q) || x.ContactName.ToLower().StartsWith(q) ||
                    x.FullName.ToLower().StartsWith(translite) || x.ContactName.ToLower().StartsWith(translite));

                _contacts.Clear();
                foreach (var contact in contacts)
                {
                    _contacts.Add(contact);
                }

                var others = App.Current.EntityService.OtherUsers.Where(x => x.FullName.ToLower().StartsWith(q) ||
                    x.FullName.ToLower().StartsWith(translite));

                _others.Clear();
                foreach (var other in others)
                {
                    _others.Add(other);
                }
            }
            else
            {
                SearchDescription.Visibility = System.Windows.Visibility.Visible;

                _friends.Clear();
                _contacts.Clear();
            }

            _model = new SearchResultsByType(_friends, _contacts, _others);
            friends.ItemsSource = _model;
        }

        private void onSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var friend = friends.SelectedItem as FriendViewModel;

            if (friend != null)
            {
                NavigationService.Navigate(new Uri(@"/Views/ChatPage.xaml?id=" + friend.Uid, UriKind.Relative));
            }
            else
            {
                var contact = friends.SelectedItem as PhoneContact;

                if (contact != null)
                {
                    App.Current.EntityService.CurrentPhoneContact = contact;

                    if (contact.NeedRequest)
                        NavigationService.Navigate(new Uri(@"/Views/ProfilePage.xaml?synced=0", UriKind.Relative));
                    else
                        NavigationService.Navigate(new Uri(@"/Views/ProfilePage.xaml?synced=1", UriKind.Relative));
                }
                else
                {
                    var other = friends.SelectedItem as UserInfo;

                    if (other != null)
                    {
                        if (other.Uid > 0)
                            NavigationService.Navigate(new Uri(@"/Views/ChatPage.xaml?id=" + other.Uid, UriKind.Relative));
                    }
                }
            }

            friends.SelectedItem = null;
        }

        private void FullName_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            TextBlock curBlock = sender as TextBlock;

            if (null != curBlock)
            {
                string query = App.Current.EntityService.SearchQuery;

                int index = curBlock.Text.ToLower().IndexOf(query.ToLower());

                if (index > -1)
                {
                    string begin = curBlock.Text.Substring(0, index);
                    string selection = curBlock.Text.Substring(index, query.Length);
                    string end = curBlock.Text.Substring(index + query.Length);

                    curBlock.Inlines.Clear();
                    curBlock.Inlines.Add(new Run() { Text = begin, Foreground = App.Current.AntiPhoneBackgroundBrush });
                    curBlock.Inlines.Add(new Run() { Text = selection, Foreground = App.Current.BlueBrush });
                    curBlock.Inlines.Add(new Run() { Text = end, Foreground = App.Current.AntiPhoneBackgroundBrush });
                }
                else
                {
                    string text = curBlock.Text;
                    curBlock.Inlines.Clear();
                    curBlock.Inlines.Add(new Run() { Text = text, Foreground = App.Current.AntiPhoneBackgroundBrush });
                }

                friends.UpdateLayout();
            }
        }

        private void ContactName_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            TextBlock curBlock = sender as TextBlock;

            if (null != curBlock)
            {
                string query = App.Current.EntityService.SearchQuery;

                int index = curBlock.Text.ToLower().IndexOf(query.ToLower());

                if (index > -1)
                {
                    string begin = curBlock.Text.Substring(0, index);
                    string selection = curBlock.Text.Substring(index, query.Length);
                    string end = curBlock.Text.Substring(index + query.Length);

                    curBlock.Inlines.Clear();
                    curBlock.Inlines.Add(new Run() { Text = begin, Foreground = App.Current.AntiPhoneBackgroundBrush });
                    curBlock.Inlines.Add(new Run() { Text = selection, Foreground = App.Current.BlueBrush });
                    curBlock.Inlines.Add(new Run() { Text = end, Foreground = App.Current.AntiPhoneBackgroundBrush });
                }
                else
                {
                    string text = curBlock.Text;
                    curBlock.Inlines.Clear();
                    curBlock.Inlines.Add(new Run() { Text = text, Foreground = App.Current.AntiPhoneBackgroundBrush });
                }

                friends.UpdateLayout();
            }
        }

        #endregion

        #region Private fields

        private ObservableCollection<FriendViewModel> _friends;
        private ObservableCollection<PhoneContact> _contacts;
        private ObservableCollection<UserInfo> _others;

        private IList<PhoneContact> _invisibleContacts;
        private TransliteHelper _helper;
        private SearchResultsByType _model;

        #endregion
    }
}