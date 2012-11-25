using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media.Imaging;
using SlXnaApp1.Cache;
using SlXnaApp1.Infrastructure;
using SlXnaApp1.Json;
using System.Windows;
using System.Globalization;
using SlXnaApp1.Api;

namespace SlXnaApp1.Entities
{
    public class ChatViewModel : INotifyPropertyChanged
    {
        #region Constructor

        public ChatViewModel(int id, string name, bool isConference, IList<Message> messagesHistory, ImageCache imageCache)
        {
            _id = id;
            _isConference = isConference;

            _name = string.Empty;
            if (!string.IsNullOrEmpty(name))
                _name = name.ToUpper();

            _imageCache = imageCache;
            Messages = new ObservableCollection<MessageViewModel>();

            _defaultAvatar = ResourceHelper.GetBitmap(@"/SlXnaApp1;component/Images/Photo_Placeholder.png");

            UpdateAllMessages(messagesHistory);
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

        public ObservableCollection<MessageViewModel> Messages { get; set; }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (_name != value)
                {
                    _name = value.ToUpper();
                    OnPropertyChanged("Name");
                }
            }
        }

        /// <summary>
        /// Is visible at the top, under Name (or Title) of the chat.
        /// </summary>
        public string Status
        {
            get
            {
                return _status;
            }
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged("Status");
                }
            }
        }

        /// <summary>
        /// At the bottom of the page: above the Message Text Box.
        /// </summary>
        public string Action
        {
            get
            {
                return _action;
            }
            set
            {
                if (_action != value)
                {
                    _action = value;
                    OnPropertyChanged("Action");
                }
            }
        }

        #endregion

        #region Public methods

        public void UpdateAllMessages(IList<Message> messagesToAdd)
        {
            try
            {
                var vm = _GetMessagesViewModel(messagesToAdd);

                Messages.Clear();
                foreach (var item in vm)
                    Messages.Add(item);

                _LoadPhotos();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("UpdateAllMessages failed: " + ex.Message);
            }
        }

        public void AddMessage(Message message)
        {
            try
            {
                var vm = _GetMessageViewModel(message);

                Messages.Add(vm);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("AddMessage failed: " + ex.Message);
            }
        }

        public void AddMessageAndLoadPhotos(Message message)
        {
            try
            {
                var vm = _GetMessageViewModel(message);

                Messages.Add(vm);

                _LoadPhotos();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("AddMessage failed: " + ex.Message);
            }
        }

        public void AddOldMessage(Message message)
        {
            try
            {
                var vm = _GetMessageViewModel(message);
                Messages.Insert(0, vm);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("AddOldMessage failed: " + ex.Message);
            }
        }

        public void UpdateStatusInfo()
        {
            if (_isConference)
            {
                if (Messages.Any())
                {
                    if (Messages.Count == 2 || Messages.Count == 3)
                        Status = String.Format(AppResources.TwoMemebersFormatMessage, _usersList.Count);
                    else
                        Status = String.Format(AppResources.NumberMemebersFormatMessage, _usersList.Count); //AppResources.NumberMessagesInChat, Messages.Count
                }
                else
                    Status = AppResources.NoMessagesLabel;
            }
            else
            {
                var friend = App.Current.EntityService.Friends.FirstOrDefault(x => x.Uid == _id);

                if (friend != null)
                    _UpdateStatus(friend.IsOnline);
                else
                {
                    // Sometimes messages may be sent by NON-FRIEND.
                    var user = App.Current.EntityService.OtherUsers.FirstOrDefault(x => x.Uid == _id);
                    if (user != null)
                        _UpdateStatus(user.IsOnline);
                }
            }
        }

        #endregion

        #region Private methods

        private BitmapImage _GetMessageAvatarPhoto(Message message)
        {
            Debug.Assert(message != null);

            try
            {
                // WARNING. Chat Id was not returned by MessagesHistory method, because we give chat_id to it. Probably, they thought we don't need this...
                //if (message.Chatid > 0)
                if (_isConference)
                {
                    var friend = App.Current.EntityService.Friends.FirstOrDefault(x => x.Uid == message.Uid);

                    if (friend != null)
                        return friend.Photo;
                    else
                    {
                        // Sometimes messages may be sent by NON-FRIEND. In that case we may take photo from dialog.
                        var user = App.Current.EntityService.OtherUsers.FirstOrDefault(x => x.Uid == message.Uid);

                        if (user != null)
                            return user.ImagePhoto;
                        else
                            return _defaultAvatar; // Actually, this is fail (cuz friends loads during initializing). But we need stability for any cases...
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("_GetMessageAvatarPhoto failed: " + ex.Message);
            }

            return null; // No need photo for the dialog
        }

        private ObservableCollection<AttachmentViewModel> _GetAttachmentsViewModel(Message message)
        {
            Debug.Assert(message != null);

            var items = new ObservableCollection<AttachmentViewModel>();

            try
            {
                if (message.GeoLocation != null)
                {
                    //NumberFormatInfo provider = new NumberFormatInfo();provider.
                    string uri = String.Format(AppResources.GeolocationMapUriFormatMessage,
                        message.GeoLocation.Latitude.ToString("0.00", CultureInfo.InvariantCulture),
                        message.GeoLocation.Longitude.ToString("0.00", CultureInfo.InvariantCulture));
                    var avm = new AttachmentViewModel(message.Mid, -1, -1, AttachmentType.Location, uri, AppResources.LocContentLabel, _defaultAvatar, null, null);

                    // Try to load image from cache.
                    string filename = CommonHelper.DoDigest(avm.Uri);
                    BitmapImage image = _imageCache.GetItem(filename);
                    if (image != null)
                        avm.AttachPhoto = image;
                    else // ...if it doesn't exists - load from web.
                        _photosToLoad.Add(avm, new AvatarLoadItem(_photosToLoad.Count, uri, filename));

                    items.Add(avm);
                }


                if (message.FwdMessages != null && message.FwdMessages.Any())
                    items.Add(new AttachmentViewModel(message.Mid, -1, -1, AttachmentType.ForwardMessage, null, "", null, _GetMessagesViewModel(message.FwdMessages), null));

                if (message.Attachments != null)
                {
                    foreach (var attach in message.Attachments)
                    {
                        AttachmentViewModel avm = null;

                        if (attach.DocAttachment != null) // Todo. Use special document photo here?
                        {
                            items.Add(new AttachmentViewModel(message.Mid, attach.DocAttachment.OwnerId, attach.DocAttachment.Did, AttachmentType.Document,
                                attach.DocAttachment.Url, AppResources.DocumentContentLabel, _defaultAvatar, null, null));
                        }
                        else if (attach.VideoAttachment != null)
                        {
                            avm = new AttachmentViewModel(message.Mid, attach.VideoAttachment.OwnerId, attach.VideoAttachment.Vid, AttachmentType.Video,
                                attach.VideoAttachment.Image, AppResources.VideoContentLabel, _defaultAvatar, null, null, attach.VideoAttachment.Title,
                                string.Empty, attach.VideoAttachment.Duration);
                        }
                        else if (attach.AudioAttachment != null)
                        {
                            avm = new AttachmentViewModel(message.Mid, attach.AudioAttachment.OwnerId, attach.AudioAttachment.Aid, AttachmentType.Audio,
                                attach.AudioAttachment.Url, AppResources.AudioContentLabel, null, null, null, attach.AudioAttachment.Title,
                                attach.AudioAttachment.Author, attach.AudioAttachment.Duration);
                        }
                        else if (attach.PhotoAttachment != null)
                        {
                            avm = new AttachmentViewModel(message.Mid, attach.PhotoAttachment.OwnerId, attach.PhotoAttachment.Pid, AttachmentType.Photo,
                                attach.PhotoAttachment.SourceBig, AppResources.PhotoContentLabel, _defaultAvatar, null, null);
                        }
                        else
                            Debug.Assert(false);

                        if (avm != null)
                        {
                            if (avm.Type != AttachmentType.Audio)
                            {
                                // Try to load image from cache.
                                string filename = CommonHelper.DoDigest(avm.Uri);
                                BitmapImage image = _imageCache.GetItem(filename);
                                if (image != null)
                                    avm.AttachPhoto = image;
                                else // ...if it doesn't exists - load from web.
                                    _photosToLoad.Add(avm, new AvatarLoadItem(_photosToLoad.Count, avm.Uri, filename));
                            }

                            // Add to current items
                            items.Add(avm);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("_GetAttachmentsViewModel failed: " + ex.Message);
            }

            return items;
        }

        private IList<MessageViewModel> _GetMessagesViewModel(IList<Message> messages)
        {
            var vmMessages = new List<MessageViewModel>();

            try
            {
                foreach (var message in messages)
                {
                    vmMessages.Add(_GetMessageViewModel(message));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("_GetMessagesViewModel failed: " + ex.Message);
            }

            return vmMessages;//.OrderBy(x => x.DateTime).ToList();
        }

        private MessageViewModel _GetMessageViewModel(Message message)
        {
            var messageVM = new MessageViewModel(
                message.Uid,
                message.Mid,
                CommonHelper.GetFormattedMessage(message.Body),
                CommonHelper.GetFormattedDate(message.Date),
                message.IsOut,
                true,
                message.IsRead,
                message.IsDeleted,
                _GetMessageAvatarPhoto(message),
                _GetAttachmentsViewModel(message));

            if (!_usersList.Contains(message.Uid))
                _usersList.Add(message.Uid); // To know count of unique users.

            return messageVM;
        }

        private void _LoadPhotos()
        {
            try
            {
                var service = new AsyncAvatarsLoader();

                // From map of attachment view model and load items get collection of avatar load items...
                var photosToLoad = _photosToLoad.Select(x => x.Value);

                if (photosToLoad.Any())
                    service.LoadAvatars(photosToLoad.ToList(), _UpdatePhoto, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("LoadPhotos failed " + ex.Message);
            }
        }

        private void _UpdatePhoto(int id)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    var loadItem = _photosToLoad.FirstOrDefault(x => x.Value.Uid == id); // We can use id just like [id]...but this is really bad =)

                    if (loadItem.Key != null && loadItem.Value != null)
                    {
                        BitmapImage image = _imageCache.GetItem(loadItem.Value.FileName);

                        if (image != null && image.PixelHeight > 0 && image.PixelWidth > 0)
                        {
                            loadItem.Key.AttachPhoto = image;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("_UpdatePhoto failed on ChatViewModel: " + ex.Message);
                }
            });
        }

        private void _UpdateStatus(bool isOnline)
        {
            if (isOnline)
                Status = AppResources.Online;
            else
            {
                Status = AppResources.Offline; // For time.

                var op = new MessagesGetLastActivity(_id, status =>
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        try
                        {
                            if (status != null)
                            {
                                string formattedTime = CommonHelper.GetFormattedStatusTime(status.Time);

                                Status = string.Format(AppResources.WasOnlineFormatMessage, formattedTime);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("_UpdatePhoto failed on ChatViewModel: " + ex.Message);
                        }
                    });
                });
                op.Execute();
            }
        }

        #endregion

        #region Private fields

        private static BitmapImage _defaultAvatar;

        private string _name;
        private string _status;
        private string _action;
        private IDictionary<AttachmentViewModel, AvatarLoadItem> _photosToLoad = new Dictionary<AttachmentViewModel, AvatarLoadItem>();
        private int _id;
        private ImageCache _imageCache;

        // To know count of unique users.
        private List<int> _usersList = new List<int>();

        private bool _isConference;

        #endregion
    }
}
