using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Device.Location;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using SlXnaApp1.Api;
using SlXnaApp1.Entities;
using SlXnaApp1.Infrastructure;
using SlXnaApp1.Json;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;

namespace SlXnaApp1.Views
{
    public partial class ChatPage : PhoneApplicationPage
    {
        #region Constructor

        public ChatPage()
        {
            InitializeComponent();

            _currentAttachmentsRequest = string.Empty;

            MessageTextBox.Text = AppResources.WriteMessageHint;
            MessageTextBox.Foreground = App.Current.GrayBrush;

            AudioPlayer.CurrentStateChanged += AudioPlayer_CurrentStateChanged;
            AudioPlayer.MediaEnded += AudioPlayer_MediaEnded;
            AudioPlayer.DownloadProgressChanged += AudioPlayer_DownloadProgressChanged;

            MessageTextBox.TextChanged += MessageTextBox_TextChanged;
        }

        #endregion

        #region Event handlers

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string id = string.Empty;

            _UpdateApplicationBar();

            try
            {
                if (NavigationContext.QueryString.TryGetValue("id", out id))
                {
                    _id = Convert.ToInt32(id);
                    _model = App.Current.EntityService.GetChatViewModel(_id, false);

                    this.DataContext = (object)_model;

                    _ScrollIntoView();

                    _model.Messages.CollectionChanged += Messages_CollectionChanged;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("OnNavigatedTo in ChatPage failed: " + ex.Message);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            App.Current.EntityService.CloseCurrentChat();
        }

        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            App.Current.EntityService.CloseCurrentChat();

            App.Current.EntityService.AttachedLatitude = string.Empty;
            App.Current.EntityService.AttachedLongitude = string.Empty;

            App.Current.EntityService.AttachmentPhotos.Clear();
        }

        private void ResendSelectedMessage_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            MessageViewModel mes = (sender as TextBlock).DataContext as MessageViewModel;
            _currentOutMessage = mes.Body;
            _SendSelectedMessage();
        }

        private void MessageTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            MessageTextBox.Text = string.Empty;
            MessageTextBox.Foreground = App.Current.BlackBrush;

            var op = new MessagesSetActivity(_id, -1, isOk => { /* Do nothing */ });
            op.Execute();
            //TODO To repeat every 6 sec.
        }

        private void MessageTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            MessageTextBox.Text = AppResources.WriteMessageHint;
            MessageTextBox.Foreground = App.Current.GrayBrush;
        }

        private void Messages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (AudioPlayer.CurrentState == MediaElementState.Playing)
            {
                AudioPlayer.Stop();
                _currentTrack = null;
            }

            if (!App.Current.EntityService.IsLoadingMessagesHistory)
                _ScrollIntoView();
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            // If user didn't type anything we need to remove our fake hint
            if (MessageTextBox.Text == AppResources.WriteMessageHint)
                _currentOutMessage = string.Empty;
            else
                _currentOutMessage = MessageTextBox.Text;

            _SendSelectedMessage();
        }

        private void ManageAttachmentsButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri(@"/Views/AttachmentsPage.xaml", UriKind.Relative));
        }

        private void AddPhotoButton_Click(object sender, EventArgs e)
        {
            if (App.Current.EntityService.IsFullyInitialized)
            {
                App.Current.PhotoGallery.Completed += photoGallery_Completed;
                App.Current.PhotoGallery.Show();
            }
            else
            {
                MessageBox.Show(AppResources.AppIsLoadingItems);
            }
        }

        private void RemoveDialog_Click(object sender, EventArgs e)
        {
            _RemoveCurrentDialog();
        }

        private void SelectMessagesButton_Click(object sender, EventArgs e)
        {
            MessagesPanel.SelectionMode = SelectionMode.Multiple;

            MessagesPanel.SelectionChanged += onSelectionChanged;

            _CreateBar2();
        }

        private void onSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                MessageViewModel mes = (MessageViewModel)e.AddedItems[0];
                mes.IsSelected = true;
                ((ApplicationBarIconButton)this.ApplicationBar.Buttons[1]).IsEnabled = true;
            }
            else if (e.RemovedItems.Count > 0)
            {
                MessageViewModel mes = (MessageViewModel)e.RemovedItems[0];
                mes.IsSelected = false;
            }
        }

        private void photoGallery_Completed(object sender, PhotoResult e)
        {
            try
            {
                if (e.ChosenPhoto != null)
                {
                    if (!App.Current.EntityService.AttachmentPhotos.ContainsKey(e.OriginalFileName))
                    {
                        App.Current.EntityService.AttachmentPhotos.Add(e.OriginalFileName, e.ChosenPhoto);

                        _UpdateApplicationBar();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("photoGallery_Completed failed and couched in UI! WTF! " + ex.Message);
            }
        }

        private void AddLocationButton_Click(object sender, EventArgs e)
        {
            GlobalIndicator.Instance.Text = AppResources.GettingLocation;
            GlobalIndicator.Instance.IsLoading = true;

            try
            {
                var watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);

                if (watcher.Permission == GeoPositionPermission.Granted)
                {
                    watcher.MovementThreshold = 100; // in meters
                    watcher.StatusChanged += (s, ee) =>
                    {
                        if (ee.Status == GeoPositionStatus.Disabled)
                            MessageBox.Show(AppResources.GPSNotEnabled);
                    };

                    if (!watcher.TryStart(true, TimeSpan.FromSeconds(5)))
                    {
                        MessageBox.Show(AppResources.GPSNotEnabled);
                    }
                    else
                    {
                        GeoCoordinate coord = watcher.Position.Location;

                        if (!coord.IsUnknown)
                        {
                            App.Current.EntityService.AttachedLatitude = coord.Latitude.ToString();
                            App.Current.EntityService.AttachedLongitude = coord.Longitude.ToString();

                            MessageBox.Show(AppResources.LocationAttachmentDescription);

                            _UpdateSendButtonState();
                        }
                    }
                }

                GlobalIndicator.Instance.Text = string.Empty;
                GlobalIndicator.Instance.IsLoading = false;
            }
            catch (Exception)
            {
                GlobalIndicator.Instance.Text = string.Empty;
                GlobalIndicator.Instance.IsLoading = false;
            }
        }

        private void PlayVideo_Tap(object sender, EventArgs e)
        {
            Button cur = sender as Button;

            if (cur != null)
            {
                AttachmentViewModel current = cur.DataContext as AttachmentViewModel;

                if (current != null)
                {
                    // I JUST DON'T UNDERSTAND WHERE TO GET LINK TO THE VIDEO!!! SO OPEN IN BROWSER.

                    //VideoGet op = new VideoGet(current.OwnerId.ToString() + "_" + current.Aid.ToString() , current.OwnerId,
                    //    link =>
                    //    {
                            try
                            {
                                string link = "http://vk.com/video" + current.OwnerId.ToString() + "_" + current.Aid.ToString();
                                WebBrowserTask task = new WebBrowserTask();
                                task.Uri = new Uri(link);
                                task.Show();
                                //MediaPlayerLauncher mpLauncherTask = new MediaPlayerLauncher();
                                //mpLauncherTask.Media = new Uri(link, UriKind.Absolute);
                                //mpLauncherTask.Controls = MediaPlaybackControls.All;
                                //mpLauncherTask.Location = MediaLocationType.Data;
                                //mpLauncherTask.Orientation = MediaPlayerOrientation.Portrait;
                                //mpLauncherTask.Show();
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("PlayVideo_Tap failed: " + ex.Message);
                            }
                    //    });
                    //op.Execute();
                }
            }
        }

        private void PlayAudio_Tap(object sender, EventArgs e)
        {
            Button cur = sender as Button;

            if (cur != null)
            {
                AttachmentViewModel currentTrack = cur.DataContext as AttachmentViewModel;

                if (currentTrack != null)
                {
                    if (AudioPlayer.CurrentState == MediaElementState.Playing)
                    {
                        AudioPlayer.Stop();

                        if (_currentTrack.Mid == currentTrack.Mid)
                        {
                            Image content = cur.Content as Image;
                            if (null != content)
                                content.Source = ResourceHelper.GetBitmap(@"/SlXnaApp1;component/Images/Play.png");

                            _currentPlayButton = null;
                            _currentTrack = null;
                        }
                        else // Handle the case when user don't stop current audio, but start another one...
                        {
                            Image content = cur.Content as Image;
                            if (null != content)
                                content.Source = ResourceHelper.GetBitmap(@"/SlXnaApp1;component/Images/Pause.png");

                            Image contentOld = _currentPlayButton.Content as Image;
                            if (null != content)
                                contentOld.Source = ResourceHelper.GetBitmap(@"/SlXnaApp1;component/Images/Play.png");

                            _currentTrack = currentTrack;
                            AudioPlayer.Source = new Uri(_currentTrack.Uri, UriKind.Absolute);
                            //_currentTrack.Progress = 0;
                            AudioPlayer.Play();

                            _currentPlayButton = cur;
                        }
                    }
                    else
                    {
                        Image content = cur.Content as Image;
                        if (null != content)
                            content.Source = ResourceHelper.GetBitmap(@"/SlXnaApp1;component/Images/Pause.png");

                        _currentTrack = currentTrack;
                        AudioPlayer.Source = new Uri(_currentTrack.Uri, UriKind.Absolute);
                        //_currentTrack.Progress = 0;
                        AudioPlayer.Play();

                        _currentPlayButton = cur;
                    }
                }
            }
        }

        private void AudioPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (_currentPlayButton != null)
            {
                Image content = _currentPlayButton.Content as Image;
                if (null != content)
                    content.Source = ResourceHelper.GetBitmap(@"/SlXnaApp1;component/Images/Play.png");
            }
        }

        private void AudioPlayer_DownloadProgressChanged(object sender, RoutedEventArgs e)
        {
            MediaElement me = sender as MediaElement;

            if (me != null && _currentTrack != null)
            {
                _currentTrack.LoadingProgress = me.DownloadProgress * 100.0;
            }
        }

        private void AudioPlayer_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            MediaElement me = sender as MediaElement;

            if (me != null)
            {
                if (me.CurrentState == MediaElementState.Playing)
                {
                    double duration = AudioPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                    ThreadPool.QueueUserWorkItem(o =>
                    {
                        while (_currentTrack != null)
                        {
                            Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                try
                                {
                                    if (_currentTrack != null)
                                        _currentTrack.Progress = AudioPlayer.Position.TotalSeconds * 100 / duration;
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine("AudioPlayer_CurrentStateChanged failed in ChatPage: " + ex.Message);
                                }
                            });
                            Thread.Sleep(0);
                        }
                    });
                }
            }
        }

        /// <summary>
        /// WORKAROUND. To show typing text in its current place.
        /// </summary>
        private void MessageTextBox_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            // if cursor is at the end of the text, scroll the ScrollViewer to the end 
            if (MessageTextBox.SelectionStart == MessageTextBox.Text.Length)
            {
                scroller.ScrollToVerticalOffset(MessageTextBox.ActualHeight);
            } 
        }
        
        /// <summary>
        /// Load more messages if user scrolled to the last available message and want to see more.
        /// </summary>
        private void MessagesPanel_ManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            if (_model != null && _model.Messages.Count > 0 &&
                WindowsPhoneHelpers.IsFirstItemVisible(MessagesPanel))
            {
                App.Current.EntityService.LoadMoreMessages(_id, false);
            }
        }

        /// <summary>
        /// Adds selected messages to fwd list for next sending.
        /// </summary>
        private void ForwardButton_Click(object sender, EventArgs e)
        {
            // Put to the attachments...
            var str = string.Empty;
            if (MessagesPanel.SelectedItems.Count > 0)
            {
                foreach (var o in MessagesPanel.SelectedItems)
                {
                    var mes = o as MessageViewModel;

                    if (mes != null)
                        str += mes.Mid.ToString() + ",";
                }
            }
            else
            {
                var selectedItem = (sender as MenuItem).DataContext as MessageViewModel;

                if (selectedItem != null)
                    str = selectedItem.Mid.ToString();
            }

            str = str.TrimEnd(',');

            App.Current.EntityService.MessagesToForward = str;

            if (App.Current.EntityService.MessagesToForward != string.Empty)
            {
                MessageBox.Show(AppResources.ForwardMessagesDescription);

                NavigationService.Navigate(new Uri(@"/Views/FriendsPage.xaml", UriKind.Relative));
            }

            _ResetSelectionMode();
        }

        /// <summary>
        /// 
        /// </summary>
        private void RemoveButton_Click(object sender, EventArgs e)
        {
            // Collect
            var str = string.Empty;
            var mids = new List<int>();
            if (MessagesPanel.SelectedItems.Count > 0)
            {
                foreach (var o in MessagesPanel.SelectedItems)
                {
                    var mes = o as MessageViewModel;

                    if (mes != null)
                    {
                        str += mes.Mid.ToString() + ",";
                        mids.Add(mes.Mid);
                    }
                }

                str = str.TrimEnd(',');
            }
            else
            {
                var selectedItem = (sender as MenuItem).DataContext as MessageViewModel;

                if (selectedItem != null)
                {
                    str = selectedItem.Mid.ToString();
                    mids.Add(selectedItem.Mid);
                }
            }

            if (!string.IsNullOrEmpty(str))
            {
                var op = new MessagesDelete(str, results =>
                {
                    if (results != null && results.Any())
                    {
                        try
                        {
                            foreach (var result in results)
                            {
                                if (result.Value == 1)
                                    App.Current.EntityService.DeleteMessage(Convert.ToInt32(result.Key));
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("RemoveButton_Click -> MessagesDelete failed: " + ex.Message);
                        }
                    }
                });
                op.Execute();
            }

            _ResetSelectionMode();
        }

        /// <summary>
        /// Copies some details of message into clipboard.
        /// </summary>
        private void CopyButton_Click(object sender, EventArgs e)
        {
            var item = (sender as MenuItem).DataContext as MessageViewModel;

            if (item != null)
            {
                // Body
                if (!string.IsNullOrEmpty(item.Body))
                {
                    Clipboard.SetText(item.Body);
                }
                // Attachment description
                else if (item.Attachments != null && item.Attachments.Any())
                {
                    if (item.Attachments[0].FwdMessages != null && item.Attachments[0].FwdMessages.Any())
                    {
                        // Copy all fwd messages with New Lines splitter
                        string body = string.Empty;
                        foreach (var mes in item.Attachments[0].FwdMessages)
                        {
                            body += mes.Body + Environment.NewLine;
                        }
                        Clipboard.SetText(body);
                    }
                    else
                    {
                        //...and uri
                        Clipboard.SetText(item.Attachments[0].Description + item.Attachments[0].Uri);
                    }
                }
                else
                {
                    // At least Mid we let user to copy =)
                    Clipboard.SetText(item.Mid.ToString());
                }
            }

            _ResetSelectionMode();
        }

        /// <summary>
        /// Just cancels selecting messages.
        /// </summary>
        private void CancelButton_Click(object sender, EventArgs e)
        {
            _ResetSelectionMode();
        }

        /// <summary>
        /// Makes text box enabled or disabled.
        /// </summary>
        private void MessageTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _UpdateSendButtonState();
        }

        private void Image_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                AttachmentViewModel obj = ((FrameworkElement)sender).DataContext as AttachmentViewModel;

                if (obj != null && obj.AttachPhoto != null)
                {
                    PopupImage.Source = obj.AttachPhoto;
                    Popup.IsOpen = true;
                }
            }
            catch
            {
            }
        }

        // Handle the back key button to close the popup
        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (Popup.IsOpen)
            {
                Popup.IsOpen = false;
                e.Cancel = true;
            }
        }

        #endregion

        #region Private method

        private void _UpdateSendButtonState()
        {
            var cur = this.ApplicationBar.Buttons[0] as ApplicationBarIconButton;

            if (cur != null)
            {
                if ((MessageTextBox.Text != string.Empty && MessageTextBox.Text != AppResources.WriteMessageHint) ||
                    !string.IsNullOrEmpty(App.Current.EntityService.MessagesToForward) ||
                    (!string.IsNullOrEmpty(App.Current.EntityService.AttachedLatitude) && !string.IsNullOrEmpty(App.Current.EntityService.AttachedLongitude)) ||
                    App.Current.EntityService.AttachmentPhotos.Any())
                {
                    cur.IsEnabled = true;
                }
                else
                {
                    cur.IsEnabled = false;
                }
            }
        }

        /// <summary>
        /// Moves list box to last item.
        /// </summary>
        private void _ScrollIntoView()
        {
            MessagesPanel.UpdateLayout();

            object item = MessagesPanel.Items.LastOrDefault();

            if (item != null)
                MessagesPanel.ScrollIntoView(item);
        }

        private void _UpdateApplicationBar()
        {
            if (App.Current.EntityService.AttachmentPhotos.Any())
                _CreateBar3();
            else
                _CreateBar();

            _UpdateSendButtonState();
        }

        /// <summary>
        /// Resets view after user multiple messages selecting mode.
        /// </summary>
        private void _ResetSelectionMode()
        {
            foreach (var mes in _model.Messages)
                if (mes.IsSelected)
                    mes.IsSelected = false;

            MessagesPanel.SelectionMode = SelectionMode.Single;
            MessagesPanel.SelectionChanged -= onSelectionChanged;

            MessagesPanel.SelectedIndex = -1;

            _UpdateApplicationBar();
        }

        /// <summary>
        /// Creates default bar.
        /// </summary>
        private void _CreateBar()
        {
            this.ApplicationBar = new ApplicationBar();

            var appBarButton = new ApplicationBarIconButton(
                new Uri("/Images/Appbar_Icons/appbar.send.text.rest.png", UriKind.Relative));
            appBarButton.Text = AppResources.SendButton;
            appBarButton.IsEnabled = false;
            this.ApplicationBar.Buttons.Add(appBarButton);
            appBarButton.Click += new EventHandler(SendButton_Click);

            var appBarButton1 = new ApplicationBarIconButton(
                new Uri("/Images/Appbar_Icons/appbar.feature.camera.rest.png", UriKind.Relative));
            appBarButton1.Text = AppResources.ChoosePhotoButton;
            this.ApplicationBar.Buttons.Add(appBarButton1);
            appBarButton1.Click += new EventHandler(AddPhotoButton_Click);

            var appBarButton2 = new ApplicationBarIconButton(
                new Uri("/Images/Appbar_Icons/appbar.checkin.rest.png", UriKind.Relative));
            appBarButton2.Text = AppResources.ShareLocationButton;
            this.ApplicationBar.Buttons.Add(appBarButton2);
            appBarButton2.Click += new EventHandler(AddLocationButton_Click);

            if (!string.IsNullOrEmpty(App.Current.EntityService.AttachedLatitude) &&
                !string.IsNullOrEmpty(App.Current.EntityService.AttachedLongitude))
            {
                appBarButton2.IsEnabled = false;
            }

            var appBarButton3 = new ApplicationBarIconButton(
                new Uri("/Images/Appbar_Icons/appbar.manage.rest.png", UriKind.Relative));
            appBarButton3.Text = AppResources.SelectButton;
            this.ApplicationBar.Buttons.Add(appBarButton3);
            appBarButton3.Click += new EventHandler(SelectMessagesButton_Click);

            var appBarMenuItem = new ApplicationBarMenuItem(AppResources.RemoveDialog);
            this.ApplicationBar.MenuItems.Add(appBarMenuItem);
            appBarMenuItem.Click += RemoveDialog_Click;
        }

        /// <summary>
        /// Creates app bar for multiple messages selection mode.
        /// </summary>
        private void _CreateBar2()
        {
            this.ApplicationBar = new ApplicationBar();

            var appBarButton = new ApplicationBarIconButton(
                new Uri("/Images/Appbar_Icons/appbar.forward.rest.png", UriKind.Relative));
            appBarButton.Text = AppResources.ForwardButton;
            this.ApplicationBar.Buttons.Add(appBarButton);
            appBarButton.Click += ForwardButton_Click;

            var appBarButton1 = new ApplicationBarIconButton(
                new Uri("/Images/Appbar_Icons/appbar.delete.rest.png", UriKind.Relative));
            appBarButton1.Text = AppResources.DeleteButton;
            appBarButton1.IsEnabled = false;
            this.ApplicationBar.Buttons.Add(appBarButton1);
            appBarButton1.Click += RemoveButton_Click;

            var appBarButton2 = new ApplicationBarIconButton(
                new Uri("/Images/Appbar_Icons/appbar.cancel.rest.png", UriKind.Relative));
            appBarButton2.Text = AppResources.CancelButton;
            this.ApplicationBar.Buttons.Add(appBarButton2);
            appBarButton2.Click += CancelButton_Click;
        }

        /// <summary>
        /// Creates bar with buttons for view with attachments.
        /// </summary>
        private void _CreateBar3()
        {
            this.ApplicationBar = new ApplicationBar();

            var appBarButton = new ApplicationBarIconButton(
                new Uri("/Images/Appbar_Icons/appbar.send.text.rest.png", UriKind.Relative));
            appBarButton.Text = AppResources.SendButton;
            appBarButton.IsEnabled = false;
            this.ApplicationBar.Buttons.Add(appBarButton);
            appBarButton.Click += new EventHandler(SendButton_Click);

            var appBarButton1 = new ApplicationBarIconButton(
                new Uri(CommonHelper.GetUriOfAttachmentIcon(), UriKind.Relative));
            appBarButton1.Text = AppResources.ChoosePhotoButton;
            this.ApplicationBar.Buttons.Add(appBarButton1);
            appBarButton1.Click += new EventHandler(ManageAttachmentsButton_Click);

            var appBarButton3 = new ApplicationBarIconButton(
                new Uri("/Images/Appbar_Icons/appbar.manage.rest.png", UriKind.Relative));
            appBarButton3.Text = AppResources.SelectButton;
            this.ApplicationBar.Buttons.Add(appBarButton3);
            appBarButton3.Click += new EventHandler(SelectMessagesButton_Click);

            var appBarMenuItem = new ApplicationBarMenuItem(AppResources.RemoveDialog);
            this.ApplicationBar.MenuItems.Add(appBarMenuItem);
            appBarMenuItem.Click += RemoveDialog_Click;
        }

        /// <summary>
        /// Callback after message sending finished (either successfully or not).
        /// </summary>
        private void _MessageSent(int mid)
        {
            var fake = _model.Messages.FirstOrDefault(m => m.Mid == Int32.MaxValue);

            // MESSAGE NOT SENT
            if (mid == -1)
            {
                // Add FAKE message until it will be completely sent.
                Dispatcher.BeginInvoke(() =>
                {
                    if (fake == null)
                    {
                        try
                        {
                            // If wasn't added before.
                            if (!string.IsNullOrEmpty(_currentOutMessage))
                            {
                                _model.Messages.Add(new MessageViewModel(App.Current.EntityService.CurrentUser.UserInfo.Uid,
                                    Int32.MaxValue, _currentOutMessage, string.Empty,
                                    true, false, false, false, null, new ObservableCollection<AttachmentViewModel>()));
                            }
                            else if (!string.IsNullOrEmpty(_currentAttachmentsRequest))
                            {
                                _model.Messages.Add(new MessageViewModel(App.Current.EntityService.CurrentUser.UserInfo.Uid,
                                    Int32.MaxValue, _currentAttachmentsRequest, string.Empty,
                                    true, false, false, false, null, new ObservableCollection<AttachmentViewModel>()));
                            }
                            else if (!string.IsNullOrEmpty(App.Current.EntityService.AttachedLatitude) &&
                                    !string.IsNullOrEmpty(App.Current.EntityService.AttachedLongitude))
                            {
                                _model.Messages.Add(new MessageViewModel(App.Current.EntityService.CurrentUser.UserInfo.Uid,
                                    Int32.MaxValue, App.Current.EntityService.AttachedLatitude + "," + App.Current.EntityService.AttachedLongitude,
                                    string.Empty, true, false, false, false, null, new ObservableCollection<AttachmentViewModel>()));
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("_MessageSent failed in ChatPage: " + ex.Message);
                        }
                    }

                    GlobalIndicator.Instance.Text = string.Empty;
                    GlobalIndicator.Instance.IsLoading = false;
                });

                Debug.WriteLine("Message send failed.");
            }
            // MESSAGE SENT
            else
            {
                // Remove FAKE message if it is
                Dispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        if (fake != null)
                            _model.Messages.Remove(fake);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("_MessageSent failed in ChatPage: " + ex.Message);
                    }
                });

                App.Current.EntityService.AttachedLatitude = string.Empty;
                App.Current.EntityService.AttachedLongitude = string.Empty;
                App.Current.EntityService.MessagesToForward = string.Empty;
                App.Current.EntityService.AttachmentPhotos.Clear();

                Debug.WriteLine("Message successfully sent " + mid.ToString());
            }

            Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    MessageTextBox.Text = string.Empty;
                    MessageTextBox.IsEnabled = false;
                    MessageTextBox.IsEnabled = true;
                    GlobalIndicator.Instance.Text = string.Empty;
                    GlobalIndicator.Instance.IsLoading = false;

                    _UpdateApplicationBar();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("_MessageSent->last step of Updating UI failed in ChatPage: " + ex.Message);
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Request string to send attachments with API method messages.Send.</returns>
        private void _UploadCurrentPhotoAttachments()
        {
            if (App.Current.EntityService.AttachmentPhotos.Count == 0)
                _UploadCompleted();
            else
            {
                //////////////////////////////////// GET UPLOAD SERVER
                if (string.IsNullOrEmpty(_uploadServer))
                {
                    var getUploadServer = new PhotosGetMessagesUploadServer(uploadServer =>
                    {
                        _uploadServer = uploadServer;
                        _UploadCurrentPhotoAttachments();
                    });
                    getUploadServer.Execute();
                }
                else
                {
                    foreach (var photo in App.Current.EntityService.AttachmentPhotos)
                    {
                        /////////////////////////// READ FILE DATA
                        var data = new byte[photo.Value.Length];
                        photo.Value.Read(data, 0, (int)photo.Value.Length);

                        if (data != null)
                        {
                            //////////////////////////////////// SEND DATA
                            var client = new VKWebClient();
                            var handler = new ManualResetEvent(false);

                            client.SendPhoto(_uploadServer, "photo", data, response =>
                            {
                                try
                                {
                                    SentPhoto sentPhoto = SerializeHelper.Deserialise<SentPhoto>(response);

                                    //////////////////////////////////// SAVE ATTACHMENT PHOTO
                                    PhotosSaveMessagesPhoto op = new PhotosSaveMessagesPhoto(sentPhoto.Server,
                                        sentPhoto.Photo, sentPhoto.Hash, att =>
                                        {
                                            if (att != null)
                                            {
                                                //////////////////////////////////// REMEMBER ATTACHMENT ID
                                                _currentAttachmentsRequest += att.Id.ToString() + ",";
                                            }
                                            handler.Set();
                                        });
                                    op.Execute();
                                }
                                catch (Exception ex)
                                {
                                    _UploadCurrentPhotoAttachments();
                                    _uploadServer = string.Empty;
                                    //handler.Set();
                                    Debug.WriteLine("Parse response from _SavePhotoAttach failed." + ex.Message);
                                }
                            });
                            handler.WaitOne();
                        }
                    }

                    //_uploadServer = string.Empty;
                    _UploadCompleted();
                }
            }
        }

        /// <summary>
        /// Uploading pictures completed...so we can start sending message.
        /// </summary>
        /// <param name="requestString">Contains uids of uploaded attachments.</param>
        private void _UploadCompleted()
        {
            try
            {
                string requestString = string.Empty;
                if (_currentAttachmentsRequest.Length > 0)
                    requestString = _currentAttachmentsRequest.Remove(_currentAttachmentsRequest.Length - 1, 1);

                MessagesSend send = new MessagesSend(_id, -1, _currentOutMessage,
                    requestString, App.Current.EntityService.MessagesToForward, string.Empty, 0,
                    App.Current.EntityService.AttachedLatitude,
                    App.Current.EntityService.AttachedLongitude, Guid.NewGuid().ToString(), _MessageSent);
                send.Execute();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("MessagesSend. failed and couched in UI! WTF! " + ex.Message);
            }
        }

        private void _SendSelectedMessage()
        {
            GlobalIndicator.Instance.Text = AppResources.SendingMessage;
            GlobalIndicator.Instance.IsLoading = true;

            Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    _currentAttachmentsRequest = string.Empty;
                    _UploadCurrentPhotoAttachments();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("UploadCurrentPhotoAttachments failed and couched in UI! WTF! " + ex.Message);
                }
            });
        }

        private void _RemoveCurrentDialog()
        {
            try
            {
                MessagesDeleteDialog op = null;

                // Calculate only first time before operation.
                if (_numberOfDialogDeleteRequests == 0)
                    _numberOfDialogDeleteRequests = (int)Math.Ceiling((double)_model.Messages.Count / 10000.0);

                if (_numberOfDialogDeleteRequests > 1)
                {
                    op = new MessagesDeleteDialog(_id, -1, isOk =>
                    {
                        _numberOfDialogDeleteRequests -= 1;
                        _RemoveCurrentDialog();
                    });
                }
                else
                {
                    op = new MessagesDeleteDialog(_id, -1, isOk =>
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            try
                            {
                                _numberOfDialogDeleteRequests = -1; // Not necessary since we destroy this object....but ok

                                App.Current.EntityService.DeleteDialog(_id);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("_RemoveCurrentDialog failed in ChatPage: " + ex.Message);
                            }

                            NavigationService.Navigate(new Uri(@"/Views/DialogsPage.xaml", UriKind.Relative));
                        });
                    });
                }
                op.Execute();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("RemoveDialog_Click failed:" + ex.Message);
            }
        }

        #endregion

        #region Private fields

        private int _id;
        private ChatViewModel _model;
        private string _currentAttachmentsRequest;
        private string _currentOutMessage;
        private AttachmentViewModel _currentTrack;

        // This button is need to be saved for only 1 purpose: if user clicked start on another track, we need
        // to reset old button state (from Pause -> to Play).
        private Button _currentPlayButton;

        private string _uploadServer;
        private int _numberOfDialogDeleteRequests = 0;

        #endregion
    }
}