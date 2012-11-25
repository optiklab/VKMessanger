using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Collections.Generic;
using SlXnaApp1.Json;

namespace SlXnaApp1.Api
{
    /// <summary>
    /// 
    /// </summary>
    public class FriendsGetSuggestions : APIRequest
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter">mutual, contacts, mutual_contacts. Default - all.</param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="callback"></param>
        public FriendsGetSuggestions(string filter, int offset, int count,
            Action<IList<Friend>> callback)
            : base("friends.getSuggestions")
        {
            // WARNING! Before this, "account.importContacts" should be called.
            Debug.Assert(false);

            if (!string.IsNullOrEmpty(filter))
                base.AddParameter("filter", filter); // default - all

            if (offset < 0 || count < 0)
                Debug.Assert(false);

            base.AddParameter("offset", offset.ToString());
            base.AddParameter("count", count.ToString());

            base.SetSuccessHandler(_ParseResponse);

            _callback = callback;
        }

        private void _ParseResponse(string response)
        {
            try
            {
                FriendsResponse friends = SerializeHelper.Deserialise<FriendsResponse>(response);

                if (friends.Friends != null)
                    _callback(friends.Friends);
            }
            catch (Exception)
            {
                Debug.WriteLine("Parse response from FriendsGetSuggestions failed.");

                _callback(new List<Friend>());
            }
        }

        private Action<IList<Friend>> _callback;
    }
}
