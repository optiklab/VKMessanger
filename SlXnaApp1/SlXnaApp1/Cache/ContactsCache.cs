using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.Serialization.Json;
using SlXnaApp1.Entities;

namespace SlXnaApp1.Cache
{
    public class ContactsCache : ICollectionCache<PhoneContact>
    {
        #region Constructor

        public ContactsCache()
        {
            _Initialize();
        }

        ~ContactsCache()
        {
            _Save();
        }

        #endregion

        #region Public methods

        public IList<PhoneContact> GetItems()
        {
            return _contacts.OrderBy(x => x.FullName).ToList();
        }

        public void AddItem(PhoneContact item)
        {
            if (item == null)
                return;

            var contact = _contacts.FirstOrDefault(x => x.FullName == item.FullName &&
                x.VerifiedPhone == item.VerifiedPhone);

            if (contact == null)
            {
                _contacts.Add(item);
                //_Save();
            }
        }

        public void AddItems(IList<PhoneContact> items)
        {
            if (items == null)
                return;

            foreach (var contact in items)
            {
                _contacts.Add(contact);
            }

            //_Save();
        }

        public bool IsEmpty()
        {
            return !_contacts.Any();
        }

        public void RenewItem(PhoneContact item, PhoneContact new_item)
        {
        }

        public bool Clear()
        {
            bool result = false;

            IsolatedStorageFile filesystem = null;

            try
            {
                _contacts.Clear();

                filesystem = IsolatedStorageFile.GetUserStoreForApplication();
                filesystem.DeleteFile(CONTACTS_CACHE);

                result = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ContactsCache.Clear failed: " + ex.Message);
            }
            finally
            {
                if (filesystem != null)
                    filesystem.Dispose();
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

                if (!filesystem.FileExists(CONTACTS_CACHE))
                {
                    _contacts = new List<PhoneContact>();
                }
                else
                {
                    fs = new IsolatedStorageFileStream(CONTACTS_CACHE, FileMode.Open, filesystem);

                    var serializer = new DataContractJsonSerializer(typeof(List<PhoneContact>));
                    _contacts = serializer.ReadObject(fs) as List<PhoneContact>;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Cannot initialize contacts cache: " + ex.Message);
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
                fs = new IsolatedStorageFileStream(CONTACTS_CACHE, FileMode.Create, filesystem);

                var serializer = new DataContractJsonSerializer(typeof(IList<PhoneContact>));
                serializer.WriteObject(fs, _contacts);
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

        private const string CONTACTS_CACHE = "ContactsCache.contacts";

        #endregion

        #region Private fields

        private List<PhoneContact> _contacts;

        #endregion
    }
}
