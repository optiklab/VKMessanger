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
    public class MessagesCache : IDictionaryCache<int, Message>
    {
        #region Constructor

        public MessagesCache()
        {
            _Initialize();
        }

        ~MessagesCache()
        {
            _Save();
        }

        #endregion

        #region IDictionaryCache methods

        public IList<Message> GetItems(int id)
        {
            if (_messages.ContainsKey(id))
                return _messages[id].OrderBy(x => x.Date).ToList();

            return Enumerable.Empty<Message>().ToList();
        }

        public void AddItem(int id, Message item)
        {
            if (item == null)
                return;

            if (_messages.ContainsKey(id))
            {
                if (!_messages[id].Any(x => x.Mid == item.Mid))
                    _messages[id].Add(item);
            }
            else
            {
                var newMessages = new List<Message>();
                newMessages.Add(item);
                _messages.Add(id, newMessages);
            }

            //_Save();
        }

        public void AddItems(int id, IList<Message> items)
        {
            if (items == null)
                return;

            if (_messages.ContainsKey(id))
            {
                var currentMessages = _messages[id];

                var added = items.Where(i => !currentMessages.Any(c => c.Mid == i.Mid)).ToList();

                _messages[id].AddRange(added);
            }
            else
            {
                _messages.Add(id, items.ToList());
            }

            //_Save();
        }

        public IList<Message> AddItemsEx(int id, IList<Message> items)
        {
            if (items == null)
                return new List<Message>();

            List<Message> added = new List<Message>();
            if (_messages.ContainsKey(id))
            {
                var currentMessages = _messages[id];

                added = items.Where(i => !currentMessages.Any(c => c.Mid == i.Mid)).ToList();

                if (added.Any())
                    _messages[id].AddRange(added);
                else
                    return items;
            }
            else
            {
                added = items.ToList();
                _messages.Add(id, added);
            }

            //_Save();

            return added;
        }

        public void ReplaceItems(int id, IList<Message> items)
        {
            if (items == null)
                return;

            if (_messages.ContainsKey(id))
            {
                _messages[id].Clear();
                _messages[id].AddRange(items);
            }
            else
            {
                _messages.Add(id, items.ToList());
            }

            //_Save();
        }

        public bool Clear()
        {
            bool result = false;

            IsolatedStorageFile filesystem = null;

            try
            {
                _messages.Clear();

                filesystem = IsolatedStorageFile.GetUserStoreForApplication();
                filesystem.DeleteFile(MESSAGES_CACHE);

                result = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("MessagesCache.Clear failed: " + ex.Message);
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

        #region Public methods

        public Message GetItem(int mid)
        {
            foreach (var key in _messages.Keys)
            {
                var mes = _messages[key].FirstOrDefault(x => x.Mid == mid);

                if (mes != null)
                {
                    return mes;
                }
            }

            return null;
        }

        public bool RemoveItem(int mid)
        {
            bool result = false;

            foreach (var key in _messages.Keys)
            {
                var mes = _messages[key].FirstOrDefault(x => x.Mid == mid);

                if (mes != null)
                {
                    _messages[key].Remove(mes);
                    result = true;
                    break;
                }
            }

            return result;
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

                if (!filesystem.FileExists(MESSAGES_CACHE))
                {
                    _messages = new Dictionary<int, List<Message>>();
                }
                else
                {
                    fs = new IsolatedStorageFileStream(MESSAGES_CACHE, FileMode.Open, filesystem);

                    var serializer = new DataContractJsonSerializer(typeof(IDictionary<int, List<Message>>), _knownTypes);
                    _messages = serializer.ReadObject(fs) as IDictionary<int, List<Message>>;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Cannot initialize messages cache: " + ex.Message);
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
                fs = new IsolatedStorageFileStream(MESSAGES_CACHE, FileMode.Create, filesystem);

                var serializer = new DataContractJsonSerializer(typeof(IDictionary<int, List<Message>>), _knownTypes);
                serializer.WriteObject(fs, _messages);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Cannot save messages cache:" + ex.Message);
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

        private const string MESSAGES_CACHE = "MessagesCache.messages";

        #endregion

        #region Private fields

        private List<Type> _knownTypes = new List<Type> { typeof(List<Message>) };
        private IDictionary<int, List<Message>> _messages;

        #endregion
    }
}
