using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using SlXnaApp1.Api;
using SlXnaApp1.Infrastructure;
using SlXnaApp1.Json;

namespace SlXnaApp1.Views
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            try
            {
                if (App.Current.EntityService.CurrentUser != null)
                {
                    ProfilePhoto.Source = App.Current.EntityService.CurrentUser.Photo;
                    ProfileName.Text = App.Current.EntityService.CurrentUser.FullName;
                    App.Current.EntityService.CurrentUser.PropertyChanged += CurrentUser_PropertyChanged;
                }

                VibrationCheckbox.IsChecked = App.Current.IsVibrationOn;
                SoundCheckbox.IsChecked = App.Current.IsSoundOn;
                ToastCheckbox.IsChecked = App.Current.IsToastOn;

                if (!App.Current.IsPushOn)
                {
                    SoundNotificationsTime.Visibility = System.Windows.Visibility.Visible;
                    CancelButton.Visibility = System.Windows.Visibility.Visible;
                    SoundNotificationsTime.Text = string.Format(AppResources.SoundNotificationsDisabledUntilFormatMessage,
                        App.Current.PushTurnOnTime.ToShortTimeString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("OnNavigatedTo in SettingsPage failed: " + ex.Message);
            }
        }

        void CurrentUser_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ProfilePhoto.Source = App.Current.EntityService.CurrentUser.Photo;
            ProfileName.Text = App.Current.EntityService.CurrentUser.FullName;
        }

        private void SoundCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            App.Current.IsSoundOn = true;
        }

        private void SoundCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            App.Current.IsSoundOn = false;
        }

        private void VibrationCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            App.Current.IsVibrationOn = false;
        }

        private void VibrationCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            App.Current.IsVibrationOn = false;
        }

        private void ToastCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            App.Current.IsToastOn = true;

            App.Current.PushNotifications.TurnOnToasts();
        }

        private void ToastCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            App.Current.IsToastOn = false;

            App.Current.PushNotifications.TurnOffToasts();
        }
        
        private void HourTimeOutButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            TimeSpan temp = new TimeSpan(1, 0, 0);//test: 0, 1, 0
            DateTime destination = DateTime.Now + temp;
            SoundNotificationsTime.Text = string.Format(AppResources.SoundNotificationsDisabledUntilFormatMessage,
                destination.ToShortTimeString());
            SoundNotificationsTime.Visibility = System.Windows.Visibility.Visible;
            CancelButton.Visibility = System.Windows.Visibility.Visible;

            App.Current.PushTurnOnTime = destination;
            App.Current.IsPushOn = false;

            try
            {
                var op = new AccountSetSilenceMode(App.Current.PushNotifications.ChannelUri, temp.Seconds, isOk => { });
                op.Execute();
                HourTimeOutButton.IsEnabled = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("AccountSetSilenceMode 1 hour failed: " + ex.Message);
            }

            App.Current.PushNotificationsSwitchTimer = new Timer(state =>
            {
                App.Current.IsPushOn = true;
                App.Current.PushNotificationsSwitchTimer.Dispose();

                if (SoundNotificationsTime != null && CancelButton != null)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        SoundNotificationsTime.Visibility = System.Windows.Visibility.Collapsed;
                        CancelButton.Visibility = System.Windows.Visibility.Collapsed;
                        HourTimeOutButton.IsEnabled = true;
                    });
                }
            }, null, Convert.ToInt32(temp.TotalMilliseconds), -1);
        }

        private void EightHoursTimeOutButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            TimeSpan temp = new TimeSpan(8, 0, 0);//8,0,0
            DateTime destination = DateTime.Now + temp;
            SoundNotificationsTime.Text = string.Format(AppResources.SoundNotificationsDisabledUntilFormatMessage,
                destination.ToShortTimeString());
            SoundNotificationsTime.Visibility = System.Windows.Visibility.Visible;
            CancelButton.Visibility = System.Windows.Visibility.Visible;

            App.Current.PushTurnOnTime = destination;
            App.Current.IsPushOn = false;

            try
            {
                var op = new AccountSetSilenceMode(App.Current.PushNotifications.ChannelUri, temp.Seconds, isOk => { });
                op.Execute();
                EightHoursTimeOutButton.IsEnabled = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("AccountSetSilenceMode 8 hours failed: " + ex.Message);
            }

            App.Current.PushNotificationsSwitchTimer = new Timer(state =>
            {
                App.Current.IsPushOn = true;
                App.Current.PushNotificationsSwitchTimer.Dispose();

                if (SoundNotificationsTime != null && CancelButton != null)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        SoundNotificationsTime.Visibility = System.Windows.Visibility.Collapsed;
                        CancelButton.Visibility = System.Windows.Visibility.Collapsed;
                        EightHoursTimeOutButton.IsEnabled = true;
                    });
                }
            }, null, Convert.ToInt32(temp.TotalMilliseconds), -1);
        }

        private void ChangePhotoButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (App.Current.EntityService.IsFullyInitialized)
            {
                App.Current.PhotoGallery.Completed += PhotoGallery_Completed;
                App.Current.PhotoGallery.Show();
            }
            else
            {
                MessageBox.Show(AppResources.AppIsLoadingItems);
            }
        }

        private void PhotoGallery_Completed(object sender, PhotoResult e)
        {
            try
            {
                if (e.ChosenPhoto != null)
                {
                    GlobalIndicator.Instance.Text = AppResources.UploadingPhoto;
                    GlobalIndicator.Instance.IsLoading = true;

                    //////////////////////////////////// GET UPLOAD SERVER
                    var getUploadServer = new PhotosGetProfileUploadServer(uploadServer =>
                    {
                        if (!string.IsNullOrEmpty(uploadServer))
                        {
                            /////////////////////////// READ FILE DATA
                            var data = new byte[e.ChosenPhoto.Length];
                            e.ChosenPhoto.Read(data, 0, (int)e.ChosenPhoto.Length);

                            if (data != null)
                            {
                                //////////////////////////////////// SEND DATA
                                var client = new VKWebClient();
                                var handler = new ManualResetEvent(false);

                                client.SendPhoto(uploadServer, "photo", data, response =>
                                {
                                    try
                                    {
                                        var sentPhoto = SerializeHelper.Deserialise<SentPhoto>(response);

                                        //////////////////////////////////// SAVE ATTACHMENT PHOTO
                                        var op = new PhotosSaveProfilePhoto(sentPhoto.Server,
                                            sentPhoto.Photo, sentPhoto.Hash, photo =>
                                            {
                                                if (photo != null)
                                                {
                                                    //////////////////////////////////// RELOAD AVATAR
                                                    App.Current.EntityService.CurrentUser.UserInfo.Photo = photo.SourceSmall;
                                                    App.Current.EntityService.CurrentUser.UserInfo.PhotoMedium = photo.Source;
                                                    App.Current.EntityService.CurrentUser.UserInfo.PhotoBig = photo.SourceBig;
                                                    App.Current.EntityService.LoadAvatars();
                                                }
                                                else
                                                {
                                                    MessageBox.Show(AppResources.UploadPhotoError);
                                                }

                                                handler.Set();
                                            });
                                        op.Execute();
                                    }
                                    catch (Exception)
                                    {
                                        Debug.WriteLine("Parse response from PhotosSaveMessagesPhoto failed.");
                                        handler.Set();
                                    }
                                });
                                handler.WaitOne();

                                Dispatcher.BeginInvoke(() =>
                                {
                                    GlobalIndicator.Instance.Text = string.Empty;
                                    GlobalIndicator.Instance.IsLoading = false;
                                });
                            }
                            else
                            {
                                Dispatcher.BeginInvoke(() =>
                                {
                                    GlobalIndicator.Instance.Text = string.Empty;
                                    GlobalIndicator.Instance.IsLoading = false;
                                });
                            }
                        }
                    });
                    getUploadServer.Execute();
                }
            }
            catch (Exception ex)
            {
                GlobalIndicator.Instance.Text = string.Empty;
                GlobalIndicator.Instance.IsLoading = false;
                Debug.WriteLine("PhotoGallery_Completed failed and couched in UI! WTF! " + ex.Message);
            }
        }

        private void CancelButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SoundNotificationsTime.Visibility = System.Windows.Visibility.Collapsed;
            CancelButton.Visibility = System.Windows.Visibility.Collapsed;

            App.Current.IsPushOn = true;
            App.Current.PushNotificationsSwitchTimer.Dispose();

            try
            {
                var op = new AccountSetSilenceMode(App.Current.PushNotifications.ChannelUri, 0, isOk => { });
                op.Execute();
                HourTimeOutButton.IsEnabled = true;
                EightHoursTimeOutButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("CancelButton_Tap failed: " + ex.Message);
            }
        }

        private void LogOutButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            App.Current.EntityService.ClearAllData();

            // Go to Auth page
            NavigationService.Navigate(new Uri(@"/Views/AuthPage.xaml", UriKind.Relative));

            App.Current.IsFirstRun = true;
        }
    }
}