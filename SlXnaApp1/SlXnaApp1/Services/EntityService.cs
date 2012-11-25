using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Devices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using SlXnaApp1.Api;
using SlXnaApp1.Cache;
using SlXnaApp1.Entities;
using SlXnaApp1.Infrastructure;
using SlXnaApp1.Json;

namespace SlXnaApp1.Services
{
    public class EntityService : IApiService
    {
        #region Constructor

        public EntityService()
        {
            _dialogsCache = new DialogsCache();
            _friendsCache = new FriendsCache();
            _usersCache = new UsersCache();
            _imageCache = new ImageCache();
            _messagesCache = new MessagesCache();

            Dialogs = new ObservableCollection<Dialog>();
            Contacts = new ObservableCollection<PhoneContact>();
            Friends = new ObservableCollection<FriendViewModel>();
            FriendsRequests = new ObservableCollection<FriendViewModel>();
            FriendsMutual = new ObservableCollection<FriendViewModel>();
            AttachmentPhotos = new Dictionary<string, Stream>();
            OtherUsers = new ObservableCollection<UserInfo>();

            IsLoadingMessagesHistory = false;

            _settings = new Settings(new ProtectDataAdapter());

            _stateCounter = new StateCounter(0, new List<int>());

            DefaultAvatar = ResourceHelper.GetBitmap(@"/SlXnaApp1;component/Images/Photo_Placeholder.png");
            CurrentUser = new CurrentUserInfo(null, DefaultAvatar);

            App.Current.UpdatesService.UserBecomeOffline += UpdatesService_UserBecomeOffline;
            App.Current.UpdatesService.UserBecomeOnline += UpdatesService_UserBecomeOnline;
            App.Current.UpdatesService.MessageAdded += UpdatesService_MessageAdded;
            App.Current.UpdatesService.MessageDeleted += UpdatesService_MessageDeleted;
            App.Current.UpdatesService.MessageFlagsChanged += UpdatesService_MessageFlagsChanged;
            App.Current.UpdatesService.MessageRemovedFlags += UpdatesService_MessageRemovedFlags;
            App.Current.UpdatesService.MessageSetFlags += UpdatesService_MessageSetFlags;
            App.Current.UpdatesService.ChatChanged += UpdatesService_ChatChanged;
            App.Current.UpdatesService.TypingInChatStarted += UpdatesService_TypingInChatStarted;
            App.Current.UpdatesService.TypingInGroupChatStarted += UpdatesService_TypingInGroupChatStarted;

            try
            {
                var info = App.GetResourceStream(new Uri("Resources/Untitled.wav", UriKind.Relative));
                _soundEffect = SoundEffect.FromStream(info.Stream);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Loading SoundEffect failed: " + ex.Message);
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// PropertyChanged event.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// OnPropertyChanged event handler.
        /// </summary>
        /// <param name="propertyName">Changed property.</param>
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this,
                    new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Public properties

        public ObservableCollection<Dialog> Dialogs { get; private set; }
        public ObservableCollection<PhoneContact> Contacts { get; private set; }
        public ObservableCollection<FriendViewModel> Friends { get; private set; }
        public ObservableCollection<FriendViewModel> FriendsRequests { get; private set; }
        public ObservableCollection<FriendViewModel> FriendsMutual { get; private set; }
        public ObservableCollection<UserInfo> OtherUsers { get; private set; }
        public CurrentUserInfo CurrentUser { get; set; }
        public PhoneContact CurrentPhoneContact { get; set; }
        public FriendViewModel FoundGlobalUser { get; set; }
        public BitmapImage DefaultAvatar { get; set; }
        public string MessagesToForward { get; set; }
        public string AttachedLatitude { get; set; }
        public string AttachedLongitude { get; set; }

        public bool IsLoadingMessagesHistory { get; private set; }

        /// <summary>
        /// Flag means that Entity Service is completely initialized with friends, dialogs,
        /// current user info and even Avatars. It is false until last avatar is loaded.
        /// </summary>
        public bool IsFullyInitialized { get { return _isInited; } }

        /// <summary>
        /// Keep last search query.
        /// </summary>
        public string SearchQuery { get; set; }

        /// <summary>
        // It needs, because this list should be shared between Chat (or GroupChat) and Attachments views.
        /// <summary>
        public IDictionary<string, Stream> AttachmentPhotos { get; set; }

        /// <summary>
        /// State counter contains info about Number of Friend requests and New Messages.
        /// </summary>
        public StateCounter StateCounter { get { return _stateCounter; } }

        #endregion

        #region UpdatesService events handlers

        /// <summary>
        /// 
        /// </summary>
        private void UpdatesService_MessageAdded(object sender, Infrastructure.AddMessageEventArgs e)
        {
            Debug.WriteLine("UpdatesService_MessageAdded " + e.MessageId.ToString() + " with flags: " + e.Flags.ToString());

            _flagsToImplementForMessages.Add(e.MessageId, e.Flags);

            if (App.Current.IsVibrationOn)
            {
                VibrateController.Default.Start(TimeSpan.FromMilliseconds(10));
            }

            if (App.Current.IsSoundOn)
            {
                _PlaySound();
            }

            // So, we don't need to show toasts by ourself?
            //if (App.Current.IsToastOn)
            //{
                //ShellToast toast = new ShellToast();
                //var friend = Friends.FirstOrDefault(x => x.Uid == e.FromId);

                //if (friend != null)
                //    toast.Title = friend.FullName + ":";

                //if ((e.Flags & 16) != 0) // CHAT
                //    toast.NavigationUri = new Uri("/Views/GroupChatPage.xaml?id=" + e.FromId.ToString(), UriKind.Relative);
                //else
                //    toast.NavigationUri = new Uri("/Views/ChatPage.xaml?id=" + e.FromId.ToString(), UriKind.Relative);

                //toast.Content = e.Text;
                //toast.Show();
            //}
        }

        private void _PlaySound()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    FrameworkDispatcher.Update();
                    _soundEffect.Play();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("_PlaySound failed: " + ex.Message);
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdatesService_MessageDeleted(object sender, Infrastructure.MessageDeletedEventArgs e)
        {
            Debug.WriteLine("UpdatesService_MessageDeleted " + e.MessageId.ToString());

            DeleteMessage(e.MessageId);
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdatesService_MessageFlagsChanged(object sender, Infrastructure.MessageFlagsChangedEventArgs e)
        {
            try
            {
                Debug.WriteLine("UpdatesService_MessageFlagsChanged " + e.MessageId.ToString() + " with flags: " + e.Flags.ToString());

                var mes = _messagesCache.GetItem(e.MessageId);

                if (mes != null)
                {
                    if (_flagsToImplementForMessages.ContainsKey(e.MessageId))
                        _flagsToImplementForMessages[e.MessageId] = e.Flags;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("UpdatesService_MessageFlagsChanged failed: " + ex.Message + System.Environment.NewLine + ex.StackTrace);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdatesService_MessageRemovedFlags(object sender, Infrastructure.MessageRemoveFlagsEventArgs e)
        {
            try
            {
                Debug.WriteLine("UpdatesService_MessageRemovedFlags " + e.MessageId.ToString() + " with mask: " + e.Mask.ToString());

                var mes = _messagesCache.GetItem(e.MessageId);

                if (mes != null)
                {
                    if (_flagsToImplementForMessages.ContainsKey(e.MessageId))
                        _flagsToImplementForMessages[e.MessageId] &= ~e.Mask;

                    if ((e.Mask & 128) != 0) // DELЕTЕD becomes UNDELETED
                    {
                        mes.IsDeleted = false;

                        if (_currentChatId != -1 && _currentChatViewModel != null)
                        {
                            Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                try
                                {
                                    _currentChatViewModel.AddMessage(mes);
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine("UpdatesService_MessageRemovedFlags failed in EntityService: " + ex.Message);
                                }
                            });
                        }
                    }

                    if ((e.Mask & 1) != 0 && mes.IsOut) // UNREAD becomes READ
                    {
                        mes.IsRead = true;

                        if (_currentChatId != -1 && _currentChatViewModel != null)
                        {
                            var message = _currentChatViewModel.Messages.FirstOrDefault(x => x.Mid == mes.Mid);

                            if (message != null)
                                Deployment.Current.Dispatcher.BeginInvoke(() => { message.IsRead = mes.IsRead; });
                        }
                    }
                }
                // TODO Update Model View.
            }
            catch (Exception ex)
            {
                Debug.WriteLine("UpdatesService_MessageRemovedFlags failed: " + ex.Message + System.Environment.NewLine + ex.StackTrace);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdatesService_MessageSetFlags(object sender, Infrastructure.MessageSetFlagsEventArgs e)
        {
            try
            {
                Debug.WriteLine("UpdatesService_MessageSetFlags " + e.MessageId.ToString() + " with mask: " + e.Mask.ToString());

                var mes = _messagesCache.GetItem(e.MessageId);

                if (mes != null)
                {
                    if (_flagsToImplementForMessages.ContainsKey(e.MessageId))
                        _flagsToImplementForMessages[e.MessageId] |= e.Mask;

                    if ((e.Mask & 128) != 0) // UNDELETED becomes DELETED
                    {
                        mes.IsDeleted = true;

                        if (_currentChatId != -1 && _currentChatViewModel != null)
                        {
                            var message = _currentChatViewModel.Messages.FirstOrDefault(x => x.Mid == mes.Mid);

                            // Remove message from view.
                            if (message != null)
                                Deployment.Current.Dispatcher.BeginInvoke(() => { _currentChatViewModel.Messages.Remove(message); });
                        }
                    }
                }

                // TODO Update Model View.
            }
            catch (Exception ex)
            {
                Debug.WriteLine("UpdatesService_MessageSetFlags failed: " + ex.Message + System.Environment.NewLine + ex.StackTrace);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdatesService_ChatChanged(object sender, Infrastructure.ChatChangedEventArgs e)
        {
            Debug.WriteLine("UpdatesService_ChatChanged with chatid " + e.ChatId.ToString() + " by " + (e.Self == 1 ? "myself" : "some user"));

            if (_currentChatViewModel != null)
                _currentChatViewModel.UpdateStatusInfo();

            // TODO. Group chat avatar not updated right in the time.
            var dialog = Dialogs.FirstOrDefault(x => x.ChatId == e.ChatId && x.IsConference);

            if (dialog != null && dialog.IsConference)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        var key = _GenereateGroupChatAvatar(dialog);

                        if (key == string.Empty)
                        {
                            dialog.Photo = DefaultAvatar;
                        }
                        else
                        {
                            var image = _imageCache.GetItem(key);

                            if (image != null)
                                dialog.Photo = image;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("UpdatesService_ChatChanged failed in EntityService: " + ex.Message);
                    }
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdatesService_TypingInGroupChatStarted(object sender, Infrastructure.StartTypingInGroupChatEventArgs e)
        {
            try
            {
                Debug.WriteLine("UpdatesService_TypingInGroupChatStarted UserID: " + e.UserId.ToString() + " ChatID " + e.ChatId.ToString());

                if (_currentChatId == e.ChatId && _currentChatViewModel != null)
                {
                    FriendViewModel friend = Friends.FirstOrDefault(x => e.UserId == x.Uid);

                    if (friend != null)
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            try
                            {
                                _currentChatViewModel.Action = String.Format(AppResources.IsTypingLabel,
                                    friend.FullName); // FullName is more unique for conference than just name

                                _CreateTypingTimer();
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("UpdatesService_TypingInGroupChatStarted failed in EntityService: " + ex.Message);
                            }
                        });
                    }
                    else
                    {
                        // Sometimes messages may be sent by NON-FRIEND.
                        var user = App.Current.EntityService.OtherUsers.FirstOrDefault(x => x.Uid == e.UserId);
                        if (user != null)
                            Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                try
                                {
                                    _currentChatViewModel.Action = String.Format(AppResources.IsTypingLabel,
                                        user.FullName); // FullName is more unique for conference than just name

                                    _CreateTypingTimer();
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine("UpdatesService_TypingInGroupChatStarted failed in EntityService: " + ex.Message);
                                }
                            });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("UpdatesService_TypingInGroupChatStarted failed: " + ex.Message + System.Environment.NewLine + ex.StackTrace);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdatesService_TypingInChatStarted(object sender, Infrastructure.StartTypingInChatEventArgs e)
        {
            try
            {
                Debug.WriteLine("UpdatesService_TypingInChatStarted UserID: " + e.UserId.ToString() + " Flags: " + e.Flags.ToString());

                if (_currentChatId == e.UserId && _currentChatViewModel != null)
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        try
                        {
                            _currentChatViewModel.Status = String.Format(AppResources.IsTypingLabel, string.Empty);

                            _CreateTypingTimer();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("UpdatesService_TypingInChatStarted failed in EntityService: " + ex.Message);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("UpdatesService_TypingInChatStarted failed: " + ex.Message + System.Environment.NewLine + ex.StackTrace);
            }
        }

        #endregion

        #region Online/Offline status handlers

        private void UpdatesService_UserBecomeOnline(object sender, Infrastructure.UserOnlineEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    _UpdateOnlineFriendStatus(e.UserId, true);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("UpdatesService_UserBecomeOnline failed in EntityService: " + ex.Message);
                }
            });
        }

        private void UpdatesService_UserBecomeOffline(object sender, Infrastructure.UserOfflineEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    _UpdateOnlineFriendStatus(e.UserId, false);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("UpdatesService_UserBecomeOffline failed in EntityService: " + ex.Message);
                }
            });
        }

        private void _UpdateOnlineFriendStatus(int userId, bool isOnline)
        {
            try
            {
                FriendViewModel friend = Friends.FirstOrDefault(x => userId == x.Uid);
                string tempString = (isOnline ? "online" : "offline");

                if (friend != null)
                {
                    friend.IsOnline = isOnline;

                    Debug.WriteLine("Friend http://vk.com/id" + userId + " " + friend.FullName + " become " + tempString);
                }
                else
                {
                    // Sometimes messages may be sent by NON-FRIEND.
                    var user = App.Current.EntityService.OtherUsers.FirstOrDefault(x => x.Uid == userId);
                    if (user != null)
                    {
                        user.IsOnline = isOnline;
                        Debug.WriteLine("Friend http://vk.com/id" + userId + " " + user.FullName + " become " + tempString);
                    }
                    else
                    {
                        Debug.WriteLine("Friend http://vk.com/id" + userId + " become " + tempString);
                    }
                }

                if (_currentChatViewModel != null)
                    _currentChatViewModel.UpdateStatusInfo();

                var dialog = Dialogs.FirstOrDefault(y => !y.IsConference && y.Uid == userId);

                if (dialog != null)
                    dialog.IsOnline = isOnline;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("_UpdateOnlineFriendStatus failed: " + ex.Message + System.Environment.NewLine + ex.StackTrace);
            }
        }

        #endregion

        #region Public method

        public void Initialize()
        {
            _waitHandle = new ManualResetEvent(false);

            UpdateFriends(); // Get friends from the web.

            _waitHandle.WaitOne();// Wait here until friends will be loaded.
            _waitHandle.Reset();

            UpdateCurrentUserInfo();

            _waitHandle.WaitOne();
            _waitHandle.Reset();

            var users = _usersCache.GetItems();
            foreach (var item in users)
                OtherUsers.Add(item);

            _InitializeDialogsList();

            _waitHandle.WaitOne();
            _waitHandle.Reset();

            _ReinitializeCounters();

            _waitHandle.WaitOne();
            _waitHandle.Reset();

            // Check if after last switching of Silence mode it is still should be silent.
            if (!_settings.IsPushOn && DateTime.Now < _settings.PushTurnOnTime)
            {
                // Turn on push notifications when time occurs.
                TimeSpan temp = _settings.PushTurnOnTime - DateTime.Now;
                var op = new AccountSetSilenceMode(App.Current.PushNotifications.ChannelUri, temp.Seconds, isOk => { });
                op.Execute();
            }
        }

        /// <summary>
        /// Loads friends avatars from the web and updates it in UI.
        /// </summary>
        public void LoadAvatars()
        {
            try
            {
                var itemsToLoad = _GetInitialAvatarsToLoad();

                if (!itemsToLoad.Any())
                    _isInited = true; // Only if no one item loaded we may think that nothing was initialized - this is first time.
                else
                {
                    var service = new AsyncAvatarsLoader();
                    service.LoadAvatars(itemsToLoad, _UpdateAvatar, () =>
                    {
                        var dialogsToReloadPicture = Dialogs.Where(d => d.IsConference);

                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            try
                            {
                                foreach (var dialog in dialogsToReloadPicture)
                                {
                                    var key = _GenereateGroupChatAvatar(dialog);

                                    if (key == string.Empty)
                                    {
                                        dialog.Photo = DefaultAvatar;
                                    }
                                    else
                                    {
                                        var image = _imageCache.GetItem(key);

                                        if (image != null)
                                            dialog.Photo = image;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("LoadAvatars failed in EntityService: " + ex.Message);
                            }

                            _isInited = true;
                        });
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("LoadAvatars failed " + ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void LoadMoreDialogs()
        {
            var getDialogs = new ExecuteDialogs(Dialogs.Count, _UpdateDialogsList);//new DialogsGet(DIALOGS_COUNT, Dialogs.Count, _friendsCache.GetItems(), _UpdateDialogsList);
            getDialogs.Execute();
        }

        /// <summary>
        /// 
        /// </summary>
        public void UpdateDialogs()
        {
            // Get MAX of latest dialogs (because of offset 0).

            var getDialogs = new ExecuteDialogs(0, (dialogs, profiles) => //new DialogsGet(DIALOGS_COUNT, 0, _friendsCache.GetItems(), dialogs =>
            {
                try
                {
                    _AddDistinctUserInfo(profiles);

                    var cached = _dialogsCache.GetItems();

                    // Find dialogs which should be added or updated.
                    foreach (var dialog in dialogs)
                    {
                        Dialog current_cached = cached.FirstOrDefault(x => (x.Uid == dialog.Uid && !x.IsConference && !dialog.IsConference) ||
                                                                      (x.ChatId == dialog.ChatId && x.IsConference && dialog.IsConference));

                        if (current_cached == null) // Completely new dialog.
                            _dialogsCache.AddItem(dialog);
                        else // Replace with latest one
                            _dialogsCache.RenewItem(current_cached, dialog);
                    }

                    _dialogsCache.Save();

                    _ReinitializeCounters();

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        try
                        {
                            _PushDialogsToView();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("UpdateDialogs->_PushDialogsToView failed in EntityService: " + ex.Message);
                        }
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("EntityService _UpdateDialogsList failed: " + ex.Message);
                }
            });
            getDialogs.Execute();
        }

        /// <summary>
        /// 
        /// </summary>
        public void UpdateFriends()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    var getFriends = new FriendsGet(friends =>
                    {
                        try
                        {
                            // Update cache.
                            var cachedFriends = _friendsCache.GetItems();
                            foreach (var friend in friends)
                            {
                                Friend cached = cachedFriends.FirstOrDefault(x => x.Uid == friend.Uid);

                                if (cached != null)
                                    cached = friend;
                                else
                                    _friendsCache.AddItem(friend);

                                // Remove duplicated info.
                                var userInfo = OtherUsers.FirstOrDefault(o => o.Uid == friend.Uid);
                                OtherUsers.Remove(userInfo);
                            }

                            _friendsCache.Save();

                            Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                try
                                {
                                    // Show results.
                                    _PushFriendsToView();
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine("UpdateFriends->_PushDialogsToView failed in EntityService: " + ex.Message);
                                }

                                _waitHandle.Set();
                            });
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("UpdateFriends => callback failed: " + ex.Message);
                            _waitHandle.Set();
                        }
                    });
                    getFriends.Execute();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("EntityService -> UpdateFriends failed: " + ex.Message);
                    _waitHandle.Set();
                }
            });
        }

        public void GetFriendRequests(Action finishedCallback)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    var op = new FriendsGetRequests(1, friends =>
                    {
                        try
                        {
                            _stateCounter.CountOfRequests = friends.Count;

                            // Collect all uids to load info.
                            var dict = new Dictionary<int, string>();
                            string uids = string.Empty;
                            string mutualUids = string.Empty;
                            foreach (var request in friends)
                            {
                                uids += request.Uid.ToString() + ",";

                                dict.Add(request.Uid, request.Message == null ? string.Empty : request.Message);

                                if (request.MutualFriends != null)
                                {
                                    foreach (var mutual in request.MutualFriends.Uids)
                                    {
                                        mutualUids += mutual.ToString() + ",";
                                    }
                                }
                            }

                            // Get friends requests.
                            if (!string.IsNullOrEmpty(uids))
                            {
                                UsersGet op1 = new UsersGet(uids, info =>
                                {
                                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                                    {
                                        try
                                        {
                                            FriendsRequests.Clear();

                                            foreach (var user in info)
                                            {
                                                var model = new FriendViewModel(user.Uid, user.FullName, string.Empty,
                                                    user.FirstName, user.LastName, user.IsOnline, -1, user.PhotoBig, DefaultAvatar);

                                                string message = string.Empty;
                                                dict.TryGetValue(user.Uid, out message);
                                                model.Message = message;
                                                FriendsRequests.Add(model);//new FriendRequestViewModel(model, user.Photo, message));
                                            }

                                            if (string.IsNullOrEmpty(mutualUids))
                                                finishedCallback();
                                        }
                                        catch (Exception ex)
                                        {
                                            Debug.WriteLine("UsersGet in GetFriendRequests failed: " + ex.Message);
                                            finishedCallback();
                                        }
                                    });
                                });
                                op1.Execute();
                            }

                            // Get mutual friends.
                            if (!string.IsNullOrEmpty(mutualUids))
                            {
                                UsersGet op2 = new UsersGet(mutualUids, info =>
                                {
                                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                                    {
                                        try
                                        {
                                            FriendsMutual.Clear();

                                            foreach (var user in info)
                                            {
                                                var inFriends = Friends.FirstOrDefault(x => x.Uid == user.Uid);

                                                if (inFriends == null)
                                                    FriendsMutual.Add(new FriendViewModel(user.Uid, user.FullName, string.Empty,
                                                        user.FirstName, user.LastName, user.IsOnline, -1, user.PhotoBig, DefaultAvatar));
                                            }

                                            finishedCallback();
                                        }
                                        catch (Exception ex)
                                        {
                                            Debug.WriteLine("UsersGet in GetFriendRequests failed: " + ex.Message);
                                            finishedCallback();
                                        }
                                    });
                                });
                                op2.Execute();
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(uids))
                                    finishedCallback();
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("GetFriendRequests failed in EntityService: " + ex.Message);
                            finishedCallback();
                        }
                    });
                    op.Execute();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("GetFriendRequests failed: " + ex.Message);
                }
            });
        }
        /// <summary>
        /// 
        /// </summary>
        public void UpdateCurrentUserInfo()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    UsersGet op = new UsersGet(_settings.UserId, usersInfo =>
                    {
                        _waitHandle.Set();

                        if (usersInfo != null && _settings != null && usersInfo.Any())
                        {
                            var info = usersInfo.FirstOrDefault();

                            if (info != null)
                                Deployment.Current.Dispatcher.BeginInvoke(() => { CurrentUser.UserInfo = info; });
                        }
                    });
                    op.Execute();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("EntityService -> Initialize -> UsersGet failed: " + ex.Message);

                    _waitHandle.Set();
                }
            });
        }

        /// <summary>
        /// Call this method ONLY and ONLY if you have got Long Poll History already.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isConference"></param>
        public void LoadMoreMessages(int id, bool isConference)
        {
            if (_currentChatViewModel == null)
                return;

            IsLoadingMessagesHistory = true;

            var currentMessages = _messagesCache.GetItems(id);

            // Only and only if any messages already presents, we may load more elder data (since we think, that this data is latest and greatest).
            // Otherwise we can get unsynchronized data with VK.
            int number = _currentChatViewModel.Messages.Count;
            if (number < currentMessages.Count)
            {
                try
                {
                    // Where from to start take.
                    int start = currentMessages.Count - number;

                    // how much take.
                    int finish = start - MESSAGES_COUNT;
                    if (finish < 0)
                        finish = 0;

                    for (int i = start - 1; i >= finish; i--)
                    {
                        _currentChatViewModel.AddOldMessage(currentMessages[i]);
                    }

                    _currentChatViewModel.UpdateStatusInfo();

                    IsLoadingMessagesHistory = false;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("_UpdateCurrentMessagesList failed: " + ex.Message);
                }
            }
            else //currentMessages.Any())
            {
                if (isConference)
                {
                    // load chat messages
                    MessagesGet getMessages = new MessagesGet(-1, id, currentMessages.Count, MESSAGES_COUNT, _UpdateCurrentMessagesList);
                    getMessages.Execute();
                }
                else
                {
                    // load messages of user
                    MessagesGet getMessages = new MessagesGet(id, -1, currentMessages.Count, MESSAGES_COUNT, _UpdateCurrentMessagesList);
                    getMessages.Execute();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isConference"></param>
        /// <returns></returns>
        public ChatViewModel GetChatViewModel(int id, bool isConference)
        {
            _currentChatId = id;
            _isCurrentConference = isConference;

            try
            {
                var currentMessages = _messagesCache.GetItems(id);

                string title = EntitiesHelpers.GetChatTitle(id, isConference);

                // Load only 30 first from cache.
                int finish = currentMessages.Count - MESSAGES_COUNT;
                if (finish < 0)
                    finish = 0;

                int count = MESSAGES_COUNT;
                if (currentMessages.Count < count)
                    count = currentMessages.Count;

                var filtered = new Message[count];// new List<Message>(MESSAGES_COUNT);
                for (int i = currentMessages.Count - 1, j = count - 1; i >= finish; i--, j--)
                    filtered[j] = currentMessages[i];

                _currentChatViewModel = new ChatViewModel(id, title, isConference, filtered, _imageCache);

                if (!currentMessages.Any()) // Get messages only first time for this chat.
                {
                    MessagesGet getMessages = null;
                    if (isConference)
                        getMessages = new MessagesGet(-1, id, 0, MESSAGES_COUNT, _UpdateCurrentMessagesList);
                    else
                        getMessages = new MessagesGet(id, -1, 0, MESSAGES_COUNT, _UpdateCurrentMessagesList);

                    getMessages.Execute();
                }
                else
                    _UpdateReadStatusOfMessages(currentMessages); // This method will be called in _UpdateCurrentMessagesList

                _currentChatViewModel.UpdateStatusInfo();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetChatViewModel failed: " + ex.Message);
            }

            return _currentChatViewModel;
        }

        public void CloseCurrentChat()
        {
            _currentChatId = -1;
            _currentChatViewModel = null;
        }

        public void UpdateMessagesById(string ids)
        {
            try
            {
                var op = new MessagesGetById(ids, messages =>
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        try
                        {
                            // Update UI first of all
                            if (_currentChatId != -1 && _currentChatViewModel != null)
                            {
                                // Get only currently visible on screen messages.
                                var currentMessages = messages.Where(x => x.Chatid == _currentChatId || ( x.Uid == _currentChatId && (x.Chatid == 0 || x.Chatid == -1))).ToList();

                                _UpdateCurrentMessagesList(false, currentMessages);
                            }

                            // Update all message queues in cache, if need
                            foreach (var message in messages)
                            {
                                if (message.Chatid > 0)
                                { // Update chat.
                                    var fromCache = _messagesCache.GetItems(message.Chatid);

                                    if (fromCache.Any())
                                        _messagesCache.AddItem(message.Chatid, message);
                                }
                                else
                                { // Update users dialog.
                                    var fromCache = _messagesCache.GetItems(message.Uid);

                                    if (fromCache.Any())
                                        _messagesCache.AddItem(message.Uid, message);
                                }

                                if (!message.IsRead && !_stateCounter.UnreadMids.Contains(message.Mid))
                                    _stateCounter.UnreadMids.Add(message.Mid);
                            }

                            // Get latest dialogs list.
                            UpdateDialogs();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("UpdateMessagesById -> MessagesGetById Callback failed: " + ex.Message);
                        }
                    });
                });
                op.Execute();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("UpdateMessagesById -> MessagesGetById failed: " + ex.Message);
            }
        }

        /// <summary>
        /// Clears data before logging out.
        /// </summary>
        public void ClearAllData()
        {
            try
            {
                _settings.AccessToken = string.Empty;
                _settings.Password = string.Empty;
                _settings.Secret = string.Empty;
                _settings.Ts = -1;
                _settings.UserId = string.Empty;
                _settings.UserName = string.Empty;

                _dialogsCache.Clear();
                _usersCache.Clear();
                _friendsCache.Clear();
                _messagesCache.Clear();
                _usersCache.Clear();

                _imageCache.Clear();

                var contactsCache = new ContactsCache();
                contactsCache.Clear();

                Dialogs.Clear();
                Friends.Clear();
                FriendsRequests.Clear();
                FriendsMutual.Clear();
                OtherUsers.Clear();
                Contacts.Clear();

                CurrentPhoneContact = null;

                MessagesToForward = string.Empty;
                AttachedLatitude = string.Empty;
                AttachedLongitude = string.Empty;

                _isInited = false;

                AttachmentPhotos.Clear();

                _stateCounter.CountOfRequests = 0;
                _stateCounter.UnreadMids.Clear();

                App.Current.PushNotifications.SwitchOff();

                App.Current.LastContactsSync = DateTime.MinValue;

                App.Current.LongPollService.TurnOff();

                CurrentUser.FullName = string.Empty;
                CurrentUser.Photo = DefaultAvatar;
            }
            catch(Exception ex)
            {
                Debug.WriteLine("ClearAllData failed: " + ex.Message);
            }
        }

        public void DeleteMessage(int mid)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    // Remove item from cache, and if need - remove message view model from UI.
                    if (_messagesCache.RemoveItem(mid) && _currentChatId != -1 && _currentChatViewModel != null)
                    {
                        var uiMessage = _currentChatViewModel.Messages.FirstOrDefault(x => x.Mid == mid);

                        if (uiMessage != null)
                        {
                            _currentChatViewModel.Messages.Remove(uiMessage);
                            _currentChatViewModel.UpdateStatusInfo();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("UpdatesService_MessageDeleted failed: " + ex.Message + System.Environment.NewLine + ex.StackTrace);
                }
            });
        }

        public void DeleteDialog(int id)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    var dialog = Dialogs.FirstOrDefault(x => x.Uid == id && !x.IsConference);

                    if (dialog != null)
                    {
                        _dialogsCache.RemoveItem(id);
                        Dialogs.Remove(dialog);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("DeleteDialog failed: " + ex.Message + System.Environment.NewLine + ex.StackTrace);
                }
            });
        }

        public void DeleteFriend(int id)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    var friend = Friends.FirstOrDefault(x => x.Uid == id);

                    if (friend != null)
                    {
                        _friendsCache.RemoveItem(id);
                        Friends.Remove(friend);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("DeleteFriend failed: " + ex.Message + System.Environment.NewLine + ex.StackTrace);
                }
            });
        }

        #endregion

        #region Friends private methods

        /// <summary>
        /// Pushes friends information to a public collection which will be used for UI Lists.
        /// </summary>
        private void _PushFriendsToView()
        {
            try
            {
                Friends.Clear();

                var friends = _friendsCache.GetItems();

                foreach (var friend in friends)
                {
                    string fileName = _GetFriendPhotoFileName(friend.Uid, friend.Photo);

                    // Try get from cache
                    BitmapImage image = null;
                    if (!string.IsNullOrEmpty(fileName))
                        image = _imageCache.GetItem(fileName);

                    string contactName = friend.ContactName != null ? friend.ContactName : string.Empty;

                    Friends.Add(new FriendViewModel(friend.Uid, friend.FullName, contactName, friend.FirstName, friend.LastName,
                        friend.IsOnline, friend.HintOrder, friend.Photo, image == null ? DefaultAvatar : image));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("_PushFriendsToView failed: " + ex.Message);
            }
        }

        /// <summary>
        /// Generates unique filename for user image.
        /// </summary>
        private string _GetFriendPhotoFileName(int id, string photo)
        {
            string fileName = string.Empty;

            try
            {
                if (!_friendsAndImages.ContainsKey(id))
                {
                    fileName = CommonHelper.DoDigest(photo);

                    _friendsAndImages.Add(id, fileName);
                }
                else
                {
                    fileName = _friendsAndImages[id];
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("_GetFriendPhotoFileName failed: " + ex.Message);
            }

            return fileName;
        }

        /// <summary>
        /// When avatar image is already loaded and is placed in cache (or in the storage) we may
        /// apply this image for specified user.
        /// </summary>
        private void _UpdateAvatar(int friendUid)
        {
            try
            {
                string fileName = string.Empty;

                if (_friendsAndImages.ContainsKey(friendUid))
                {
                    fileName = _friendsAndImages[friendUid];
                }
                else
                {
                    // Sometimes friends have the same default image, which means account is broken...
                    // To do not load these images second time, we just recalculate filenames (which will be same) and load image from cache.
                    var friends = _friendsCache.GetItems();

                    var friend = friends.FirstOrDefault(f => f.Uid == friendUid);

                    if (friend != null && !string.IsNullOrEmpty(friend.Photo))
                    {
                        fileName = CommonHelper.DoDigest(friend.Photo);
                    }
                }

                // Set image to account.
                if (!string.IsNullOrEmpty(fileName))
                {
                    if (friendUid == CurrentUser.UserInfo.Uid)
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            try
                            {
                                BitmapImage image = _imageCache.GetItem(fileName);

                                if (image != null)
                                    CurrentUser.Photo = image;
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("_UpdateAvatar failed in EntityService: " + ex.Message);
                            }
                        });
                    }
                    else
                    {
                        var friendVM = Friends.FirstOrDefault(f => f.Uid == friendUid);
                        // Sometimes messages may be sent by NON-FRIEND. In that case we may take photo from dialog too.
                        var dialogsVM = Dialogs.Where(d => d.Uid == friendUid && !d.IsConference);
                        var user = OtherUsers.FirstOrDefault(x => x.Uid == friendUid);

                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            try
                            {
                                BitmapImage image = _imageCache.GetItem(fileName);

                                if (image != null && image.PixelHeight > 0 && image.PixelWidth > 0)
                                {
                                    if (friendVM != null)
                                        friendVM.Photo = image;

                                    foreach (var dialog in dialogsVM)
                                    {
                                        dialog.Photo = image;
                                    }

                                    if (user != null)
                                        user.ImagePhoto = image;
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("_UpdateAvatar failed in EntityService: " + ex.Message);
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("_UpdateAvatar failed: " + ex.Message);
            }
        }

        /// <summary>
        /// Creates list of avatars to load for all current friends and current user.
        /// </summary>
        private List<AvatarLoadItem> _GetInitialAvatarsToLoad()
        {
            var itemsToLoad = new List<AvatarLoadItem>();
            var friends = _friendsCache.GetItems();

            foreach (var friend in friends)
            {
                string fileName = _GetFriendPhotoFileName(friend.Uid, friend.Photo); // Update friends photos only during first time initialization.

                if (_settings.IsFirstRun /* Do not spend time for File I/O operations first time */ ||
                    (!string.IsNullOrEmpty(fileName) && _imageCache.GetItem(fileName) == null))
                {
                    itemsToLoad.Add(new AvatarLoadItem(friend.Uid, friend.Photo, fileName));
                }
            }

            var users = _usersCache.GetItems();
            foreach (var info in users)
            {
                string fileName = _GetFriendPhotoFileName(info.Uid, info.Photo);

                if (_settings.IsFirstRun /* Do not spend time for File I/O operations first time */ ||
                    (!string.IsNullOrEmpty(fileName) && _imageCache.GetItem(fileName) == null))
                {
                    itemsToLoad.Add(new AvatarLoadItem(info.Uid, info.Photo, fileName));
                }
            }

            // Special case for current user.
            if (CurrentUser != null && CurrentUser.UserInfo != null)
            {
                string fileName = CommonHelper.DoDigest(CurrentUser.UserInfo.PhotoMedium); // Don't get from _GetFriendPhotoFileName to get updated new photo if need.

                if (!_friendsAndImages.ContainsKey(CurrentUser.UserInfo.Uid))
                    _friendsAndImages.Add(CurrentUser.UserInfo.Uid, fileName);
                else
                    _friendsAndImages[CurrentUser.UserInfo.Uid] = fileName;

                if (_settings.IsFirstRun || (!string.IsNullOrEmpty(fileName) && _imageCache.GetItem(fileName) == null))
                {
                    itemsToLoad.Add(new AvatarLoadItem(CurrentUser.UserInfo.Uid, CurrentUser.UserInfo.PhotoMedium, fileName));
                }
            }

            return itemsToLoad;
        }

        #endregion

        #region Dialogs private methods

        private void _ReinitializeCounters()
        {
            try
            {
                var op = new GetStatInfo((mids, requests) =>
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        try
                        {
                            _stateCounter.UnreadMids.Clear();

                            foreach (int id in mids)
                                _stateCounter.UnreadMids.Add(id);

                            _stateCounter.CountOfRequests = requests.Count;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("_ReinitializeCounters failed in EntityService: " + ex.Message);
                        }

                        _waitHandle.Set();
                    });
                });
                op.Execute();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("_ReinitializeCounters failed: " + ex.Message);
            }
        }

        private void _InitializeDialogsList()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    var cachedDialogs = _dialogsCache.GetItems();

                    if (cachedDialogs.Any())
                    {
                        _PushDialogsToView();

                        //int maxMid = cachedDialogs.Where(x => !x.IsConference).Max(y => y.ChatId); // Find max message Id in dialogs, since dialogs contains LATEST messages.
                        //App.Current.LongPollService.GetHistory(maxMid);

                        _waitHandle.Set();
                    }
                    else
                    {
                        // Get only and only if cache is empty, because initially we load only 100-200 dialogs until use scrolled down
                        // (so, probably, there may be situation where user never load all history).
                        var getDialogs = new ExecuteDialogs(0, _UpdateDialogsList);// new DialogsGet(DIALOGS_COUNT, 0, _friendsCache.GetItems(), _UpdateDialogsList);//
                        getDialogs.Execute();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("_InitializeDialogsList failed: " + ex.Message);
                    _waitHandle.Set();
                }
            });
        }

        private void _UpdateDialogsList(IList<Dialog> dialogs, IList<UserInfo> profiles)
        {
            ManualResetEvent waitHandle = new ManualResetEvent(false);

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    _AddDistinctUserInfo(profiles);

                    var cached = _dialogsCache.GetItems();

                    // Find dialogs which should be added or updated.
                    foreach (var dialog in dialogs)
                    {
                        Dialog current_cached = cached.FirstOrDefault(x => (x.Uid == dialog.Uid && !x.IsConference && !dialog.IsConference) ||
                            // Check for IsConference every time is need because Id may be Mid in case of non-conference dialog.
                            (x.ChatId == dialog.ChatId && x.IsConference && dialog.IsConference));

                        if (current_cached == null) // Completely new dialog.
                            _dialogsCache.AddItem(dialog);
                        else // Replace with latest one
                            _dialogsCache.RenewItem(current_cached, dialog);
                    }

                    _dialogsCache.Save();

                    _PushDialogsToView();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("EntityService _UpdateDialogsList failed: " + ex.Message);
                }
                waitHandle.Set();
            });

            waitHandle.WaitOne();

            _waitHandle.Set();
        }

        private void _AddDistinctUserInfo(IList<UserInfo> profiles)
        {
            var cache = _usersCache.GetItems();

            foreach (var info in profiles)
            {
                if (info.Uid != CurrentUser.UserInfo.Uid)
                {
                    var userInfo = OtherUsers.FirstOrDefault(x => x.Uid == info.Uid);
                    var friend = Friends.FirstOrDefault(x => x.Uid == info.Uid);
                    var cached = cache.FirstOrDefault(x => x.Uid == info.Uid);

                    if (userInfo == null && friend == null && cached == null)
                    {
                        _usersCache.AddItem(info);

                        OtherUsers.Add(info);
                    }
                }
            }

            _usersCache.Save();
        }

        /// <summary>
        /// Pushes dialogs information to a public collection which will be used for UI Lists.
        /// </summary>
        private void _PushDialogsToView()
        {
            Dialogs.Clear();

            // Show cached dialogs.
            var cachedDialogs = _dialogsCache.GetItems();
            foreach (var dialogEntry in cachedDialogs)
            {
                Dialogs.Add(dialogEntry);
            }

            // Update photos
            foreach (var dialog in Dialogs)
            {
                if (!dialog.IsConference)
                {
                    FriendViewModel friend = Friends.FirstOrDefault(x => x.Uid == dialog.Uid);

                    if (friend != null)
                    {
                        dialog.Photo = friend.Photo;
                    }
                    else
                    {
                        // Sometimes messages may be sent by NON-FRIEND. In that case we may take photo from dialog.
                        UserInfo info = OtherUsers.FirstOrDefault(x => x.Uid == dialog.Uid);

                        if (info != null)
                        {
                            string key = _GetFriendPhotoFileName(dialog.Uid, info.Photo);
                            var image = _imageCache.GetItem(key);

                            if (image != null)
                                dialog.Photo = image;
                        }
                    }
                }
                // During initialization we wait for loading all friend pictures,
                // so here we can generate pictures only if service already initialized.
                else if (_isInited)
                {
                    var key = _GenereateGroupChatAvatar(dialog);

                    if (key == string.Empty)
                    {
                        dialog.Photo = DefaultAvatar;
                    }
                    else
                    {
                        var image = _imageCache.GetItem(key);

                        if (image != null)
                            dialog.Photo = image;
                    }
                }
            }
        }

        #endregion

        #region Chat private methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dialog"></param>
        /// <returns></returns>
        private string _GenereateGroupChatAvatar(Dialog dialog)
        {
            string key = string.Empty;

            try
            {
                // Get all friends (and only friends) of this chat.
                var ids = dialog.GetLastUserIds();
                var friends = Friends.Where(f => ids.Contains(f.Uid)).ToList();
                var others = OtherUsers.Where(o => ids.Contains(o.Uid)).ToList();
                var images1 = friends.Select(x => x.Photo).ToList();
                var images2 = others.Select(x => x.ImagePhoto).ToList();

                images1.AddRange(images2);

                if (images1.Any())
                {
                    string filename = _GetFriendPhotoFileName(dialog.ChatId, dialog.ChatActive);
                    key = MosaicPictureGenerator.GenereateMosaicPicture(images1, filename);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("_GenereateGroupChatAvatar failed." + ex.Message);
            }

            return key;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messages"></param>
        private void _UpdateCurrentMessagesList(bool isLoadHistory, IList<Message> messages)
        {
            if (_currentChatId == -1 || _currentChatViewModel == null)
            {
                Debug.Assert(false); // Normal situation?
                return;
            }

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    if (isLoadHistory)
                    {
                        _messagesCache.AddItems(_currentChatId, messages);

                        _currentChatViewModel.UpdateAllMessages(_messagesCache.GetItems(_currentChatId));
                    }
                    else
                    {
                        //NOTE. Messages cache will never add copies of same Mids.
                        var added = _messagesCache.AddItemsEx(_currentChatId, messages);

                        foreach (var item in added)
                        {
                            _currentChatViewModel.AddMessageAndLoadPhotos(item);
                        }
                    }

                    _currentChatViewModel.UpdateStatusInfo();

                    _UpdateReadStatusOfMessages(messages);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("_UpdateCurrentMessagesList failed: " + ex.Message);
                }

                IsLoadingMessagesHistory = false;
            });
        }

        private void _UpdateReadStatusOfMessages(IList<Message> messages)
        {
            try
            {
                // Find out all messages which is new for me and mark it read since user see it in the UI.
                var unreadMessages = messages.Where(x => !x.IsRead);
                var unreadIds = unreadMessages.Select(y => y.Mid);
                if (unreadIds.Any())
                {
                    int count = unreadIds.Count();
                    string mids = CommonHelper.GetCommaDelimetedIntegersString(unreadIds.ToList());

                    var op = new MessagesMarkAsRead(mids, isOk =>
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            try
                            {
                                // Recount "unread" messages.
                                foreach (var mid in unreadIds)
                                {
                                    _stateCounter.UnreadMids.Remove(mid);
                                }

                                // Mark "read" appropriate dialogs
                                foreach (var message in unreadMessages)
                                {
                                    var dialog = Dialogs.FirstOrDefault(x => (x.ChatId == _currentChatId && x.IsConference) ||
                                                                                (x.Uid == _currentChatId && !x.IsConference));

                                    if (dialog != null)
                                        dialog.IsRead = true;
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("MessagesMarkAsRead callback failed:" + ex.Message);
                            }
                        });
                    });
                    op.Execute();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("_UpdateReadStatusOfMessages failed: " + ex.Message);
            }
        }

        /// <summary>
        /// Timer for switching on and off user status "Typing"
        /// </summary>
        private void _CreateTypingTimer()
        {
            if (_userTypingTimer != null)
            {
                _userTypingTimer.Dispose();
                _userTypingTimer = null;
            }

            _userTypingTimer = new Timer(state =>
            {
                if (_currentChatViewModel != null && _currentChatId != -1)
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        try
                        {
                            // Update status and action fields in model.
                            _currentChatViewModel.Action = string.Empty;
                            _currentChatViewModel.UpdateStatusInfo();

                            // Stop timer.
                            _userTypingTimer.Dispose();
                            _userTypingTimer = null;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("_CreateTypingTimer failed in EntityService: " + ex.Message);
                        }
                    });
                }
            }, null, 4000, -1); // After 5 sec notification will be repeated.
        }

        #endregion

        #region Private constants

        private const int MESSAGES_COUNT = 30; // Maximum on VK 100

        #endregion

        #region Private fields

        private DialogsCache _dialogsCache = null;
        private FriendsCache _friendsCache = null;
        private UsersCache _usersCache = null;
        private ImageCache _imageCache = null;
        private MessagesCache _messagesCache = null;

        private Settings _settings;

        private IDictionary<int, string> _friendsAndImages = new Dictionary<int, string>();

        private ManualResetEvent _waitHandle;

        // Chat related fields.
        private ChatViewModel _currentChatViewModel = null;
        private int _currentChatId = -1;
        private bool _isCurrentConference = false;

        private Timer _userTypingTimer = null;

        private StateCounter _stateCounter;

        private IDictionary<int, int> _flagsToImplementForMessages = new Dictionary<int, int>();

        private bool _isInited = false;

        private SoundEffect _soundEffect;

        #endregion
    }
}
