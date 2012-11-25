using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows.Media.Imaging;
using System.Windows;

namespace SlXnaApp1.Cache
{
    public class ImageCache
    {
        #region Constructor

        public ImageCache()
        {
            _images = new Dictionary<string, BitmapImage>();
        }

        #endregion

        #region Public methods

        public BitmapImage GetItem(string key)
        {
            BitmapImage image = null;

            if (string.IsNullOrEmpty(key))
                return image;

            if (_images.ContainsKey(key))
                image = _images[key];
            else
            {
                // Try to load from file system.
                image = _LoadImage(key);

                // Store in current cache collection.
                if (image != null)
                    _images.Add(key, image);
            }

            return image;
        }

        public bool Clear()
        {
            bool result = false;

            IsolatedStorageFile filesystem = null;

            try
            {
                filesystem = IsolatedStorageFile.GetUserStoreForApplication();

                foreach (var image in _images)
                {
                    try
                    {
                        filesystem.DeleteFile(image.Key + ".jpg");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Delete image by key failed: " + ex.Message);
                    }
                }

                _images.Clear();

                result = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ImageCache.Clear failed: " + ex.Message);
            }
            finally
            {
                if (filesystem != null)
                    filesystem.Dispose();
            }

            return result;
        }

        #endregion

        #region Private methods

        private BitmapImage _LoadImage(string filename)
        {
            BitmapImage image = null;
            IsolatedStorageFile filesystem = null;
            IsolatedStorageFileStream fs = null;
            try
            {
                filesystem = IsolatedStorageFile.GetUserStoreForApplication();
                filename = filename + ".jpg";
                if (filesystem.FileExists(filename))
                {
                    fs = new IsolatedStorageFileStream(filename, FileMode.Open, filesystem);
 
                    image = new BitmapImage();

                    image.SetSource(fs);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("_LoadImage error: " + ex.Message);
            }
            finally
            {
                if (filesystem != null)
                    filesystem.Dispose();

                if (fs != null)
                    fs.Dispose();
            }

            return image;
        }

        #endregion

        #region Private fields

        private IDictionary<string, BitmapImage> _images;

        #endregion
    }
}
