using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Device.Location;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using SlXnaApp1.Cache;
using SlXnaApp1.Entities;
using SlXnaApp1.Infrastructure;

namespace SlXnaApp1.Views
{
    public partial class AttachmentsPage : PhoneApplicationPage
    {
        public AttachmentsPage()
        {
            InitializeComponent();

            _CreateBar();

            _model.CollectionChanged += _Model_CollectionChanged;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            try
            {
                _model.Clear();

                int count = App.Current.EntityService.AttachmentPhotos.Count;
                foreach (var item in App.Current.EntityService.AttachmentPhotos)
                {
                    var image = new BitmapImage(new Uri(item.Key, UriKind.Absolute));
                    //image.SetSource(item.Value);
                    _model.Add(new AttachmentViewModel(-1, -1, -1, AttachmentType.Photo,
                        item.Key, null, image, null, null));
                }

                if (_AddLocationAttachment())
                    count += 1;

                Header.Text = string.Format(AppResources.NumberAttachments, count);

                attachments.ItemsSource = _model;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("OnNavigatedTo in AttachmentsPage failed: " + ex.Message);
            }
        }

        private void _Model_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            int count = 0;
            foreach (var item in _model)
            {
                if (item.Type == AttachmentType.Location)
                {
                    // Set location button disabled. Only one location can be added.
                    ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = false;
                    //break;
                }
                count += 1;
            }

            Header.Text = string.Format(AppResources.NumberAttachments, count);
        }

        private void Image_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var avm = (sender as Image).DataContext as AttachmentViewModel;

            if (avm != null)
            {
                try
                {
                    // Remove location attachment
                    if (avm.Type == AttachmentType.Location)
                    {
                        App.Current.EntityService.AttachedLatitude = string.Empty;
                        App.Current.EntityService.AttachedLongitude = string.Empty;

                        AttachmentViewModel temp = null;
                        foreach (var item in _model)
                        {
                            if (item.Type == AttachmentType.Location)
                            {
                                temp = item;
                                break;
                            }
                        }

                        if (temp != null)
                            _model.Remove(temp);
                    }
                    // Remove photo attachment.
                    else
                    {
                        if (App.Current.EntityService.AttachmentPhotos.Remove(avm.Uri))
                        {
                            AttachmentViewModel temp = null;
                            foreach (var item in _model)
                            {
                                if (item.Uri == avm.Uri)
                                {
                                    temp = item;
                                    break;
                                }
                            }

                            if (temp != null)
                                _model.Remove(temp);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Image_Tap failed: " + ex.Message);
                }
            }
        }

        private bool _AddLocationAttachment()
        {
            if (!string.IsNullOrEmpty(App.Current.EntityService.AttachedLatitude) &&
                !string.IsNullOrEmpty(App.Current.EntityService.AttachedLongitude))
            {
                try
                {
                    string uri = String.Format(AppResources.GeolocationMapUriFormatMessage,
                        App.Current.EntityService.AttachedLatitude.Replace(",", "."),
                        App.Current.EntityService.AttachedLongitude.Replace(",", "."));

                    var avm = new AttachmentViewModel(-1, -1, -1, AttachmentType.Location, uri, null, null, null, null);
                    _model.Add(avm);

                    string filename = CommonHelper.DoDigest(uri);
                    var photosToLoad = new List<AvatarLoadItem>();
                    photosToLoad.Add(new AvatarLoadItem(1, uri, filename));

                    var service = new AsyncAvatarsLoader();

                    if (photosToLoad.Any())
                        service.LoadAvatars(photosToLoad.ToList(), id =>
                        {
                            Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                try
                                {
                                    ImageCache cache = new ImageCache();
                                    BitmapImage image = cache.GetItem(filename);

                                    if (image != null && image.PixelHeight > 0 && image.PixelWidth > 0)
                                    {
                                        avm.AttachPhoto = image;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine("_AddLocationAttachment failed in AttachmentsPage: " + ex.Message);
                                }
                            });
                        }, null);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Load attached location photo failed " + ex.Message);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Shows photo chooser
        /// </summary>
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

        /// <summary>
        /// Adds photo attachment
        /// </summary>
        private void photoGallery_Completed(object sender, PhotoResult e)
        {
            try
            {
                if (e.ChosenPhoto != null)
                {
                    if (!App.Current.EntityService.AttachmentPhotos.ContainsKey(e.OriginalFileName))
                    {
                        App.Current.EntityService.AttachmentPhotos.Add(e.OriginalFileName, e.ChosenPhoto);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("photoGallery_Completed failed and couched in UI! WTF! " + ex.Message);
            }
        }

        /// <summary>
        /// Trying to attach location.
        /// </summary>
        private void AddLocationButton_Click(object sender, EventArgs e)
        {
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

                            _AddLocationAttachment();
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Creates default bar.
        /// </summary>
        private void _CreateBar()
        {
            this.ApplicationBar = new ApplicationBar();

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
        }

        private ObservableCollection<AttachmentViewModel> _model = new ObservableCollection<AttachmentViewModel>();
    }
}