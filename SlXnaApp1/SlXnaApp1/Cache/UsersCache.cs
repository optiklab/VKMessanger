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
    public class UsersCache : ICollectionCache<UserInfo>
    {
        #region Constructor

        public UsersCache()
        {
            _Initialize();
        }

        ~UsersCache()
        {
            _Save();
        }

        #endregion

        #region Public methods

        public IList<UserInfo> GetItems()
        {
            return _users.OrderBy(x => x.FullName).ToList();
        }

        public void AddItem(UserInfo item)
        {
            if (item == null)
                return;

            _users.Add(item);

            //_Save();
        }

        public void AddItems(IList<UserInfo> items)
        {
            if (items == null)
                return;

            foreach (var friend in items)
            {
                _users.Add(friend);
            }

            //_Save();
        }

        public void RenewItem(UserInfo item, UserInfo new_item)
        {
        }

        public bool IsEmpty()
        {
            return !_users.Any();
        }

        public bool Clear()
        {
            bool result = false;

            IsolatedStorageFile filesystem = null;

            try
            {
                _users.Clear();

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
                var toDelete = _users.FirstOrDefault(f => f.Uid == id);

                if (toDelete != null)
                {
                    _users.Remove(toDelete);
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
                    _users = new List<UserInfo>();
                }
                else
                {
                    fs = new IsolatedStorageFileStream(FRIENDS_CACHE, FileMode.Open, filesystem);

                    var serializer = new DataContractJsonSerializer(typeof(List<UserInfo>));
                    _users = serializer.ReadObject(fs) as List<UserInfo>;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Cannot initialize users cache: " + ex.Message);
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

                var serializer = new DataContractJsonSerializer(typeof(IList<UserInfo>));
                serializer.WriteObject(fs, _users);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Cannot save users cache:" + ex.Message);
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

        private const string FRIENDS_CACHE = "UsersCache.users";

        #endregion

        #region Private fields

        private List<UserInfo> _users;

        #endregion
    }
}
