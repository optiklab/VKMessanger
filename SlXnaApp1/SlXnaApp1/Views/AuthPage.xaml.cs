using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using SlXnaApp1.Api;
using SlXnaApp1.Infrastructure;
using SlXnaApp1.Json;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows.Navigation;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Phone.Shell;
using System.Windows.Data;
using System.ComponentModel;

namespace SlXnaApp1.Views
{
    public partial class AuthPage : Microsoft.Phone.Controls.PhoneApplicationPage
    {
        #region Constructor

        public AuthPage()
        {
            InitializeComponent();

            _settings = new Settings(new ProtectDataAdapter());

            if (!WindowsPhoneHelpers.IsAnyNetworkAvailable())
            {
                NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;
                SignInButton.IsEnabled = false;
                SignUpButton.IsEnabled = false;
            }
            else
            {
                //if (!string.IsNullOrEmpty(_settings.AccessToken))
                if (!string.IsNullOrEmpty(_settings.UserName) & !string.IsNullOrEmpty(_settings.Password))
                {
                    App.Current.IsFirstRun = false; //always false

                    // To make less work during checking and getting values, save settings in a local variables.
                    string userName = _settings.UserName;
                    string password = _settings.Password;

                    if (string.IsNullOrEmpty(_settings.Secret) || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                    {
                        // Internal error message show... and continue work
                    }
                    else
                    {
                        LoginTextBox.Text = userName;
                        PasswordTextBox.Password = password;

                        _Authorize(userName, password, null);
                    }
                }
            }
        }

        #endregion

        #region Event handlers

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _ClearBackHistory();
        }

        private void _ClearBackHistory()
        {
            try
            {
                // Clear back stack history.
                while (this.NavigationService.BackStack.Any())
                {
                    this.NavigationService.RemoveBackEntry();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("_ClearBackHistory failed: " + ex.Message);
            }
        }

        void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
        {
            if (WindowsPhoneHelpers.IsAnyNetworkAvailable())
            {
                NetworkChange.NetworkAddressChanged -= NetworkChange_NetworkAddressChanged;
                SignInButton.IsEnabled = true;
                SignUpButton.IsEnabled = true;
            }
        }

        private void SignInButton_Tap(object sender, GestureEventArgs e)
        {
            string login = LoginTextBox.Text;
            string password = PasswordTextBox.Password;
            string key = CaptchaTextBox.Text;

            _Authorize(login, password, key);
        }

        private void LoginTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            _SetSignInButtonEnabling();
        }

        private void PasswordTextBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _SetSignInButtonEnabling();
        }

        private void SignUpButton_Tap(object sender, GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/SignUpPage.xaml", UriKind.Relative));
        }

        #endregion

        #region Private methods

        private void _SetSignInButtonEnabling()
        {
            if (WindowsPhoneHelpers.IsAnyNetworkAvailable() && // TODO Remove this line after testing
                !string.IsNullOrEmpty(LoginTextBox.Text) &&
                !string.IsNullOrEmpty(PasswordTextBox.Password))
            {
                SignInButton.IsEnabled = true;
            }
            else
            {
                SignInButton.IsEnabled = false;
            }
        }

        private void _Authorize(string login, string password, string captcha_key)
        {
            GlobalIndicator.Instance.Text = AppResources.Connecting;
            GlobalIndicator.Instance.IsLoading = true;

            SignInButton.IsEnabled = false;
            SignUpButton.IsEnabled = false;

            _settings.Password = password;
            _settings.UserName = login;

            Authorizer auth = new Authorizer();

            if (!string.IsNullOrEmpty(captcha_key))
                auth.Authorize(login, password, _captchaSid, captcha_key, _SignInResult);
            else
                auth.Authorize(login, password, null, null, _SignInResult);
        }

        private void _SignInResult(ErrorCode code, Dictionary<string, string> values)
        {
            if (code == ErrorCode.AUTH_SUCCESS)
            {
                this.Dispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        string userId;
                        if (values.TryGetValue("user_id", out userId))
                            _settings.UserId = userId;

                        string accessToken;
                        if (values.TryGetValue("access_token", out accessToken))
                            _settings.AccessToken = accessToken;

                        string secret;
                        if (values.TryGetValue("secret", out secret))
                            _settings.Secret = secret;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("_SignInResult failed in AuthPage: " + ex.Message);
                    }
                });

                if (_settings.IsPushOn)
                    App.Current.PushNotifications.SwitchOn();

                App.Current.EntityService.Initialize();

                // Since we don't need GetHistory, let's initialize LongPoll after initialize entity service.
                App.Current.LongPollService.Initialize();

                this.Dispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        SignInButton.IsEnabled = true;
                        SignUpButton.IsEnabled = true;
                        GlobalIndicator.Instance.Text = string.Empty;
                        GlobalIndicator.Instance.IsLoading = false;
                        NavigationService.Navigate(new Uri("/Views/DialogsPage.xaml", UriKind.Relative));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("_SignInResult failed in AuthPage: " + ex.Message);
                    }
                });

                App.Current.EntityService.LoadAvatars();
            }
            else if (code == ErrorCode.NEED_CAPTCHA)
            {
                this.Dispatcher.BeginInvoke(() =>
                {
                    GlobalIndicator.Instance.IsLoading = false;

                    string sid;
                    if (values.TryGetValue("captcha_sid", out sid))
                        _captchaSid = sid;

                    string image;
                    if (values.TryGetValue("captcha_img", out image))
                        _captchaImg = image;

                    _LoadCaptcha();
                });
            }
            else
            {
                this.Dispatcher.BeginInvoke(() =>
                {
                    GlobalIndicator.Instance.Text = string.Empty;
                    GlobalIndicator.Instance.IsLoading = false;

                    MessageBox.Show(AppResources.ErrorAuthorization);
                });
            }

            this.Dispatcher.BeginInvoke(() =>
            {
                SignInButton.IsEnabled = true;
                SignUpButton.IsEnabled = true;
            });
        }

        private void _LoadCaptcha()
        {
            try
            {
                if (!string.IsNullOrEmpty(_captchaImg))
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        try
                        {
                            var client = new VKWebClient();
                            client.GetByteData(_captchaImg, _ShowCaptcha);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("_LoadCaptcha failed in AuthPage: " + ex.Message);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("_LoadCaptcha failed " + ex.Message);

                MessageBox.Show(AppResources.InternalApiError);
            }
        }

        private void _ShowCaptcha(Stream stream)
        {
            CaptchaImage.Visibility = System.Windows.Visibility.Visible;
            CaptchaTextBoxLabel.Visibility = System.Windows.Visibility.Visible;
            CaptchaTextBox.Visibility = System.Windows.Visibility.Visible;

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    BitmapImage image = new BitmapImage();
                    image.SetSource(stream);

                    CaptchaImage.Source = image;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("_ShowCaptcha failed " + ex.Message);
                }
            });
        }

        #endregion

        #region Private fields

        private Settings _settings = null;
        private string _captchaSid = null;
        private string _captchaImg = null;

        #endregion
    }
}