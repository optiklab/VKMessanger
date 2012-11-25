using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.Serialization.Json;
using SlXnaApp1.Json;

namespace SlXnaApp1.Cache
{
    public class DialogsCache : ICollectionCache<Dialog>
    {
        #region Constructor

        public DialogsCache()
        {
            _Initialize();
        }

        ~DialogsCache()
        {
            _Save();
        }

        #endregion

        #region Public methods

        public IList<Dialog> GetItems()
        {
            return _dialogs.OrderByDescending(x => x.Time).ToList();
        }

        public void AddItem(Dialog item)
        {
            if (item == null)
                return;

            _dialogs.Add(item);

            //_Save();
        }

        public void AddItems(IList<Dialog> items)
        {
            if (items == null)
                return;

            foreach (var dialog in items)
            {
                _dialogs.Add(dialog);
            }

            //_Save();
        }

        public void RenewItem(Dialog item, Dialog new_item)
        {
            try
            {
                _dialogs.Remove(item);
                _dialogs.Add(new_item);

                //_Save();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("RenewItem failed: " + ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        public bool IsEmpty()
        {
            return !_dialogs.Any();
        }

        public bool Clear()
        {
            bool result = false;

            IsolatedStorageFile filesystem = null;

            try
            {
                _dialogs.Clear();

                filesystem = IsolatedStorageFile.GetUserStoreForApplication();
                filesystem.DeleteFile(DIALOGS_CACHE);

                result = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DialogsCache.Clear failed: " + ex.Message);
            }
            finally
            {
                if (filesystem != null)
                    filesystem.Dispose();
            }

            return result;
        }

        public bool RemoveItem(int id)
        {
            bool result = false;

            try
            {
                var toDelete = _dialogs.FirstOrDefault(dialog => dialog.Uid == id && !dialog.IsConference);

                if (toDelete != null)
                {
                    _dialogs.Remove(toDelete);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DialogsCache.RemoveItem failed: " + ex.Message);
            }

            return result;
        }

        public void Save()
        {
            _Save();
        }

        #endregion

        #region Private methods

        private void _Initialize()
        {
            IsolatedStorageFile filesystem = null;
            IsolatedStorageFileStream fs = null;

            try
            {
                filesystem = IsolatedStorageFile.GetUserStoreForApplication();

                if (!filesystem.FileExists(DIALOGS_CACHE))
                {
                    _dialogs = new List<Dialog>();
                }
                else
                {
                    fs = new IsolatedStorageFileStream(DIALOGS_CACHE, FileMode.Open, filesystem);

                    var serializer = new DataContractJsonSerializer(typeof(List<Dialog>));
                    _dialogs = serializer.ReadObject(fs) as List<Dialog>;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Cannot initialize dialogs cache: " + ex.Message);
            }
            finally
            {
                if (filesystem != null)
                    filesystem.Dispose();

                if (fs != null)
                    fs.Dispose();
            }
        }

        private void _Save()
        {
            IsolatedStorageFile filesystem = null;
            IsolatedStorageFileStream fs = null;

            try
            {
                filesystem = IsolatedStorageFile.GetUserStoreForApplication();
                fs = new IsolatedStorageFileStream(DIALOGS_CACHE, FileMode.Create, filesystem);

                var serializer = new DataContractJsonSerializer(typeof(IList<Dialog>));
                serializer.WriteObject(fs, _dialogs);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Cannot save dialogs cache:" + ex.Message);
            }
            finally
            {
                if (filesystem != null)
                    filesystem.Dispose();

                if (fs != null)
                    fs.Dispose();
            }
        }

        #endregion

        #region Constants

        private const string DIALOGS_CACHE = "DialogsCache.dialogs";

        #endregion

        #region Private fields

        private List<Dialog> _dialogs;

        #endregion
    }
}
