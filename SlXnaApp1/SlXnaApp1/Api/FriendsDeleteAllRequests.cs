using System;
using SlXnaApp1.Json;
using System.Diagnostics;

namespace SlXnaApp1.Api
{
    /// <summary>
    /// Marks all friend requests as viewed.
    /// </summary>
    public class FriendsDeleteAllRequests : APIRequest
    {
        public FriendsDeleteAllRequests(Action<bool> callback)
            : base("friends.deleteAllRequests")
        {
            base.SetSuccessHandler(_ParseResponse);

            _callback = callback;
        }

        private void _ParseResponse(string response)
        {
            try
            {
                SuccessEntity success = SerializeHelper.Deserialise<SuccessEntity>(response);

                _callback(success.Succeed == 1 ? true : false);
            }
            catch (Exception)
            {
                Debug.WriteLine("Parse response from FriendsDeleteAllRequests failed.");
                _callback(false);
            }
        }

        private Action<bool> _callback;
    }
}
