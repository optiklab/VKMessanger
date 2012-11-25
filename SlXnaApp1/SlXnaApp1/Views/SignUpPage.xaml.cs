using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using SlXnaApp1.Api;
using SlXnaApp1.Infrastructure;
using System.Windows.Controls;
using SlXnaApp1.Json;
using System.Diagnostics;
using System.Windows.Media;

namespace SlXnaApp1.Views
{
    public partial class SignUpPage : Microsoft.Phone.Controls.PhoneApplicationPage
    {
        #region Constructor

        public SignUpPage()
        {
            InitializeComponent();

            _SwitchUIToStep1();

            _settings = new Settings(new ProtectDataAdapter());
        }

        #endregion

        #region Event handler

        private void SignUpButton_Tap(object sender, GestureEventArgs e)
        {
            string phone = LoginTextBox.Text;

            if (!_ShowValidationErrors())
            {
                _CheckPhone(phone);
            }
        }

        private void OkButton_Tap(object sender, GestureEventArgs e)
        {
            GlobalIndicator.Instance.IsLoading = true;

            _ConfirmSignUp();
        }

        private void RequestSMSButton_Tap(object sender, GestureEventArgs e)
        {
            GlobalIndicator.Instance.IsLoading = true;

            // Call sign up method again with filled additional parameter SID.
            _DoSignUp();
        }

        private void LastNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _SetSignUpButtonEnabling();
        }

        private void NameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _SetSignUpButtonEnabling();
        }

        private void LoginTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _SetSignUpButtonEnabling();
        }

        private void PasswordTextBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _SetSignUpButtonEnabling();
        }

        private void PasswordConfirmTextBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _SetSignUpButtonEnabling();
        }

        private void SmsCodeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SmsCodeTextBox.Text) &&
                !string.IsNullOrEmpty(LoginTextBox.Text))
            {
                OkButton.IsEnabled = true;
            }
            else
            {
                OkButton.IsEnabled = false;
            }
        }

        #endregion

        #region Private methods

        private void _SetSignUpButtonEnabling()
        {
            if (!string.IsNullOrEmpty(LastNameTextBox.Text) &&
                !string.IsNullOrEmpty(NameTextBox.Text) &&
                !string.IsNullOrEmpty(LoginTextBox.Text) &&
                !string.IsNullOrEmpty(PasswordTextBox.Password) &&
                !string.IsNullOrEmpty(PasswordConfirmTextBox.Password))
            {
                SignUpButton.IsEnabled = true;
            }
            else
            {
                SignUpButton.IsEnabled = false;
            }
        }

        private void _SwitchUIToStep1()
        {
            Step2Panel.Visibility = System.Windows.Visibility.Collapsed;
            Step1Panel.Visibility = System.Windows.Visibility.Visible;
        }

        private void _SwitchUIToStep2()
        {
            Step1Panel.Visibility = System.Windows.Visibility.Collapsed;
            Step2Panel.Visibility = System.Windows.Visibility.Visible;
        }

        private void _CheckPhone(string phone)
        {
            GlobalIndicator.Instance.IsLoading = true;
            SignUpButton.IsEnabled = false;

            var op = new AuthCheckPhone(phone, _PhoneIsOK);
            op.Execute();
        }

        private void _PhoneIsOK(bool isOk)
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                if (isOk)
                {
                    try
                    {
                        _DoSignUp();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("_PhoneIsOK failed in SignUpPage: " + ex.Message);
                    }
                }
                else
                {
                    LoginTextBox.Focus();

                    MessageBox.Show(AppResources.PhoneInUse);

                    GlobalIndicator.Instance.Text = string.Empty;
                    GlobalIndicator.Instance.IsLoading = false;
                    SignUpButton.IsEnabled = true;
                }
            });
        }

        private void _DoSignUp()
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                // Get values only once and don't do this on next retries.
                if (string.IsNullOrEmpty(_sid))
                {
                    _login = LoginTextBox.Text;
                    _name = NameTextBox.Text;
                    _lastname = LastNameTextBox.Text;
                    _password = PasswordConfirmTextBox.Password;
                }

                try
                {
// NOTE. Do not handle specially error 1112, cuz we already checked phone number, so probably error should not happen.
                    AuthSignUp op = new AuthSignUp(_login, _name, _lastname, _password, _sid, _WaitSms);
                    op.Execute();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("_DoSignUp failed:" + ex.Message);

                    this.Dispatcher.BeginInvoke(() =>
                    {
                        GlobalIndicator.Instance.Text = string.Empty;
                        GlobalIndicator.Instance.IsLoading = false;

                        _SetSignUpButtonEnabling();
                    });
                }
            });
        }

        private bool _ShowValidationErrors()
        {
            bool errors = false;

            if (PasswordTextBox.Password != PasswordConfirmTextBox.Password || PasswordTextBox.Password.Length < 6)
            {
                PasswordTextBox.Focus();

                MessageBox.Show(AppResources.WrongPassword);

                errors = true;
            }

            return errors;
        }

        private void _WaitSms(SidDataResponse sid)
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                if (sid != null && sid.SidData != null && !string.IsNullOrEmpty(sid.SidData.Sid))
                {
                    _sid = sid.SidData.Sid;

                    _SwitchUIToStep2();
                }
                else
                {
                    MessageBox.Show(AppResources.SignUpFailed);
                }

                GlobalIndicator.Instance.Text = string.Empty;
                GlobalIndicator.Instance.IsLoading = false;

                _SetSignUpButtonEnabling();
            });
        }

        private void _ConfirmSignUp()
        {
            string code = SmsCodeTextBox.Text;

            try
            {
                AuthConfirm op = new AuthConfirm(_login, _password, code, _FinishSignUp);
                op.Execute();
            }
            catch (Exception ex)
            {
                this.Dispatcher.BeginInvoke(() =>
                {
                    GlobalIndicator.Instance.Text = string.Empty;
                    GlobalIndicator.Instance.IsLoading = false;

                    _SetSignUpButtonEnabling();
                });

                Debug.WriteLine("_ConfirmSignUp failed:" + ex.Message);
            }
        }

        private void _FinishSignUp(int uid, bool isOK)
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                // Clear UI.
                LoginTextBox.Text = string.Empty;
                NameTextBox.Text = string.Empty;
                LastNameTextBox.Text = string.Empty;
                PasswordTextBox.Password = string.Empty;
                PasswordConfirmTextBox.Password = string.Empty;
                SmsCodeTextBox.Text = string.Empty;

                _SwitchUIToStep1();

                GlobalIndicator.Instance.Text = string.Empty;
                GlobalIndicator.Instance.IsLoading = false;

                _SetSignUpButtonEnabling();

                if (isOK && uid > -1)
                {
                    _settings.UserId = uid.ToString();
                    _settings.UserName = _login;
                    _settings.Password = _password;

                    NavigationService.Navigate(new Uri("/Views/AuthPage.xaml", UriKind.Relative));
                }
                else
                {
                    MessageBox.Show(AppResources.SignUpFailed);
                }

                // Clear state.
                _password = string.Empty;
                _login = string.Empty;
                _name = string.Empty;
                _lastname = string.Empty;
                _sid = string.Empty;
            });
        }

        #endregion

        #region Private fields

        private Settings _settings = null;
        private string _sid = null;
        private string _password = null;
        private string _login = null;
        private string _name = null;
        private string _lastname = null;

        #endregion
    }
}