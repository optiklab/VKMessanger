using System;
using System.IO;
using Microsoft.Phone.Notification;
using SlXnaApp1.Api;

namespace SlXnaApp1.Services
{
    public class PushNotifications
    {
        #region Public methods

        public void SwitchOn()
        {
            _channel = HttpNotificationChannel.Find(CHANNEL_NAME);
            if (_channel == null)
            {
                _channel = new HttpNotificationChannel(CHANNEL_NAME);
                _channel.Open();

                if (App.Current.IsToastOn)
                    TurnOnToasts();
            }
            else
            {
                string channelUri = _channel.ChannelUri.ToString();

                _Register(channelUri);
            }

            _channel.ShellToastNotificationReceived += _ChannelToastNotificationReceived;
            _channel.HttpNotificationReceived += _ChannelNotificationReceived;
            _channel.ChannelUriUpdated += _ChannelUriUpdated;
            _channel.ErrorOccurred += _ChannelErrorOccurred;
        }

        public void SwitchOff()
        {
            _Unregister();

            _channel.ShellToastNotificationReceived -= _ChannelToastNotificationReceived;
            _channel.HttpNotificationReceived -= _ChannelNotificationReceived;
            _channel.ChannelUriUpdated -= _ChannelUriUpdated;
            _channel.ErrorOccurred -= _ChannelErrorOccurred;
        }

        /// <summary>
        /// Start listening Toasts.
        /// </summary>
        public void TurnOnToasts()
        {
            try
            {
                _channel.BindToShellToast();
            }
            catch (Exception)
            {
            }
        }

        public void TurnOffToasts()
        {
            try
            {
                _channel.UnbindToShellToast();
            }
            catch (Exception)
            {
            }
        }

        #endregion

        #region Public properties

        public string ChannelUri
        {
            get
            {
                return _channel.ChannelUri.ToString();
            }
        }

        #endregion

        #region Public methods

        private void _ChannelToastNotificationReceived(object sender, NotificationEventArgs e)
        {
            // TODO Handle e.Collection
        }

        private void _ChannelNotificationReceived(object sender, HttpNotificationEventArgs e)
        {
            string message;
            using (StreamReader reader = new StreamReader(e.Notification.Body))
            {
                message = reader.ReadToEnd();
            }
        }

        private void _ChannelUriUpdated(object sender, NotificationChannelUriEventArgs e)
        {
            string channelUri = e.ChannelUri.ToString();

            _Register(channelUri);
        }

        private void _ChannelErrorOccurred(object sender, NotificationChannelErrorEventArgs e)
        {
            /*
                обрабатываем ошибку
            */
        }

        private void _Register(string channelUri)
        {
            var op = new AccountRegisterDevice(channelUri, isOk =>
            {
                if (isOk)
                {
                }
            });
            op.Execute();
        }

        private void _Unregister()
        {
            var op = new AccountUnregisterDevice(_channel.ChannelUri.ToString(), isOk =>
            {
                if (isOk)
                {
                }
            });
            op.Execute();
        }

        #endregion

        #region Private fields

        private const string CHANNEL_NAME = "ViKey";//"VK_Messanger";

        private HttpNotificationChannel _channel;

        #endregion
    }
}
