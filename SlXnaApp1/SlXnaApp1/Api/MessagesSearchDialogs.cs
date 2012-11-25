using System;
using System.Collections.Generic;
using SlXnaApp1.Json;
using System.Diagnostics;

namespace SlXnaApp1.Api
{
    public class MessagesSearchDialogs : APIRequest
    {
        public MessagesSearchDialogs(string query, Action<IList<SearchResult>> callback)
            : base("messages.searchDialogs")
        {
            base.AddParameter("q", query);
            base.AddParameter("fields", "first_name,last_name,photo_rec");

            base.SetSuccessHandler(_ParseResponse);

            _callback = callback;
        }

        private void _ParseResponse(string response)
        {
            try
            {
                SearchResponse searchResult = SerializeHelper.Deserialise<SearchResponse>(response);

                if (searchResult.SearchResults != null)
                    _callback(searchResult.SearchResults);
            }
            catch (Exception)
            {
                Debug.WriteLine("Parse response from MessagesSearchDialogs failed.");

                _callback(new List<SearchResult>());
            }
        }

        private Action<IList<SearchResult>> _callback;
    }
}
