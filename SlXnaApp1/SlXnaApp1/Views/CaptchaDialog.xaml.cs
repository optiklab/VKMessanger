using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using SlXnaApp1.Infrastructure;
using System.IO;

namespace SlXnaApp1.Views
{
    public partial class CaptchaDialog : UserControl
    {
        public CaptchaDialog()
        {
            InitializeComponent();

            _LoadCaptcha(App.Current.APIErrorHandler.LastCaptchaImage);
        }

        #region Event handlers

        private void CaptchaTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(CaptchaTextBox.Text))
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

        private void _LoadCaptcha(string captchaImg)
        {
            try
            {
                if (!string.IsNullOrEmpty(captchaImg))
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        try
                        {
                            var client = new VKWebClient();
                            client.GetByteData(captchaImg, _ShowCaptcha);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("_LoadCaptcha failed in CaptchaPage: " + ex.Message);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("_LoadCaptcha failed " + ex.Message);
            }
        }

        private void _ShowCaptcha(Stream stream)
        {
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
    }
}
