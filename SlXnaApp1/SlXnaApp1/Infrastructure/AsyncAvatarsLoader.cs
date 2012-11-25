using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using SlXnaApp1.Cache;

namespace SlXnaApp1.Infrastructure
{
    public class AvatarLoadItem
    {
        public AvatarLoadItem(int uid, string uri, string filename)
        {
            Uid = uid;
            Uri = uri;
            FileName = filename;
        }

        public int Uid { get; set; }
        public string Uri { get; set; }
        public string FileName { get; set; }
    }

    /// <summary>
    /// Class is responsible for loading images and avatars from the web in sequence one by one.
    /// It saves pictures in storage with filename which was specified externally.
    /// </summary>
    public class AsyncAvatarsLoader
    {
        #region Public methods

        public void LoadAvatars(IList<AvatarLoadItem> itemsToLoad, Action<int> callback, Action finishCallback)
        {
            if (_isInProgress)
                throw new Exception("Loading avatars currently is in progress.");

            // Elements of this collection will be removed until loading images.
            _itemsToLoad = itemsToLoad.Select(x => x).ToList();

            _callback = callback;
            _finishCallback = finishCallback;

            _isEnoughSpace = true;
            _currentFileName = string.Empty;
            _currentFriendId = -1;
            _numberOfLoaded = 0;

            _client = new VKWebClient();

            _isInProgress = true;

            _LoadImages();
        }

        #endregion

        #region Private methods

        private void _LoadImages()
        {
            try
            {
                var item = _itemsToLoad.FirstOrDefault();

                if (item != null && !string.IsNullOrEmpty(item.Uri) && !string.IsNullOrEmpty(item.FileName) && _isEnoughSpace)
                {
                    _currentFriendId = item.Uid;
                    _currentFileName = item.FileName;

                    if (!item.Uri.EndsWith(".gif")) // We can't correctly handle GIFs for now... let's think about it later
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                try
                                {
                                    _client.GetByteData(item.Uri, _GetResult);
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine("_LoadImages failed on AsyncAvatarsLoader: " + ex.Message);
                                    _GetResult(null);
                                }
                            });
                    }
                    else
                    {
                        _callback(_currentFriendId);
                        _LoadNext();
                    }
                }
                else
                    _FinishJob();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("_LoadImages failed: " + ex.Message);

                if (_itemsToLoad.Count >= 0)
                    _LoadNext();
                else
                    _FinishJob();
            }
        }

        private void _GetResult(Stream result)
        {
            if (result == null)
            {
                _LoadNext();
            }
            else
            {
                try
                {
                    _isEnoughSpace = CacheHelpers.GetMoreSpace(result.Length);

                    if (_isEnoughSpace)
                    {
                        lock (guard)
                        {
                            IsolatedStorageFile filesystem = null;
                            IsolatedStorageFileStream fs = null;
                            try
                            {
                                filesystem = IsolatedStorageFile.GetUserStoreForApplication();
                                string filename = _currentFileName + ".jpg";

                                if (!filesystem.FileExists(filename))
                                {
                                    fs = new IsolatedStorageFileStream(filename, FileMode.Create, filesystem);
                                    // Save the image to Isolated Storage
                                    Int64 imgLen = (Int64)result.Length;
                                    byte[] b = new byte[imgLen];
                                    result.Read(b, 0, b.Length);
                                    fs.Write(b, 0, b.Length);
                                    fs.Flush();
                                }

                                _numberOfLoaded++;
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("Save avatar in file system failed: " + ex.Message);
                            }
                            finally
                            {
                                if (filesystem != null)
                                    filesystem.Dispose();

                                if (fs != null)
                                    fs.Dispose();
                            }
                        }

                        _callback(_currentFriendId);

                        _LoadNext();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Image loading failed." + ex.Message);

                    _LoadNext();
                }
            }
        }

        private void _LoadNext()
        {
            try
            {
                lock (guard)
                {
                    // Don't try to load this image again.
                    var temp = _itemsToLoad.FirstOrDefault(x => x.Uid == _currentFriendId);
                    _itemsToLoad.Remove(temp);
                }

                _LoadImages();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("_LoadNext failed." + ex.Message);

                // Shit happens, go ahead.
                _LoadNext();
            }
        }

        private void _FinishJob()
        {
            _isEnoughSpace = true;
            _isInProgress = false;
            _currentFileName = string.Empty;
            _currentFriendId = -1;

            if (_finishCallback != null)
                _finishCallback();
        }

        #endregion

        #region Private fields

        private const int FILE_NAME_START_POSITION = 10;

        private bool _isInProgress;
        private bool _isEnoughSpace;
        private string _currentFileName;
        private int _currentFriendId;
        private int _numberOfLoaded;

        private VKWebClient _client;
        private IList<AvatarLoadItem> _itemsToLoad;
        private Action<int> _callback;
        private Action _finishCallback;

        private object guard = new object();

        #endregion
    }
}
