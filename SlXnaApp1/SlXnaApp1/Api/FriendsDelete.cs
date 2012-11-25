using System;
using SlXnaApp1.Json;
using System.Diagnostics;

namespace SlXnaApp1.Api
{
    /// <summary>
    /// If user id is in the list of requested friends: denies user's request to friends of current user friends.
    /// Otherwise just removes user from friends of current user.
    /// </summary>
    public class FriendsDelete : APIRequest
    {
        public FriendsDelete(int uid, Action<int> callback)
            : base("friends.delete")
        {
            base.AddParameter("uid", uid.ToString());

            base.SetSuccessHandler(_ParseResponse);

            _callback = callback;
        }

        private void _ParseResponse(string response)
        {
            try
            {
                // 1 - user deleted
                // 2 - request to friends denied
                int code = SerializeHelper.Deserialise<int>(response);
                _callback(code);
            }
            catch (Exception)
            {
                Debug.WriteLine("Parse response from FriendsDelete failed.");
                _callback(-1);
            }
        }

        private Action<int> _callback;
    }
}
