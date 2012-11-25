using System;
using System.Diagnostics;
using SlXnaApp1.Api;
using SlXnaApp1.Json;
using System.Windows;
using SlXnaApp1.Infrastructure;

namespace SlXnaApp1.Services
{
    public enum Modes
    {
        DontGetAttachments = 0,
        GetAttachments = 2
    }

    /// <summary>
    /// Service for taking long poll network connection.
    /// </summary>
    public class LongPollService : IApiService
    {
        #region Constructor

        public LongPollService()
        {
            _settings = new Settings(new ProtectDataAdapter());

            if (_settings.Ts > -1 &&
                !string.IsNullOrEmpty(_settings.LongPollServerKey) &&
                !string.IsNullOrEmpty(_settings.LongPollServerUri))
            {
                _info = new LongPollServerInfo
                {
                    Ts = _settings.Ts,
                    Key = _settings.LongPollServerKey,
                    Server = _settings.LongPollServerUri
                };
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Start point of the long polling server.
        /// </summary>
        public void Initialize()
        {
            _isOff = false;

            if (_info == null)
            {
                _RefreshServerInfo();
            }
            else
            {
                _GetUpdates();
            }
        }

        public void TurnOff()
        {
            _isOff = true;
            _info = null;
        }

        public void GetHistory(int max_msg_id)
        {
            _GetHistory(max_msg_id);
        }

        #endregion

        #region Private method

        private void _RefreshServerInfo()
        {
            var apiMethod = new MessagesGetLongPollServer(_ReconnectWithServerInfo);
            apiMethod.Execute();
        }

        private void _ReconnectWithServerInfo(LongPollServerInfo info)
        {
            if (_isOff)
                return;

            try
            {
                if (info == null)
                {
                    // Some error happened. User one of the algorithms:
                    // - Try push notifications?
                    // - Check network connections and make user notifications with suggestions.
                    // - Retry request after time out?
                    Debug.WriteLine("LongPollServer can't get server info, so we need to do something...");

                    // Get time outs

                    _RefreshServerInfo();
                }
                else
                {
                    _info = info;
                    _SaveInfo();

                    _GetUpdates();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("_ReconnectWithServerInfo failed: " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void _GetHistory(int max_msg_id)
        {
            try
            {
// TODO Error if more than 200 records
                var op = new MessagesGetLongPollHistory(_info.Ts, max_msg_id, _SaveHistory);
                op.Execute();
            }
            catch (ArgumentNullException)
            {
                _RefreshServerInfo();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("_GetHistory failed: " + ex.Message);
            }
        }

        private void _SaveHistory(LongPollHistoryResponse response)
        {

            if (response == null)
                Debug.WriteLine("_SaveHistory got successful response, but it is NULL");
            else
            {
                App.Current.UpdatesService.PutHistory(response.LongPollHistory);
            }
        }

        private void _GetUpdates()
        {
            if (_isOff)
                return;

            try
            {
                _request = new LongPollServerRequest(_info.Server, _info.Key, _info.Ts,
                    Modes.DontGetAttachments, _ConnectedToServer, _ConnectionFailed);
                _request.Execute();
            }
            catch (ArgumentNullException)
            {
                _RefreshServerInfo();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("_GetUpdates failed: " + ex.Message);

                _RefreshServerInfoAfterTimeOut();
            }
        }

        private void _ConnectedToServer(UpdatesResponse response)
        {
            if (_isOff)
                return;

            try
            {
                _request = null;

                if (response == null)
                {
                    Debug.WriteLine("LongPollServer got successful response, but it is NULL");
                }
                else
                {
                    _info.Ts = response.Ts;
                    _SaveInfo();

                    if (response.Updates != null && response.Updates.Count > 0)
                        App.Current.UpdatesService.PutUpdates(response.Updates);
                }

                _RefreshUpdates();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("_ConnectedToServer failed: " + ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        private void _ConnectionFailed(int errorCode)
        {
            _request = null;

            if (errorCode == 2)
            {
                // Need to update server info and reconnect
                _RefreshServerInfo();
            }
            else
            {
                Debug.WriteLine("LongPollServer can't get updates because of strange error with code " + errorCode.ToString() + ", but expected 2");

                _RefreshUpdates();
            }
        }

        private void _RefreshUpdates()
        {
            Debug.WriteLine("LongPollServer called _RefreshUpdates at " + DateTime.Now.ToLongTimeString());

            _GetUpdates();
        }

        private void _RefreshServerInfoAfterTimeOut()
        {
            Debug.WriteLine("LongPollServer refreshes server info after time out at " + DateTime.Now.ToLongTimeString());

            _RefreshServerInfo();
        }

        private void _SaveInfo()
        {
            if (_info != null &&
                _info.Ts > -1 &&
                !string.IsNullOrEmpty(_info.Key) &&
                !string.IsNullOrEmpty(_info.Server))
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        _settings.Ts = _info.Ts;
                        _settings.LongPollServerKey = _info.Key;
                        _settings.LongPollServerUri = _info.Server;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("_SaveInfo failed in LongPollService: " + ex.Message);
                    }
                });
            }
        }

        #endregion

        #region Private constants

        private const int  TIMEOUT = 30000;

        #endregion

        #region Private fields

        private LongPollServerInfo _info;
        private LongPollServerRequest _request;
        private Settings _settings;
        private bool _isOff = false;

        #endregion
    }
}
