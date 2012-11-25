using System;
using SlXnaApp1.Json;
using System.Diagnostics;

namespace SlXnaApp1.Api
{
    /// <summary>
    /// Receives list of contacts for searching registered friends using
    /// method "friends.getSuggestions".
    /// </summary>
    public class AccountImportContacts : APIRequest
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="contacts">List of comma-delimited phones and
        /// emails of friends of current user.</param>
        /// <param name="callback"></param>
        public AccountImportContacts(string contacts, Action<int> callback)
            : base("account.importContacts")
        {
            base.AddParameter("contacts", contacts);

            base.SetSuccessHandler(_ParseResponse);

            _callback = callback;
        }

        private void _ParseResponse(string response)
        {
            try
            {
                // Always 1.
                int code = SerializeHelper.Deserialise<int>(response);

                _callback(code);
            }
            catch (Exception)
            {
                Debug.WriteLine("Parse response from AccountImportContacts failed.");
                _callback(-1);
            }
        }

        private Action<int> _callback;
    }
}
