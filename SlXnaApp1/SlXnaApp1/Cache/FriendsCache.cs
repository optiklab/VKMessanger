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
    public class FriendsCache : ICollectionCache<Friend>
    {
        #region Constructor

        public FriendsCache()
        {
            _Initialize();
        }

        ~FriendsCache()
        {
            _Save();
        }

        #endregion

        #region Public methods

        public IList<Friend> GetItems()
        {
            return _friends.OrderBy(x => x.FullName).ToList();
        }

        public void AddItem(Friend item)
        {
            if (item == null)
                return;

            _friends.Add(item);

            //_Save();
        }

        public void AddItems(IList<Friend> items)
        {
            if (items == null)
                return;

            foreach (var friend in items)
            {
                _friends.Add(friend);
            }

            //_Save();
        }

        public void RenewItem(Friend item, Friend new_item)
        {
        }

        public bool IsEmpty()
        {
            return !_friends.Any();
        }

        public bool Clear()
        {
            bool result = false;

            IsolatedStorageFile filesystem = null;

            try
            {
                _friends.Clear();

                filesystem = IsolatedStorageFile.GetUserStoreForApplication();
                filesystem.DeleteFile(FRIENDS_CACHE);

                result = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("FriendsCache.Clear failed: " + ex.Message);
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
                var toDelete = _friends.FirstOrDefault(f => f.Uid == id);

                if (toDelete != null)
                {
                    _friends.Remove(toDelete);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("FriendsCache.RemoveItem failed: " + ex.Message);
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

                if (!filesystem.FileExists(FRIENDS_CACHE))
                {
                    _friends = new List<Friend>();
                }
                else
                {
                    fs = new IsolatedStorageFileStream(FRIENDS_CACHE, FileMode.Open, filesystem);

                    var serializer = new DataContractJsonSerializer(typeof(List<Friend>));
                    _friends = serializer.ReadObject(fs) as List<Friend>;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Cannot initialize friends cache: " + ex.Message);
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
                fs = new IsolatedStorageFileStream(FRIENDS_CACHE, FileMode.Create, filesystem);

                var serializer = new DataContractJsonSerializer(typeof(IList<Friend>));
                serializer.WriteObject(fs, _friends);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Cannot save friends cache:" + ex.Message);
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

        private const string FRIENDS_CACHE = "FriendsCache.friends";

        #endregion

        #region Private fields

        private List<Friend> _friends;

        #endregion
    }
}
