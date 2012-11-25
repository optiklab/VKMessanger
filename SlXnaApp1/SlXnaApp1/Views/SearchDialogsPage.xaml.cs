using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using SlXnaApp1.Api;
using SlXnaApp1.Entities;
using SlXnaApp1.Infrastructure;
using SlXnaApp1.Json;
using System;
using System.Collections.Generic;
using System.Windows.Documents;
using System.Diagnostics;

namespace SlXnaApp1.Views
{
    public partial class SearchDialogsPage : PhoneApplicationPage
    {
        #region Constructor

        public SearchDialogsPage()
        {
            InitializeComponent();

            PivotControl.SelectionChanged += PivotControl_SelectionChanged;

            _dialogsModel = new ObservableCollection<SearchDialogViewModel>();
            _messageModel = new ObservableCollection<SearchDialogViewModel>();

            MessagesPanel.ItemsSource = _messageModel;
            DialogsPanel.ItemsSource = _dialogsModel;
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

            _userTypingTimer = new Timer(state =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        _workQ = SearchTextBox.Text;
                        App.Current.EntityService.SearchQuery = _workQ;

                        if (PivotControl.SelectedItem == MessagesPivotItem)
                            _UpdateMessages();
                        else
                            _UpdateDialogs();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("SearchTextBox_TextChanged failed in SearchDialogsPage: " + ex.Message);
                    }
                });
            }, null, 1000, -1);

        }

        void PivotControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PivotControl.SelectedItem == MessagesPivotItem)
            {
                if (_messagesQ != _workQ)
                    _UpdateMessages();
            }
            else
            {
                if (_dialogsQ != _workQ)
                    _UpdateDialogs();
            }
        }

        private void onSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SearchDialogViewModel result = null;

            if (PivotControl.SelectedItem == MessagesPivotItem)
                result = MessagesPanel.SelectedItem as SearchDialogViewModel;
            else
                result = DialogsPanel.SelectedItem as SearchDialogViewModel;

            if (result != null)
            {
                if (result.ChatId > 0)
                    NavigationService.Navigate(new Uri(@"/Views/GroupChatPage.xaml?id=" + result.ChatId, UriKind.Relative));
                else
                    NavigationService.Navigate(new Uri(@"/Views/ChatPage.xaml?id=" + result.Uid, UriKind.Relative));
            }

            MessagesPanel.SelectedItem = null;
            DialogsPanel.SelectedItem = null;
        }

        private void TextBlock_Loaded(object sender, System.Windows.RoutedEventArgs e)
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
                    curBlock.Inlines.Add(new Run() { Text = begin, Foreground = App.Current.GrayBrush });
                    curBlock.Inlines.Add(new Run() { Text = selection, Foreground = App.Current.BlueBrush });
                    curBlock.Inlines.Add(new Run() { Text = end, Foreground = App.Current.GrayBrush });
                }
                else
                {
                    string text = curBlock.Text;
                    curBlock.Inlines.Clear();
                    curBlock.Inlines.Add(new Run() { Text = text, Foreground = App.Current.GrayBrush });
                }

                MessagesPanel.UpdateLayout();
            }
        }

        private void TextBlockD_Loaded(object sender, System.Windows.RoutedEventArgs e)
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

                DialogsPanel.UpdateLayout();
            }
        }
        #endregion

        #region Private methods

        private void _UpdateDialogs()
        {
            // Start search after 1 second of silence.
            var op = new MessagesSearchDialogs(_workQ, results =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    _dialogsQ = _workQ;
                    _dialogsModel.Clear();

                    try
                    {
                        // Add found dialogs with users.
                        foreach (var result in results)
                        {
                            var friend = App.Current.EntityService.Friends.FirstOrDefault(x => x.Uid == result.Uid);

                            bool isOnline = false;
                            var photo = App.Current.EntityService.DefaultAvatar;
                            string title = result.Title;
                            string fullName = result.FirstName + " " + result.LastName;

                            if (friend != null)
                            {
                                isOnline = friend.IsOnline;
                                photo = friend.Photo;
                            }
                            else
                            {
                                // Search in non-friends.
                                var user = App.Current.EntityService.OtherUsers.FirstOrDefault(x => x.Uid == result.Uid);

                                if (user != null)
                                {
                                    isOnline = user.IsOnline;
                                    photo = user.ImagePhoto;
                                }
                            }

                            if (string.IsNullOrEmpty(title))
                                _dialogsModel.Add(new SearchDialogViewModel(result.Uid, -1, fullName,
                                    string.Empty, 0, false, isOnline, photo));
                            else
                                _dialogsModel.Add(new SearchDialogViewModel(-1, result.ChatId, title,
                                    string.Empty, 0, false, isOnline, photo));
                        }

                        // Local search for chats.
                        string q = _workQ.ToLower();
                        var chats = App.Current.EntityService.Dialogs.Where(x => x.Title.ToLower().Contains(q) && x.IsConference);

                        if (chats != null && chats.Any())
                        {
                            foreach (var chat in chats)
                            {
                                _dialogsModel.Add(new SearchDialogViewModel(-1, chat.ChatId, chat.Title,
                                    string.Empty, 0, false, false, chat.Photo));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("_UpdateDialogs on SearchDialogsPage failed: " + ex.Message);
                    }
                });
            });
            op.Execute();
        }

        private void _UpdateMessages()
        {
            // Start search after 1 second of silence.
            var op = new MessagesSearch(_workQ, results =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    _messagesQ = _workQ;
                    _messageModel.Clear();

                    try
                    {
                        foreach (var result in results)
                        {
                            var friend = App.Current.EntityService.Friends.FirstOrDefault(x => x.Uid == result.Uid);

                            bool isOnline = false;
                            var photo = App.Current.EntityService.DefaultAvatar;
                            string title = result.Title;
                            string fullName = string.Empty;

                            if (friend != null)
                            {
                                isOnline = friend.IsOnline;

                                // Set photo from friend or from chat.
                                if (result.Chatid > 0)
                                {
                                    var chat = App.Current.EntityService.Dialogs.FirstOrDefault(x => x.ChatId == result.Chatid && x.IsConference);

                                    if (chat != null)
                                        photo = chat.Photo;
                                }
                                else
                                    photo = friend.Photo;

                                fullName = friend.FullName;
                            }
                            else
                            {
                                // Search in non-friends dialogs.
                                var user = App.Current.EntityService.OtherUsers.FirstOrDefault(x => x.Uid == result.Uid);

                                if (user != null)
                                    photo = user.ImagePhoto;
                            }

                            string body = CommonHelper.GetFormattedMessage(result.Body);

                            if (body.Length > 30)
                                body = body.Substring(0, 30);

                            if (result.Chatid > 0)
                                _messageModel.Add(new SearchDialogViewModel(-1, result.Chatid, title,
                                    body, result.Date, result.IsOut, isOnline, photo));
                            else
                                _messageModel.Add(new SearchDialogViewModel(result.Uid, -1, !string.IsNullOrEmpty(fullName) ? fullName : title,
                                    body, result.Date, result.IsOut, isOnline, photo));
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("_UpdateMessages on SearchDialogsPage failed: " + ex.Message);
                    }
                });
            });
            op.Execute();
        }

        private string _GetTitle(SearchResult result)
        {
            return string.IsNullOrEmpty(result.Title) ? result.FirstName + " " + result.LastName : result.Title;
        }

        #endregion

        #region Private fields

        /// <summary>
        /// Is used to minimize count of possible search requests: we wait until user
        /// will finish his request. If user is paused typing during 1 second: we start to search.
        /// </summary>
        private Timer _userTypingTimer = null;
        private ObservableCollection<SearchDialogViewModel> _dialogsModel;
        private ObservableCollection<SearchDialogViewModel> _messageModel;
        private string _workQ;
        private string _messagesQ;
        private string _dialogsQ;

        #endregion
    }
}